using DevMark.Core.Execution;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DevMark.Core.Container
{
    public class ContainerEngine
    {
        private readonly ILogger<ContainerEngine> _logger;
        private readonly IDockerfileFileProvider _dockerfileFileProvider;
        private readonly DockerContainerProvider _dockerProvider;
        private readonly string _workDirectory;
        private readonly bool _skipContainerBuild;
        private readonly Dictionary<string, string> _builtImages;
        private readonly CommandFactory _commandFactory;
        private readonly CommandLogger _commandLogger;

        public bool TraceLogging { get; set; }

        public ContainerEngine(DockerContainerProvider provider, IDockerfileFileProvider dockerfileFileProvider, CommandFactory commandFactory, CommandLogger commandLogger, ILogger<ContainerEngine> logger, string workDirectory, bool skipContainerBuild)
        {
            _dockerProvider = provider;
            _logger = logger;
            _dockerfileFileProvider = dockerfileFileProvider;
            _commandFactory = commandFactory;
            _commandLogger = commandLogger;
            _workDirectory = workDirectory;
            this._skipContainerBuild = skipContainerBuild;
            _builtImages = new Dictionary<string, string>();
        }


        public DockerInfoCommand GetDockerConfig()
        {
            using (ScriptExecuter executer = new ScriptExecuter())
            {
                executer.InitializeShell();
                var dockerInfo = _commandFactory.CreateDockerInfoCommand(new CommandEngineConfiguration { TraceLogging = false });
                executer.RunScript(dockerInfo);
                _commandLogger.LogAfterExecution(dockerInfo);
                return dockerInfo;
            }
        }

        public async Task<string> Prepare(string dockerfile, string imageUri)
        {
            if (!string.IsNullOrEmpty(imageUri))
            {
                throw new NotImplementedException("ImageUri is not yet implemented");
            }

            if (_builtImages.ContainsKey(dockerfile))
            {
                return _builtImages[dockerfile];
            }

            if (dockerfile != null)
            {
                string dockerFileName = Path.GetFileName(dockerfile);
                string dockerFileNameWithoutExtention = Path.GetFileNameWithoutExtension(dockerfile);

                string dockerFullFilePath = _dockerfileFileProvider.ResolveDockerfilePath(dockerfile);

                if (_skipContainerBuild)
                {
                    string preBuiltImage = dockerFileNameWithoutExtention.ToLower() + ":latest";
                    _logger.LogInformation($"Skipping build of image \"{preBuiltImage}\" due to dev param.");
                    return preBuiltImage;
                }


                _logger.LogInformation($"About to build \"{dockerFullFilePath}\". Make sure you have all appropriate liceses to use the Software in this image and the virtualization technology on your host.");
                _logger.LogInformation($"Building docker image \"{dockerFileName}\". For larger images this may take a while, check your CPU and network usage in the task manager if you believe this is stuck. Images can be 10GB+ in size.");

                var versionLabel = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
                string image = await _dockerProvider.BuildImage(dockerFileNameWithoutExtention, dockerfile, _dockerfileFileProvider.GetFilesForImage(dockerFullFilePath), versionLabel: versionLabel);
                _builtImages[dockerfile] = image;
                return image;
            }
            else
            {
                throw new ArgumentException("No container image could be found. Currently only Dockerfiles are supported.");
            }
        }

        public string[] PrepareWorkDirectory(string[] tests)
        {
            var containerTestNames = PrepareTestsInWorkDir(tests);
            return containerTestNames.Select(x => Path.GetFileName(x)).ToArray();
        }

        public async Task<bool> HasRunningContainers()
        {
            return await _dockerProvider.HasRunningContainers();
        }

        public async Task<ContainerRunResult> Run(string tag, string[] testSuites, string categories, bool diagnostics, bool dockerWait, bool verbose, PlatformConfiguration platform)
        {
            List<string> arguments = new List<string>();
            string shareDirectory = GetShareRootDirectory(platform);

            arguments.Add("run");
            arguments.Add(string.Join(";", testSuites.Select(x => ContainerCombine(platform, shareDirectory, x))));

            arguments.Add("--docker-target");

            if (!string.IsNullOrEmpty(categories))
            {
                arguments.Add("--categories");
                arguments.Add(categories);
            }

            if (verbose)
            {
                arguments.Add("--verbose");
            }

            if (diagnostics)
            {
                arguments.Add("--diagnostics");
            }

            if (dockerWait)
            {
                arguments.Add("--docker-wait");
            }

            arguments.Add("--work-dir");
            arguments.Add(GetContainerWorkDir(platform));

            string resultFileName = $"{Guid.NewGuid()}.json";
            string resultHostPath = Path.Combine(_workDirectory, resultFileName);

            arguments.Add("--result");
            arguments.Add(ContainerCombine(platform, shareDirectory, resultFileName));

            var result = await _dockerProvider.Run(tag, arguments, _workDirectory, shareDirectory);

            if (result?.ExitCode != 0)
            {
                _logger.LogWarning($"Container exited with code {result?.ExitCode}");
            }
            if (result.ExitCode == 0)
                result.ResultPath = resultHostPath;
            return result;
        }

        public (string dockerFile, string imageUri, PlatformConfiguration platform) ResolveImage(string expectedDockerFile, PlatformConfiguration platform, List<EnvironmentConfiguration> environment)
        {
            if (!string.IsNullOrEmpty(expectedDockerFile) && environment != null && environment.Any(x => x.Container?.Platform == platform))
            {
                return (expectedDockerFile, null, platform);
            }

            var actualDockerImage = environment?.Where(x => x.Container?.Platform == platform).Select(x => x.Container).FirstOrDefault();
            if (actualDockerImage?.Image?.Dockerfile != null)
                return (actualDockerImage.Image.Dockerfile, null, actualDockerImage.Platform);

            return (null, null, PlatformConfiguration.Any);
        }

        private string GetContainerWorkDir(PlatformConfiguration platform)
        {
            if (platform == PlatformConfiguration.Windows)
                return @"C:\work";
            return "/work";
        }

        private string ContainerCombine(PlatformConfiguration? platform, string shareDirectory, string resultFileName)
        {
            if (platform == PlatformConfiguration.Windows)
            {
                return shareDirectory?.TrimEnd('\\').TrimEnd('/') + "\\" + resultFileName?.Trim('\\').Trim('/');
            }

            return shareDirectory?.TrimEnd('\\').TrimEnd('/') + "/" + resultFileName?.Trim('\\').Trim('/');
        }

        private string GetShareRootDirectory(PlatformConfiguration? platform)
        {
            if (platform == PlatformConfiguration.Windows)
            {
                return @"C:\dmshare";
            }

            return "/dmshare";
        }

        private IEnumerable<string> PrepareTestsInWorkDir(string[] tests)
        {
            List<string> testNames = new List<string>();
            string testFileTimestamp = DateTime.Now.ToString("HHmmss");
            foreach (var test in tests)
            {
                string mountTempName = Path.Combine(_workDirectory, $"{testFileTimestamp}_{Guid.NewGuid()}.yaml");
                File.Copy(test, mountTempName);
                testNames.Add(mountTempName);
            }
            return testNames;
        }
    }
}
