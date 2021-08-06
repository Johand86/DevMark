using DevMark.CommandLine.Arguments;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;

namespace DevMark.CommandLine
{
    public class CommandSetup
    {
        public static ServiceProvider ServiceProvider(BaseCommandLineArgs args, string workDirectory = null, string apiToken = null, string customServiceUrl = null, bool runningInContainer = false, bool devContainer = false, string devSourcePath = null, bool skipContainerBuild = false)
        {
            ServiceProvider serviceProvider;
            var dependencyConfig = new DependencyConfigurator();
            dependencyConfig.SetupLogging(args.Log, args.Verbose, runningInContainer);

            var logger = new SerilogLoggerProvider(Log.Logger).CreateLogger(nameof(Program));
            var workDirectoryProvider = new WorkDirectoryProvider(logger);
            string workDir = workDirectoryProvider.Setup(workDirectory);
            serviceProvider = dependencyConfig.BuildServiceProvider(workDir, args.Verbose, args.Diagnostics, apiToken, customServiceUrl, runningInContainer, devContainer, devSourcePath, skipContainerBuild);
            return serviceProvider;
        }
    }
}
