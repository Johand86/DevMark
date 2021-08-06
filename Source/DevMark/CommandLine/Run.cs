using DevMark.CommandLine.Arguments;
using DevMark.Core.SysIdle;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevMark.CommandLine
{
    public class Run
    {
        public static async Task<CommandLineResult> Execute(RunCommandLineArgs args)
        {
            ServiceProvider serviceProvider;
            IDiagnosticProvider diagnosticsProvider = null;
            try
            {
                if (args.Help)
                {
                    return CommandLineResult.Help;
                }

                serviceProvider = CommandSetup.ServiceProvider(args, args.WorkDir, apiToken: args.ApiKey, runningInContainer: args.DockerTarget);

                diagnosticsProvider = serviceProvider.GetService<IDiagnosticProvider>();
                var cmdPrinter = serviceProvider.GetRequiredService<CommandLinePrinter>();
                var testSuiteFileProvider = serviceProvider.GetService<TestSuiteFileProvider>();
                string[] testSuites = testSuiteFileProvider.ResolveTestSuites(args.Test, args.All);

                string[] categories = (args.Categories ?? "default").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

                if (testSuites.Length > 0)
                {
                    using (var operation = diagnosticsProvider.StartOperation("TestSuites"))
                    {
                        using (serviceProvider.GetService<ISystemIdlePreventionProvider>())
                        {
                            var testRunner = serviceProvider.GetService<TestRunner>();

                            var (testRun, resultPath) = testRunner.RunSuites(testSuites, categories, args.Result);

                            if (!args.DockerTarget)
                            {

                                cmdPrinter.PrintTestResults(testRun);

                                if (!string.IsNullOrEmpty(args.ApiKey))
                                {
                                    if (!testRunner.Verify(testRun))
                                    {
                                        return new CommandLineResult(false, "Failed to verify the integrity of the file.");
                                    }

                                    var uploadResult = await testRunner.Upload(testRun);
                                    cmdPrinter.PrintUploadResult(uploadResult);
                                }
                                else
                                {
                                    if (testRun.TestSuites.Any())
                                        cmdPrinter.PrintUploadSuggestion(resultPath);
                                }
                            }

                            return CommandLineResult.Success;
                        }
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
