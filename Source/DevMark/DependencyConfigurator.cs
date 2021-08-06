using DevMark.CommandLine;
using DevMark.Core;
using DevMark.Core.Container;
using DevMark.Core.Engine;
using DevMark.Core.Execution;
using DevMark.Core.SysIdle;
using DevMark.Core.SystemInformation;
using DevMark.Core.TestResult;
using Docker.DotNet;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using System.CommandLine;
using System.CommandLine.IO;
using System.Globalization;
using System.Runtime.InteropServices;

namespace DevMark
{
    public class DependencyConfigurator
    {
        public static string InstrumentationKey = "d73209f7-3712-40e8-8718-9c4a879cbd24";
        public static string DefaultServiceUrl = "https://www.devbenchmark.com/";

        public static JsonSerializerSettings DefaultSerializationSettings = new JsonSerializerSettings
        {
            Culture = CultureInfo.InvariantCulture,
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
        };

        public ServiceProvider BuildServiceProvider(string workDirectory, bool verboseLogging, bool diagnostics, string apiToken, string customServiceUrl, bool runningInContainer, bool devContainer, string devSourcePath, bool skipContainerBuild)
        {
            var services = new ServiceCollection();

            if (apiToken == "devsecret")
            {
                // We dont want to accidentally commit an api token in launchsettings.json, so for dev environments we support reading it from the user-secrets store
                // with our special keyword.
                IConfiguration config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
                apiToken = config["ServiceApiKey"];
            }

            services.AddLogging(configure =>
            {
                LogLevel level = verboseLogging ? LogLevel.Trace : LogLevel.Information;
                configure.SetMinimumLevel(level);
                configure.AddSerilog();

                if (diagnostics)
                {
                    configure.AddApplicationInsights(InstrumentationKey);
                    configure.AddFilter<ApplicationInsightsLoggerProvider>("", level);
                }

            });

            if (diagnostics)
            {
                TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
                configuration.InstrumentationKey = InstrumentationKey;
                services.AddApplicationInsightsTelemetryWorkerService(InstrumentationKey);
                services.AddSingleton(x => new TelemetryClient(configuration));
            }

            services.AddTransient(x => new CommandFactory(workDirectory));
            services.AddTransient<BenchmarkEngine>(x => new BenchmarkEngine(x.GetRequiredService<CommandFactory>(), x.GetRequiredService<BenchmarkEngineLogger>(), x.GetRequiredService<SysInfoProvider>()) { TraceLogging = verboseLogging, WorkDir = workDirectory });
            services.AddTransient<BenchmarkEngineLogger>();
            services.AddTransient<CommandLogger>();
            services.AddTransient<TestSuiteFileProvider>();
            services.AddTransient<TestSuiteConfigurationProvider>();
            services.AddTransient<CommandLinePrinter>();
            services.AddTransient<TestResultCompilationProvider>();
            services.AddTransient<SysInfoProvider>(x => new SysInfoProvider(x.GetRequiredService<CommandFactory>(), x.GetRequiredService<CommandLogger>(), runningInContainer, x.GetRequiredService<ILogger<SysInfoProvider>>()));
            services.AddTransient<TestResultFileProvider>(x => new TestResultFileProvider(x.GetRequiredService<ILogger<TestResultFileProvider>>(), DefaultSerializationSettings));
            services.AddTransient<TestResultUploadProvider>(x => new TestResultUploadProvider(x.GetRequiredService<ILogger<TestResultUploadProvider>>(), DefaultSerializationSettings, customServiceUrl ?? DefaultServiceUrl, apiToken));
            services.AddTransient<HashCalculator>();
            services.AddTransient<IConsole>((x) => new SystemConsole());
            services.AddTransient<TestRunner>();
            services.AddTransient<IDiagnosticProvider>(x => diagnostics ? new AIDiagnosticProvider(x.GetService<ILogger<AIDiagnosticProvider>>(), x.GetService<TelemetryClient>(), diagnostics) : new NopDiagnosticProvider());
            services.AddTransient<DockerContainerProvider>();
            services.AddTransient<DockerTestRunner>();
            services.AddSingleton(x => new DockerClientConfiguration().CreateClient());
            services.AddTransient(x => new ContainerEngine(x.GetRequiredService<DockerContainerProvider>(), x.GetRequiredService<IDockerfileFileProvider>(), x.GetRequiredService<CommandFactory>(), x.GetRequiredService<CommandLogger>(), x.GetRequiredService<ILogger<ContainerEngine>>(), workDirectory, skipContainerBuild) { TraceLogging = verboseLogging });

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddTransient<ISystemIdlePreventionProvider>(x => new WinSystemIdlePreventionProvider());
            }
            else
            {
                services.AddTransient<ISystemIdlePreventionProvider>(x => new NopSystemIdlePreventionProvider());
            }

            if (devContainer)
            {
                services.AddTransient<IDockerfileFileProvider>(x => new DevDockerfileFileProvider(devSourcePath));
            }
            else
            {
                services.AddTransient<IDockerfileFileProvider>(x => new DockerfileFileProvider());
            }

            return services.BuildServiceProvider();
        }

        public void SetupLogging(string logFile, bool verbose, bool runningInContainer)
        {
            var log = new LoggerConfiguration();
            
            if (runningInContainer)
                log = log.WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}{Exception}", standardErrorFromLevel: Serilog.Events.LogEventLevel.Error);
            else
                log = log.WriteTo.Console();

            if (!string.IsNullOrEmpty(logFile))
            {
                log = log.WriteTo.File(logFile);
            }

            if (verbose)
                log = log.MinimumLevel.Verbose();

            Log.Logger = log.CreateLogger();
        }
    }
}