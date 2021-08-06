using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using DevMark.CommandLine.Arguments;
using DevMark.CommandLine;
using System.Runtime.InteropServices;
using System;
using System.Threading;
using System.Linq;

namespace DevMark
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            bool dockerTarget = args.Any(x => string.Compare("--docker-target", x, true) == 0);
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !dockerTarget)
            {
                Console.WriteLine("Sorry, this app is currently only available on Windows. Please check back later!");
                return await Task.FromResult(-1);
            }
            var (rootCommand, runCommand, checkCommand, uploadCommand, listCommand) = BuildCommandLine();

            runCommand.Handler = CommandHandler.Create<RunCommandLineArgs>(async (x) => await RunHandler(runCommand, x));

            checkCommand.Handler = CommandHandler.Create<CheckCommandLineArgs>((x) => CheckHandler(checkCommand, x));
    
            uploadCommand.Handler = CommandHandler.Create<UploadCommandLineArgs>(async (x) =>
            {
                var result = await Upload.Execute(x);
                return HandleResult(uploadCommand, result, x);
            });

            listCommand.Handler = CommandHandler.Create<BaseCommandLineArgs>((x) =>
            {
                var result = List.Execute(x);
                return HandleResult(listCommand, result, x);
            });

            return await rootCommand.InvokeAsync(args);
        }

        private static int CheckHandler(Command command, CheckCommandLineArgs x)
        {
            if (x.Docker)
            {
                var result = DockerCheck.Execute(x);
                return HandleResult(command, result, x);
            }
            else
            {
                var result = Check.Execute(x);
                return HandleResult(command, result, x);
            }
        }

        private static async Task<int> RunHandler(Command command, RunCommandLineArgs x)
        {
            try
            {
                if (x.Docker)
                {
                    var result = await DockerRun.Execute(x);
                    return HandleResult(command, result, x);
                }
                else
                {
                    var result = await Run.Execute(x);
                    return HandleResult(command, result, x);
                }
            }
            finally
            {
                if (x.DockerTarget && x.DockerWait)
                {
                    Console.WriteLine("Docker Wait mode enabled, keeping process alive.");
                    while (true)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
        }

        private static int HandleResult(Command command, CommandLineResult result, BaseCommandLineArgs args)
        {
            var console = new SystemConsole();
            if (result.ShowHelp)
            {
                HelpBuilder help = new HelpBuilder(console);
                help.Write(command);
            }
            if (result.Message != null)
            {
                console.Out.WriteLine(result.Message);
                console.Out.WriteLine("");
            }

            return result.Message == null ? 0 : -1;
        }

        private static (RootCommand rootCommand, Command runCommand, Command checkCommand, Command uploadCommand, Command ListCommand) BuildCommandLine()
        {
            RootCommand rootCommand = new RootCommand(description: "A benchmarking utility for dev environments.");

            // Common params
            var verbose = new Option<bool>(aliases: new string[] { "--verbose", "-v" }, description: "Verbose output, use to investigate issues.");
            var diagnostics = new Option<bool>(aliases: new string[] { "--diagnostics", "-d" }, description: "Collect and send diagnostic logs to the application developer using Application Insights. By enabling this you agree to the user and privacy policy.");
            var log = new Option<string>(aliases: new string[] { "--log", "-l" }, description: "Write all output to a logfile.") { Argument = new Argument<string>("FilePath") };
            var help = new Option<bool>(aliases: new string[] { "--help", "-?", "-h" }, description: "Display help.");
            var dockerTarget = new Option<bool>(aliases: new string[] { "--docker-target" }, description: "For internal use only. Singal that the application is running in a docker container. ") { IsHidden = true };
            var dockerWait = new Option<bool>(aliases: new string[] { "--docker-wait" }, description: "For internal use only. Prevents the application from terminating so that the container remains running.") { IsHidden = true };


            // Run and check params
            var workDir = new Option<string>(aliases: new string[] { "--work-dir", "-w" }, description: "Path to the work directory. Leave empty for default temp folder.") { Argument = new Argument<string>("FilePath") };
            var docker = new Option<bool>(aliases: new string[] { "--docker" }, description: "Run tests in a docker container.");

            //Run and upload params

            // Run params
            var all = new Option<bool>(aliases: new string[] { "--all", "-a" }, description: "Run all test suites.");
            var upload = new Option<string>(aliases: new string[] { "--api-key", "-k" }, description: $"Specify an API key to upload your result to the web. Go to \"{DependencyConfigurator.DefaultServiceUrl}\" to setup an account.") { Argument = new Argument<string>("AccessToken") };
            var result = new Option<string>(aliases: new string[] { "--result", "-r" }, description: "Specify the name or path of a result file.") { Argument = new Argument<string>("FilePath") };
            var category = new Option<string>(aliases: new string[] { "--categories", "-c" }, description: "Specify one or more (comma separated) categories to run. The \"default\" category will be used when not specified.") { Argument = new Argument<string>("Category") };

            var dockerfile = new Option<string>(aliases: new string[] { "--dockerfile" }, description: "Override the default dockerfile that's being used to run Docker tests.") { Argument = new Argument<string>("FilePath") };
            var dockerSkipBuild = new Option<bool>(aliases: new string[] { "--docker-skip-build" }, description: "For internal use. Skip invoking the Docker build, which allows for faster testing.") { IsHidden = true };
            var dockerDevContainer = new Option<bool>(aliases: new string[] { "--docker-dev-container" }, description: "For internal use. Specify that source code should be included in the TAR file for building DevMark inside a container.") { IsHidden = true };
            var dockerDevSourcePath = new Option<string>(aliases: new string[] { "--docker-dev-source-path" }, description: "For internal use. Specify the path to the source code that should be included in the DevMark container.") { Argument = new Argument<string>("FolderPath"), IsHidden = true };

            var runCommand = new Command("run", "Run a benchmark.");
            runCommand.AddArgument(new Argument<string>("test", () => "") { Description = "The name, filename or path to a testsuite to run." });
            runCommand.Add(workDir);
            runCommand.Add(verbose);
            runCommand.Add(diagnostics);
            runCommand.Add(log);
            runCommand.Add(all);
            runCommand.Add(result);
            runCommand.Add(upload);
            runCommand.Add(help);
            runCommand.Add(category);
            runCommand.Add(dockerTarget);
            runCommand.Add(dockerWait);
            runCommand.Add(docker);
            runCommand.Add(dockerfile);
            runCommand.Add(dockerSkipBuild);
            runCommand.Add(dockerDevContainer);
            runCommand.Add(dockerDevSourcePath);

            // Check params
            var checkAll = new Option<bool>(aliases: new string[] { "--all", "-a" }, description: "Check dependencies for all test suites.");

            var checkCommand = new Command("check", "Check that dependencies are installed.");
            checkCommand.AddArgument(new Argument<string>("test", () => "") { Description = "The name, filename or path to a test suite that dependencies should be checked for." }); ;
            checkCommand.Add(workDir);
            checkCommand.Add(verbose);
            checkCommand.Add(diagnostics);
            checkCommand.Add(log);
            checkCommand.Add(checkAll);
            checkCommand.Add(help);
            checkCommand.Add(dockerWait);
            checkCommand.Add(docker);

            // Upload params
            var apiKey = new Option<string>(aliases: new string[] { "--api-key", "-k" }, description: "Access token for your account.") { Argument = new Argument<string>("AccessToken"), Required = true };
            var url = new Option<string>(aliases: new string[] { "--url", "-u" }, description: "Override the URL to the service.") { Argument = new Argument<string>("Url") };

            var uploadCommand = new Command("upload", $"Upload the results of your benchmark to the web. Go to \"{DependencyConfigurator.DefaultServiceUrl}\" to setup an account.");
            uploadCommand.AddArgument(new Argument<string>("result") { Description = "Name or path of a result file." });
            uploadCommand.Add(apiKey);
            uploadCommand.Add(url);
            uploadCommand.Add(help);
            uploadCommand.Add(dockerWait);

            var listCommand = new Command("list", "List all built in test suites.");
            listCommand.AddOption(log);
            listCommand.AddOption(verbose);
            listCommand.AddOption(help);
            listCommand.AddOption(diagnostics);
            listCommand.Add(dockerWait);

            rootCommand.AddCommand(runCommand);
            rootCommand.AddCommand(listCommand);
            rootCommand.AddCommand(checkCommand);
            rootCommand.AddCommand(uploadCommand);

            rootCommand.TreatUnmatchedTokensAsErrors = true;
            return (rootCommand, runCommand, checkCommand, uploadCommand, listCommand);
        }
    }
}