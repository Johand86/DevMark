using DevMark.Core.Execution;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;

namespace DevMark.Core.Engine
{
    public class BenchmarkEngineLogger : CommandLogger
    {

        public BenchmarkEngineLogger(ILogger<CommandLogger> logger) :base(logger)
        {
        }


        public string GetCommandScopeDiplayName(BenchmarkScope scope)
        {
            switch (scope)
            {
                case BenchmarkScope.SuitInit:
                    return "Test suite init";
                case BenchmarkScope.SuitCleanup:
                    return "Test suite cleanup";
                case BenchmarkScope.TestInit:
                    return "Init";
                case BenchmarkScope.TestCleanup:
                    return "Cleanup";
                case BenchmarkScope.Benchmark:
                    return "Benchmark";
                case BenchmarkScope.BenchmarkWarmup:
                    return "Warmup";
                case BenchmarkScope.Dependency:
                    return "Dependency check";
            }
            return "Unknown command";
        }

        public string GetLoggingDisplayName(CommandConfigurationBase configuration)
        {

            if (configuration is CommandConfiguration)
            {
                var config = configuration as CommandConfiguration;
                if (config.Git != null)
                {
                    if (config.Git.Name != null)
                        return config.Git.Name;
                    else
                        return $"git";
                }
                if (config.Shell != null)
                {
                    return config.Shell.Name ?? config.Shell.Cmd;
                }
                if (config.Process != null)
                {
                    return nameof(config.Process);
                }
                if (config.Write != null)
                {
                    return nameof(config.Write);
                }
            }
            if (configuration is DependencyCommandConfiguration)
            {
                var config = configuration as DependencyCommandConfiguration;
                
                if (config.Git != null)
                {
                    return nameof(config.Git);
                }
                if (config.NodeJS != null)
                {
                    return nameof(config.NodeJS);
                }
                if (config.VisualStudio != null)
                {
                    return nameof(config.VisualStudio);
                }
                if (config.DevMark != null)
                {
                    return nameof(config.DevMark);
                }
                if (config.Dotnet != null)
                {
                    return nameof(config.Dotnet);
                }
                if (config.DotnetFramework != null)
                {
                    return nameof(config.DotnetFramework);
                }
                if (config.Path != null)
                {
                    return nameof(config.Path);
                }
                if (config.ClearShell != null)
                {
                    return "ClearShell";
                }
            }
            return "Unknown command";
        }

        public void LogBeforeExecution(BenchmarkScope scope, TestSuiteConfiguration suiteConfiguration, CommandConfigurationBase configuration, CommandBase command)
        {
            string suiteDisplayName = suiteConfiguration.Name;
            string loggingIntendation = GetLoggingIntendation(scope);
            string loggingScope = GetCommandScopeDiplayName(scope);
            string loggingDisplayName = GetLoggingDisplayName(configuration);

            _logger.LogInformation($"{loggingIntendation}Starting  -- {loggingScope} -- {suiteDisplayName} -- {loggingDisplayName}");
        }

        public void LogAfterExecution(BenchmarkScope scope, TestSuiteConfiguration suiteConfiguration, CommandConfigurationBase configuration, CommandBase command, bool verbose)
        {
            if (!verbose)
            {
                // We do not print stream directly unless verbose mode is activated. Here we'll make surewarnings and errors are written.
                foreach (var streamObj in command.Streams.Where(x => x.Severity == StreamSeverty.Warn || x.Severity == StreamSeverty.Error))
                {
                    LogOutputStreamObject(streamObj);
                }
            }

            foreach (var streamObj in command.Pipeline)
            {
                LogOutputStreamObject(new StreamObject { Timestamp = DateTime.Now, Severity = StreamSeverty.Info, Message = streamObj });
            }

            string suiteDisplayName = suiteConfiguration.Name;
            string loggingIntendation = GetLoggingIntendation(scope);
            string loggingScope = GetCommandScopeDiplayName(scope);
            string loggingDisplayName = GetLoggingDisplayName(configuration);
            string loggingSuccessString = command.Successfull ? "" : " -- with errors";

            if (scope == BenchmarkScope.Benchmark && command.Duration.HasValue)
            {
                _logger.LogInformation($"{loggingIntendation}Completed -- {loggingScope} -- {suiteDisplayName} -- {loggingDisplayName} -- in {command.Duration.Value.TotalSeconds.ToString("0.##")}s {loggingSuccessString}");
            }
            else
            {
                _logger.LogInformation($"{loggingIntendation}Completed -- {loggingScope} -- {suiteDisplayName} -- {loggingDisplayName}{loggingSuccessString}");
            }

            if (!command.Successfull)
            {
                if (!verbose)
                {
                    _logger.LogInformation("A command failed to run, use --verbose to enable all logging.");
                }
                else
                {
                    try
                    {
                        _logger.LogTrace(JsonSerializer.Serialize(command));
                    }
                    catch (Exception)
                    {
                        try
                        {
                            var ex = command.Exception;
                            command.Exception = null;
                            _logger.LogTrace(JsonSerializer.Serialize(command));
                            _logger.LogTrace(ex.ToString());
                            command.Exception = ex;
                        }
                        catch (Exception)
                        {
                            _logger.LogTrace("Failed to log serialized command");
                        }
                    }
                }

            }
            if (command.Exception != null)
            {
                _logger.LogError(command.Exception, "An exception occured while running the command.");
            }

        }

        public void LogHandeledException(BenchmarkScope scope, CommandConfigurationBase configuration, CommandBase command, Exception e)
        {
            string loggingDisplayName = GetLoggingDisplayName(configuration);
            _logger.LogError(e, $"Exception during command \"{loggingDisplayName}\"");
        }

        private string GetLoggingIntendation(BenchmarkScope scope)
        {
            switch (scope)
            {
                case BenchmarkScope.SuitInit:
                case BenchmarkScope.SuitCleanup:
                    return "";
                case BenchmarkScope.TestInit:
                case BenchmarkScope.TestCleanup:
                    return "-- ";
                case BenchmarkScope.BenchmarkWarmup:
                case BenchmarkScope.Benchmark:
                    return "---- ";
            }
            return "";
        }

        public void LogBeforeSysInfo()
        {
            _logger.LogInformation($"Starting  -- Querying system information");
        }
        public void LogAfterSysInfo()
        {
            _logger.LogInformation($"Completed -- Querying system information");
        }

    }
}
