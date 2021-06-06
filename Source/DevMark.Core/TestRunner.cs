using DevMark.Core.Engine;
using DevMark.Core.TestResult;
using DevMark.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevMark
{
    public class TestRunner
    {
        HashCalculator _hashCalculator;
        BenchmarkEngine _engine;
        TestResultFileProvider _fileProvider;
        TestResultCompilationProvider _testResultProvider;
        TestResultUploadProvider _testUploadProvider;
        ILogger<TestRunner> _logger;
        public TestRunner(BenchmarkEngine engine, TestResultFileProvider fileProvider, TestResultCompilationProvider testResultCompilationProvider, TestResultUploadProvider testUploadProvider, HashCalculator hashCalculator, ILogger<TestRunner> logger)
        {
            _engine = engine;
            _fileProvider = fileProvider;
            _testResultProvider = testResultCompilationProvider;
            _testUploadProvider = testUploadProvider;
            _logger = logger;
            _hashCalculator = hashCalculator;
        }

        public void CheckDependencies(string[] testSuites)
        {
            int success = 0, failed = 0;
            foreach (var suite in testSuites)
            {
                var results = _engine.CheckTestSuiteFromFile(suite);
                success += results.Items.Where(x => x.Command.Successfull).Count();
                failed += results.Items.Where(x => !x.Command.Successfull).Count();
            }

            _logger.LogInformation($"Check dependencies completed, {success}/{success+failed} dependency checks were successful.");
            if (failed != 0)
            {
                _logger.LogInformation("In order to run all tests, you'll have to install the dependencies mentioned above.");
                _logger.LogInformation("Make sure to close this window and open a new one when installing dependencies.");
            }
        }

        public async Task<UploadResult> Upload(TestRun testRun)
        {
            return await _testUploadProvider.Upload(testRun);
        }

        public (TestRun testRun, string resultPath) RunSuites(string[] testSuites, string[] categories, string resultPath)
        {
            var testRun = new TestRun();
            testRun.Version = GetVersion();
            testRun.Timestamp = DateTime.UtcNow;
            testRun.Id = Guid.NewGuid();

            var resultTestSuites = new List<TestSuite>();
            testRun.TestSuites = resultTestSuites;
            foreach (var suite in testSuites)
            {
                var results = _engine.RunTestSuiteFromFile(suite, categories);
                if (results.Success && results.Items.Where(x => x.Scope == BenchmarkScope.Benchmark).Count() >= 1)
                {
                    var resultTestSuite = _testResultProvider.Convert(results.Items);
                    resultTestSuites.Add(resultTestSuite);

                    var configHash = _hashCalculator.Calculate(results.Items.FirstOrDefault().SuiteConfig);
                    resultTestSuite.ConfigIdentifier = configHash;

                    var testSuiteHash = _hashCalculator.Calculate(resultTestSuite);
                    testRun.Verification.TestSuites.Add(testSuiteHash);
                }
            }

            var sysInfo = _engine.GetSysInfo();
            testRun.SysInfo = sysInfo;
            testRun.Verification.SysInfo = _hashCalculator.Calculate(sysInfo);

            string actualResultPath = _fileProvider.Save(testRun, resultPath);
            _logger.LogInformation($"Test result written to \"{actualResultPath}\".");

            return (testRun, actualResultPath);
        }

        public TestRun Load(string resultPath)
        {
            return _fileProvider.Load(resultPath);
        }

        public bool Verify(string resultPath)
        {
            return Verify(Load(resultPath));
        }

        public bool Verify(TestRun testRun)
        {
            string sysInfoHash = _hashCalculator.Calculate(testRun.SysInfo);

            if (sysInfoHash != testRun.Verification.SysInfo)
            {
                _logger.LogDebug($"SysInfo hash \"{sysInfoHash.Substring(0, 10)}...\" doesnt match the hash value of \"{testRun.Verification.SysInfo}\" found in the file.");
            }

            for (int i= 0; i < testRun.TestSuites.Count; i++)
            {
                if (testRun.Verification.TestSuites.Count < i)
                {
                    _logger.LogDebug($"No hash found for test suite with index {i}.");
                    return false;
                }

                var suite = testRun.TestSuites[i];
                var testSuiteHash = _hashCalculator.Calculate(suite);
                var verificationHash = testRun.Verification.TestSuites[i];
                if (testSuiteHash != verificationHash)
                {
                    _logger.LogDebug($"Calculated hash \"{testSuiteHash.Substring(0, 10)}...\" doesnt match the hash value of \"{verificationHash}\" found in the file.");
                    return false;
                }
            }
            return true;
        }

        private string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }
    }
}
