using DevMark.CommandLine.Arguments;
using Microsoft.Extensions.DependencyInjection;

namespace DevMark.CommandLine
{
    public class Check
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
                var cmdPrinter = serviceProvider.GetRequiredService<CommandLinePrinter>();
                string[] testSuites = testSuiteFileProvider.ResolveTestSuites(args.Test, args.All);

                if (testSuites.Length > 0)
                {
                    using (var operation = diagnosticsProvider.StartOperation("TestSuites"))
                    {
                        var testRunner = serviceProvider.GetService<TestRunner>();

                        testRunner.CheckDependencies(testSuites);
                        return CommandLineResult.Success;
                    }

                }

                return CommandLineResult.Help;
            }
            finally
            {
                diagnosticsProvider?.Flush();
            }
        }
    }
}
