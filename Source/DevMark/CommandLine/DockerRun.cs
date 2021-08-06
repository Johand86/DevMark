using DevMark.CommandLine.Arguments;
using DevMark.Core;
using DevMark.Core.Container;
using DevMark.Core.SysIdle;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DevMark.CommandLine
{
    public class DockerRun
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

                serviceProvider = CommandSetup.ServiceProvider(args, args.WorkDir, apiToken: args.ApiKey, devContainer: args.DockerDevContainer, skipContainerBuild: args.DockerSkipBuild, devSourcePath: args.DockerDevSourcePath);

                diagnosticsProvider = serviceProvider.GetService<IDiagnosticProvider>();
                var testSuiteFileProvider = serviceProvider.GetService<TestSuiteFileProvider>();
                
                var cmdPrinter = serviceProvider.GetRequiredService<CommandLinePrinter>();

                var dockerTestRunner = serviceProvider.GetService<DockerTestRunner>();
                var testRunner = serviceProvider.GetService<TestRunner>();

                var testSuiteConfigProvider = serviceProvider.GetService<TestSuiteConfigurationProvider>();

                using (var operation = diagnosticsProvider.StartOperation("Docker"))
                {
                    using (serviceProvider.GetService<ISystemIdlePreventionProvider>())
                    {
                        string[] testSuites = testSuiteFileProvider.ResolveTestSuites(args.Test, args.All);

                        var (testRun, resultPath) = await dockerTestRunner.RunSuites(args.Result, args.Dockerfile, args.DockerWait, args.Verbose, args.Diagnostics, testSuites, args.Categories);

                        if (testRun == null || resultPath == null)
                        {
                            return CommandLineResult.None;
                        }

                        cmdPrinter.PrintTestResults(testRun);

                        if (!string.IsNullOrEmpty(args.ApiKey))
                        {
                            if (!testRunner.Verify(testRun))
                            {
                                return VerificationFailed();
                            }

                            var uploadResult = await testRunner.Upload(testRun);
                            cmdPrinter.PrintUploadResult(uploadResult);
                        }
                        else
                        {
                            if (testRun.TestSuites.Any())
                                cmdPrinter.PrintUploadSuggestion(resultPath);
                        }

                        return CommandLineResult.Success;
                    }
                }
            }
            finally
            {
                diagnosticsProvider?.Flush();
            }
        }

        private static CommandLineResult VerificationFailed()
        {
            return new CommandLineResult(false, "Failed to verify the integrity of the file.");
        }
    }
}
