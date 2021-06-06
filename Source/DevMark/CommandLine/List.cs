using DevMark.CommandLine.Arguments;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DevMark.CommandLine
{
    public class List
    {
        public static CommandLineResult Execute(BaseCommandLineArgs args)
        {
            ServiceProvider serviceProvider;
            IDiagnosticProvider diagnosticsProvider = null;
            try
            {
                if (args.Help)
                {
                    return CommandLineResult.Help;
                }
                serviceProvider = CommandSetup.ServiceProvider(args);
                var testSuiteFileProvider = serviceProvider.GetService<TestSuiteFileProvider>();
                var cmdPrinter = serviceProvider.GetRequiredService<CommandLinePrinter>();
                cmdPrinter.PrintTestSuitesList(testSuiteFileProvider.GetDefaultPath());
                return CommandLineResult.Success;
            }
            finally
            {
                diagnosticsProvider?.Flush();
            }
        }
    }
}
