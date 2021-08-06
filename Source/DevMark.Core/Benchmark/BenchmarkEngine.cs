using DevMark.Core.SystemInformation;
using DevMark.Model.SystemInformation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace DevMark.Core.Engine
{

    public class BenchmarkEngine
    {
        private readonly SysInfoProvider _sysInfoProvider;
        private readonly CommandFactory _commandFactory;
        private readonly TestSuiteConfigurationProvider _configurationManager;
        private readonly BenchmarkEngineLogger _logger;

        public bool TraceLogging { get; set; }
        public bool BreakOnError { get; set; }
        public string WorkDir { get; set; }

        public BenchmarkEngine(CommandFactory commandFactory, BenchmarkEngineLogger logWriter, SysInfoProvider sysInfoProvider)
        {
            _commandFactory = commandFactory;
            _logger = logWriter;
            _configurationManager = new TestSuiteConfigurationProvider();
            _sysInfoProvider = sysInfoProvider;
            BreakOnError = true;
        }

        public BenchmarkEngineExecutionResult CheckTestSuiteFromFile(string path)
        {
            string testSuitYaml = File.ReadAllText(path);
            return CheckTestSuite(testSuitYaml);
        }

        public BenchmarkEngineExecutionResult CheckTestSuite(string testSuiteYaml)
        {
            var data = _configurationManager.Parse(testSuiteYaml);
            return CheckTestSuite(data);
        }

        public BenchmarkEngineExecutionResult CheckTestSuite(TestSuiteConfiguration data)
        {
            var results = new BenchmarkEngineExecutionResult();
            using (ScriptExecuter executer = new ScriptExecuter())
            {
                executer.InitializeShell();
                RunDependencyChecks(executer, results, data);
            }

            return results;
        }

        public BenchmarkEngineExecutionResult RunTestSuiteFromFile(string path, string[] categories)
        {
            string testSuitYaml = File.ReadAllText(path);
            return RunTestSuit(testSuitYaml, categories);
        }

        public BenchmarkEngineExecutionResult RunTestSuit(string testSuiteYaml, string[] categories)
        {
            var data = _configurationManager.Parse(testSuiteYaml);
            return RunTestSuit(data, categories);
        }

        public BenchmarkEngineExecutionResult RunTestSuit(TestSuiteConfiguration data, string[] categories)
        {
            var results = new BenchmarkEngineExecutionResult();
            using (ScriptExecuter executer = new ScriptExecuter())
            {
                executer.InitializeShell();

                if (RunDependencyChecks(executer, results, data))
                {
                    // We may want to use AssemblyLoadContext.EnterContextualReflection()
                    // here in the future. But for now we dont have any tests that leave
                    // assemblies in such context anyway.

                    if (RunTestSuitInitializers(executer, results, data))
                    {
                        RunTests(executer, results, data, categories);
                    }
                    RunTestSuitCleanup(executer, results, data);
                }
            }
            return results;
        }

        private bool RunTests(ScriptExecuter executer, BenchmarkEngineExecutionResult results, TestSuiteConfiguration data, string[] categories)
        {
            if (data.Tests != null)
            {
                foreach (var test in data.Tests)
                {
                    if (!categories.Contains(test.Category ?? "default"))
                    {
                        continue;
                    }

                    bool warmupFailed = false;
                    for (int i = 0; i < test.WarmupIterations; i++)
                    {
                        Thread.Sleep(test.InitializationDelay);
                        if (!RunTest(executer, results, data, test, BenchmarkScope.BenchmarkWarmup, i))
                        {
                            warmupFailed = true;
                            break;
                        }
                    }
                    if (!warmupFailed)
                    {
                        for (int i = 0; i < Math.Max(1, test.Iterations); i++)
                        {
                            Thread.Sleep(test.InitializationDelay);
                            if (!RunTest(executer, results, data, test, BenchmarkScope.Benchmark, i))
                                return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool RunDependencyChecks(ScriptExecuter executer, BenchmarkEngineExecutionResult results, TestSuiteConfiguration data)
        {
            if (data.Dependencies != null)
            {
                foreach (var command in data.Dependencies)
                {
                    ExecteCommand(data, executer, results, null, command, BenchmarkScope.Dependency);
                }
            }

            AddEnvironmentVariables(executer, results.Items.Where(x => x.Command is DependencyCommand && x.Command.Successfull).Select(x => (DependencyCommand)x.Command).ToList());

            return results.Success;
        }

        private bool RunTestSuitInitializers(ScriptExecuter executer, BenchmarkEngineExecutionResult results, TestSuiteConfiguration data)
        {
            if (data.Initializers != null)
            {
                foreach (var command in data.Initializers)
                {
                    if (!ExecteCommand(data, executer, results, null, command, BenchmarkScope.SuitInit))
                        return false;
                }
            }
            return true;
        }

        private bool RunTestSuitCleanup(ScriptExecuter executer, BenchmarkEngineExecutionResult results, TestSuiteConfiguration data)
        {
            if (data.Cleanup != null)
            {
                foreach (var command in data.Cleanup)
                {
                    if (!ExecteCommand(data, executer, results, null, command, BenchmarkScope.SuitCleanup))
                        return false;
                }
            }
            return true;
        }

        private bool RunTest(ScriptExecuter executer, BenchmarkEngineExecutionResult results, TestSuiteConfiguration data, TestConfiguration test, BenchmarkScope scope, int scopeIndex)
        {
            if (test.Initializers != null)
            {
                foreach (var command in test.Initializers)
                {
                    if (!ExecteCommand(data, executer, results, test, command, BenchmarkScope.TestInit))
                        return false;
                }
            }

            if (test.Benchmark != null)
            {
                foreach (var command in test.Benchmark)
                {
                    if (!ExecteCommand(data, executer, results, test, command, scope, scopeIndex))
                        return false;
                }
            }

            if (test.Cleanup != null)
            {
                foreach (var command in test.Cleanup)
                {
                    if (!ExecteCommand(data, executer, results, test, command, BenchmarkScope.TestCleanup))
                        return false;
                }
            }

            return true;
        }


        private bool ExecteCommand(TestSuiteConfiguration suiteConfiguration, ScriptExecuter executer, BenchmarkEngineExecutionResult results, TestConfiguration testConfiguration, CommandConfigurationBase configuration, BenchmarkScope scope, int scopeIndex = 0)
        {
            if (!IsSupportedPlatform(configuration))
                return true;

            var command = _commandFactory.CreateFromConfiguration(suiteConfiguration, null, configuration, new CommandEngineConfiguration { TraceLogging = TraceLogging });
            try
            {
                if (command.TraceLogging)
                {
                    // When trace logging is enabled we write everything directly as it appares.
                    // This may help us investigate issues when a command gets stuck.
                    command.StreamDataAdded += Command_StreamDataAdded;
                }

                if (configuration.ClearShell == true)
                {
                    executer.InitializeShell();
                }

                _logger.LogBeforeExecution(scope, suiteConfiguration, configuration, command);
                executer.RunScript(command);
                _logger.LogAfterExecution(scope, suiteConfiguration, configuration, command, command.TraceLogging);
            }
            catch (Exception e)
            {
                if (BreakOnError)
                {
                    throw;
                }
                _logger.LogHandeledException(scope, configuration, command, e);
            }
            finally
            {
                command.StreamDataAdded -= Command_StreamDataAdded;
            }

            results.Add(new BenchmarkResult { Command = command, SuiteConfig = suiteConfiguration, TestConfig = testConfiguration, CommandConfiguration = configuration, Scope = scope, ScopeIndex = scopeIndex });
            return command.Successfull;
        }

        private bool IsSupportedPlatform(CommandConfigurationBase configuration)
        {
            var cfg = configuration as CommandConfiguration;

            if (cfg == null)
                return true;

            var command = cfg.Git ?? cfg.Process ?? cfg.Shell ?? cfg.Write as BaseCommandConfiguration;

            if (command == null || command.Platform.HasValue == false || command.Platform == PlatformConfiguration.Any)
                return true;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return command.Platform == PlatformConfiguration.Windows;
            }


            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return command.Platform == PlatformConfiguration.Linux;
            }

            return false;
        }

        private void Command_StreamDataAdded(object sender, StreamObject e)
        {
            _logger.LogOutputStreamObject(e);
        }

        public SysInfo GetSysInfo()
        {
            _logger.LogBeforeSysInfo();
            var sysInfo = _sysInfoProvider.Get();
            UpdateWorkDirectoryDrive(sysInfo);
            _logger.LogAfterSysInfo();
            return sysInfo;
        }

        private void UpdateWorkDirectoryDrive(SysInfo sysInfo)
        {
            var workDrive = WorkDir;
            var workDriveDisk = sysInfo.Disks.FirstOrDefault(x => x.DrivePaths.Any(y => workDrive.StartsWith(y)));
            if (workDriveDisk != null)
            {
                workDriveDisk.ContainsWorkDirectory = true;
            }
        }

        private static void AddEnvironmentVariables(ScriptExecuter executer, IEnumerable<DependencyCommand> depCmds)
        {
            foreach (var depCmd in depCmds) {
                if (depCmd.EnvironmentVariables != null)
                {
                    foreach (var key in depCmd.EnvironmentVariables.Keys)
                    {
                        executer.EnvironmentVariables[key] = depCmd.EnvironmentVariables[key];
                    }
                }
            }
        }
    }
}
