using DevMark.Core.TestResult;
using DevMark.Model;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.IO;
using System.IO;
using System.Linq;

namespace DevMark.CommandLine
{
    public class CommandLinePrinter
    {
        private readonly IConsole _console;
        private readonly TestSuiteFileProvider _testSuiteFileProvider;
        private readonly TestSuiteConfigurationProvider _testSuiteConfigurationProvider;

        public CommandLinePrinter(IConsole console, TestSuiteFileProvider testSuiteFileProvider, TestSuiteConfigurationProvider testSuiteConfigurationProvider)
        {
            _console = console;
            _testSuiteFileProvider = testSuiteFileProvider;
            _testSuiteConfigurationProvider = testSuiteConfigurationProvider;
        }

        const string separator = "---------------------";
        public void PrintTestSuitesList(string testSuitesDefaultPath)
        {
            var testSuiteList = _testSuiteFileProvider.ListTestSuits(testSuitesDefaultPath);
            _console.Out.WriteLine("Available test suites");
            _console.Out.WriteLine(separator);
            _console.Out.WriteLine("");
            foreach (var suite in testSuiteList)
            {
                var config = _testSuiteConfigurationProvider.ParseFile(suite);
                var filename = Path.GetFileName(suite);
                _console.Out.WriteLine(filename);
                _console.Out.WriteLine($"- {config.Name} {config.Version}");
                _console.Out.WriteLine($"- {config.Description}");
                foreach (var test in config.Tests)
                {
                    _console.Out.WriteLine($"-- {test.Name}: {test.Description}");
                }
                _console.Out.WriteLine("");
            }
        }

        public void PrintUploadResult(UploadResult uploadResult)
        {
            var o = _console.Out;
            o.WriteLine();

            if (uploadResult.Success)
            {
                o.WriteLine(uploadResult.Message);
                o.WriteLine($"Result was successfully uploaded, go to \"{uploadResult.Url}\" to check it out.");
            }
            else
            {
                o.WriteLine("Failed to upload test result.");
                o.WriteLine(uploadResult.Message);
            }
        }

        public void PrintTestResults(TestRun testRun)
        {
            var o = _console.Out;

            o.WriteLine();

            var sysInfo = testRun.SysInfo;
            if (sysInfo != null)
            {

                o.WriteLine("Hardware");
                o.WriteLine(separator);

                foreach (var cpu in sysInfo.CPUs)
                {
                    o.WriteLine($"{cpu.Name} @ {cpu.CurrentClockSpeed}Mhz");
                }
                foreach (var ram in sysInfo.RAM.GroupBy(x => x.Model))
                {
                    o.WriteLine($"{(ram.Count() > 1 ? ram.Count() + "x " : "")}{ram.Key} {ram.Average(x => x.Size)}MB, {ram.Sum(x => x.Size)}MB in total @ {ram.Average(x => x.ClockSpeed)}Mhz, {ram.Average(x => x.Voltage / 1000.0)}V");
                }

                var mbm = sysInfo.Motherboard;
                o.WriteLine($"{(string.IsNullOrEmpty(mbm.Manufacturer) ? "": mbm.Manufacturer + " ")}{mbm.Model}, BIOS: {mbm.Bios}");

                foreach (var gpu in sysInfo.GPUs)
                {
                    o.WriteLine($"{gpu.Name}, {gpu.Memory}MB");
                }

                foreach (var disk in sysInfo.Disks)
                {
                    o.WriteLine($"{(string.IsNullOrEmpty(disk.Manufacturer) ? "" : disk.Manufacturer + " ")}{disk.Model}, {disk.MediaType}, {disk.BusType}, {disk.Size / 1000}GB{(disk.ContainsWorkDirectory == true ? ", *** Work Directory ***" : "")}");
                }

                var os = sysInfo.OS;
                o.WriteLine($"{os.Name}, {os.Version}, {os.Architecture}");

                if (!sysInfo.PowerSupply.AcPower)
                {
                    o.WriteLine("Power Source: battery");
                }

                o.WriteLine();
                if (testRun.TestSuites.Any())
                {
                    o.WriteLine("Benchmarks");
                    o.WriteLine(separator);

                    foreach (var testSuite in testRun.TestSuites)
                    {
                        o.WriteLine($"{testSuite.Name} {testSuite.Version}");
                        foreach (var test in testSuite.Tests)
                        {
                            var avg = test.Benchmarks.Where(x => x.Duration.HasValue).Average(x => x.Duration).Value.ToString("0.##");
                            o.WriteLine($"-- {test.Name} {(string.IsNullOrEmpty(avg) ? "" : avg + "s")} ({string.Join(", ", test.Benchmarks.Where(x => x.Duration.HasValue).Select(x => x.Duration.Value.ToString("0.##") + "s"))})");
                        }
                    }
                }
                else
                {
                    o.WriteLine("No benchmarks completed successfully, check for warnings and errors above.");
                    o.Write("If you install new dependencies, make sure close this window and start a new one before retrying.");
                }
            }
        }

        internal void PrintUploadSuggestion(string resultPath)
        {
            var o = _console.Out;
            o.WriteLine();
            o.WriteLine("Please consider uploading your result to our database using the following command: ");
            o.WriteLine($"devmark.exe upload --result \"{resultPath}\" --api-key <YourApiKey>.");
            o.WriteLine($"At \"{DependencyConfigurator.DefaultServiceUrl}\" you can get your API key, as well as browse and compare your results with others. ");
        }

        public void Help(Command command, string message = null)
        {
            HelpBuilder help = new HelpBuilder(_console);

            if (message != null)
            {
                _console.Out.WriteLine(message);
            }
            help.Write(command);
        }
    }
}