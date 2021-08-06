using DevMark.CommandLine.Arguments;
using DevMark.Core;
using DevMark.Core.SysIdle;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace DevMark.CommandLine
{
    public class DockerCheck
    {
        public static CommandLineResult Execute(CheckCommandLineArgs args)
        {
            ServiceProvider serviceProvider;
            IDiagnosticProvider diagnosticsProvider = null;
            try
            {
                if (args.Help)
                {
                    return CommandLineResult.Help;
                }

                serviceProvider = CommandSetup.ServiceProvider(args, args.WorkDir);

                diagnosticsProvider = serviceProvider.GetService<IDiagnosticProvider>();
                var testSuiteFileProvider = serviceProvider.GetService<TestSuiteFileProvider>();
                var dockerTestRunner = serviceProvider.GetService<DockerTestRunner>();

                using (var operation = diagnosticsProvider.StartOperation("Docker"))
                {
                    using (serviceProvider.GetService<ISystemIdlePreventionProvider>())
                    {
                        string[] testSuites = testSuiteFileProvider.ResolveTestSuites(args.Test, args.All);

                        dockerTestRunner.CheckSuites(null, testSuites, args.DockerWait, args.Verbose, args.Diagnostics);
                        return CommandLineResult.Success;
                    }
                }
            }
            finally
            {
                diagnosticsProvider?.Flush();
            }
        }
    }
}
