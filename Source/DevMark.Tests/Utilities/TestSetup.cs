using DevMark.Core.Engine;
using DevMark.Core.SystemInformation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using System.IO;
using System.Runtime.CompilerServices;

namespace DevMark.Tests
{
    public class TestSetup
    {
        private static object initLock = new object();
        private static bool initialized = false;
        public static void Initialize()
        {
            lock (initLock)
            {
                if (!initialized)
                {
                    Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.TestCorrelator().CreateLogger();
                    initialized = true;
                }
            }
        }

        public static ServiceProvider BuildDefaultServiceProvider([CallerFilePath] string fileName = "")
        {
            var serviceCollection = new ServiceCollection();

            string tempSubFolderName = Path.GetFileNameWithoutExtension(fileName);

            string workDirectory = Path.Combine(@"C:\Temp\DevMarkTestTemp", tempSubFolderName); //TODO: use temp folder, or is it safe (for git) to use bin directory of test?
            ConfigureServices(serviceCollection, workDirectory);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }


        private static void ConfigureServices(ServiceCollection services, string workDir)
        {
            services.AddLogging(configure =>
            {
                configure.SetMinimumLevel(LogLevel.Information);
                configure.AddSerilog();

            });

            services.AddSingleton(x => new Mock<WorkDirectoryProvider>(() => new WorkDirectoryProvider(x.GetService<Microsoft.Extensions.Logging.ILogger>())).Object);
            services.AddSingleton(x => new Mock<CommandFactory>(() => new CommandFactory(workDir)).Object);
            services.AddSingleton(x => new Mock<BenchmarkEngine>(() => new BenchmarkEngine(x.GetService<CommandFactory>(), x.GetService<BenchmarkEngineLogger>(), x.GetService<SysInfoProvider>())).Object);
            services.AddSingleton(x => new Mock<SysInfoProvider>(() => new SysInfoProvider(x.GetService<CommandFactory>(), x.GetService<ILogger<SysInfoProvider>>())).Object);
            services.AddSingleton<Microsoft.Extensions.Logging.ILogger>(x => x.GetService<ILogger<TestSetup>>());
            services.AddSingleton<BenchmarkEngineLogger>();
        }

    }
}
