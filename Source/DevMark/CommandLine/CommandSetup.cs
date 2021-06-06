using DevMark.CommandLine.Arguments;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;

namespace DevMark.CommandLine
{
    public class CommandSetup
    {
        public static ServiceProvider ServiceProvider(BaseCommandLineArgs args, string workDirectory = null, string apiToken = null, string customServiceUrl = null)
        {
            ServiceProvider serviceProvider;
            var dependencyConfig = new DependencyConfigurator();
            dependencyConfig.SetupLogging(args.Log, args.Verbose);

            var logger = new SerilogLoggerProvider(Log.Logger).CreateLogger(nameof(Program));
            var workDirectoryProvider = new WorkDirectoryProvider(logger);
            string workDir = workDirectoryProvider.Setup(workDirectory);
            serviceProvider = dependencyConfig.BuildServiceProvider(workDir, args.Verbose, args.Diagnostics, apiToken, customServiceUrl);
            return serviceProvider;
        }
    }
}
