using DevMark.CommandLine.Arguments;
using DevMark.Core.TestResult;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace DevMark.CommandLine
{
    public class Upload
    {
        public static async Task<CommandLineResult> Execute(UploadCommandLineArgs args)
        {
            ServiceProvider serviceProvider;
            IDiagnosticProvider diagnosticsProvider = null;
            try
            {
                if (args.Help)
                {
                    return CommandLineResult.Help;
                }
                serviceProvider = CommandSetup.ServiceProvider(args, apiToken: args.ApiKey, customServiceUrl: args.Url);

                diagnosticsProvider = serviceProvider.GetService<IDiagnosticProvider>();

                var testSuiteFileProvider = serviceProvider.GetService<TestSuiteFileProvider>();
                var cmdPrinter = serviceProvider.GetRequiredService<CommandLinePrinter>();

                using (var operation = diagnosticsProvider.StartOperation("Upload"))
                {
                    var testRunner = serviceProvider.GetService<TestRunner>();

                    var testRun = testRunner.Load(args.Result);

                    if (!testRunner.Verify(testRun))
                    {
                        return new CommandLineResult(false, "Failed to verify the integrity of the file.");
                    }

                    var uploadResult = await testRunner.Upload(testRun);
                    cmdPrinter.PrintUploadResult(uploadResult);

                    return CommandLineResult.Success;
                }
            }
            catch (ApplicationException e)
            {
                return new CommandLineResult(true, e.Message);
            }
            finally
            {
                diagnosticsProvider?.Flush();
            }
        }
    }
}
