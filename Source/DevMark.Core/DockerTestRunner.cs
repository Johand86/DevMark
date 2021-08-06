using DevMark.Core.Container;
using DevMark.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DevMark.Core
{
    public class DockerTestRunner
    {
        private readonly ILogger<DockerTestRunner> _logger;
        private readonly TestRunner _testRunner;
        private readonly ContainerEngine _containerEngine;
        private readonly TestSuiteConfigurationProvider _configurationProvider;

        public DockerTestRunner(TestRunner testRunner, ContainerEngine containerEngine, TestSuiteConfigurationProvider configurationProvider, ILogger<DockerTestRunner> logger)
        {
            _testRunner = testRunner;
            _containerEngine = containerEngine;
            _configurationProvider = configurationProvider;
            _logger = logger;
        }

        public void CheckSuites(string suggestedDockerfile, string[] testSuites, bool dockerWait, bool verbose, bool diagnostics)
        {
            var dockerConfiguration = _containerEngine.GetDockerConfig();
            if (!dockerConfiguration.Successfull || string.IsNullOrEmpty(dockerConfiguration.Version))
            {
                return;
            }

            int success = 0, failed = 0;
            foreach (var testSuite in testSuites)
            {
                var configuration = _configurationProvider.ParseFile(testSuite);

                var (dockerfile, imageUri, containerPlatform) = _containerEngine.ResolveImage(suggestedDockerfile, dockerConfiguration.Platform, configuration.Environment);

                if (string.IsNullOrEmpty(dockerfile) && string.IsNullOrEmpty(imageUri))
                {
                    failed++;
                    _logger.LogInformation($"No dockerfile or image found compatible with test \"{configuration.Name}\".");
                    continue;
                }

                success++;

                _logger.LogInformation($"Image/dockerfile \"{dockerfile}{imageUri}\" is compatible with test \"{configuration.Name}\".");

            }

            _logger.LogInformation($"Check dependencies completed, {success}/{success + failed} dependency checks were successful.");
            if (failed != 0)
            {
                _logger.LogInformation("Some tests may only run on a specific platform. On Windows you can change platform in the Docker Desktop application, or DockerCli.exe -SwitchWindowsEngine / -SwitchLinuxEngine");
            }
        }

        public async Task<(TestRun testRun, string resultPath)> RunSuites(string resultFileOrPath, string suggestedDockerfile, bool dockerWait, bool verbose, bool diagnostics, string[] testSuites, string categories)
        {
            string[] dockerTestSuites = _containerEngine.PrepareWorkDirectory(testSuites);

            var testRun = _testRunner.CreateTestRun();
            for (int i = 0; i < testSuites.Length; i++)
            {
                var testSuite = testSuites[i];
                var testSuitePath = dockerTestSuites[i];

                var configuration = _configurationProvider.ParseFile(testSuite);

                var dockerConfiguration = _containerEngine.GetDockerConfig();

                if (!dockerConfiguration.Successfull || string.IsNullOrEmpty(dockerConfiguration.Version))
                {
                    return (null, null);
                }

                var (dockerfile, imageUri, containerPlatform) = _containerEngine.ResolveImage(suggestedDockerfile, dockerConfiguration.Platform, configuration.Environment);

                if (string.IsNullOrEmpty(dockerfile) && string.IsNullOrEmpty(imageUri))
                {
                    _logger.LogInformation($"No suitable image found for \"{configuration.Name}\" using platform \"{dockerConfiguration.Platform}\", skipping...");

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        _logger.LogInformation("You can change between Windows and Linux platforms using the Docker Desktop application, or DockerCli.exe -SwitchWindowsEngine / -SwitchLinuxEngine");
                    }

                    continue;
                }

                string tag = await _containerEngine.Prepare(dockerfile, imageUri);

                if (await _containerEngine.HasRunningContainers())
                {
                    _logger.LogError("Other containers are running, this is not allowed when running benchmarks.");
                    continue;
                }

                _logger.LogInformation($"Running \"{configuration.Name}\" in docker image \"{tag}\" on \"{containerPlatform}\".");

                var containerRunResult = await _containerEngine.Run(tag, new string[] { testSuitePath }, categories, diagnostics, dockerWait, verbose, containerPlatform);

                if (containerRunResult.ResultPath != null)
                {
                    var containerTestRun = _testRunner.Load(containerRunResult.ResultPath);
                    var containerInfo = containerTestRun.SysInfo.Containers.Single();

                    containerInfo.LogicalProcessorsLimit = (int)containerRunResult.CPUs;
                    containerInfo.MemoryLimit = containerRunResult.Memory;

                    if (!_testRunner.Verify(containerTestRun))
                    {
                        throw new ApplicationException("Failed to verify file integrity");
                    }
                    _testRunner.Merge(containerTestRun, testRun);
                }
            }
            _testRunner.UpdateSysInfo(testRun);

            string resultPath = _testRunner.Write(resultFileOrPath, testRun);

            return (testRun, resultPath);
        }

    }
}
