using Docker.DotNet;
using Docker.DotNet.Models;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DevMark.Core.Container
{
    public class DockerContainerProvider
    {
        private const string IMAGE_BASE_NAME = "DevMark";
        private readonly DockerClient _client;
        private readonly ILogger<DockerContainerProvider> _logger;

        public DockerContainerProvider(DockerClient client, ILogger<DockerContainerProvider> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<string> BuildImage(string tag, string dockerFileName, List<(string sysPath, string tarPath)> files, string versionLabel = null, string tarOutputPath = null)
        {
            string tagLower = tag.ToLower();

            using (var stream = new MemoryStream())
            {
                FillDockerTarStream(stream, files);

                if (!string.IsNullOrEmpty(tarOutputPath))
                {
                    File.WriteAllBytes(tarOutputPath, stream.ToArray());
                    stream.Position = 0;
                }

                var buildParams = new ImageBuildParameters
                {
                    Tags = new List<string> { tagLower },
                    Labels = versionLabel != null ? new Dictionary<string, string> { { "DevMarkVersion", versionLabel }  } : null,
                    Dockerfile = dockerFileName
                };

                try
                {
                    using (var response = await _client.Images.BuildImageFromDockerfileAsync(stream, buildParams))
                    {

                        using (var ms = new MemoryStream())
                        {
                            using (var sr = new StreamReader(response))
                            {
                                while (!sr.EndOfStream)
                                {
                                    LogFromBuild(sr);
                                }
                            }
                        }
                    }
                }
                catch (TimeoutException)
                {
                    _logger.LogError("Timeout while connecting to the Docker Engine. Make sure Docker is installed and started.");
                    throw;
                }
            }

            return $"{tagLower}:latest";
        }

        private void LogFromBuild(StreamReader sr)
        {
            string str = sr.ReadLine();
            try
            {
                var lineObj = JsonConvert.DeserializeObject<BuildLogOutput>(str);
                if (lineObj.Stream != null)
                {
                    var infoExpression = new Regex(@"(Step \d+\/\d+)|Successfully");
                    if (infoExpression.IsMatch(lineObj.Stream))
                    {
                        _logger.LogInformation(lineObj.Stream);
                    }
                    else
                    {
                        _logger.LogDebug(lineObj.Stream);
                    }
                }
                if (lineObj.Aux != null)
                {
                    _logger.LogDebug(lineObj.Aux.ToString());
                }
            }
            catch (Exception)
            {
                _logger.LogWarning(str);
            }
        }

        public async Task<ContainerRunResult> Run(string tag, IEnumerable<string> arguments, string shareHostDirectory = null, string shareContainerDirectory = null)
        {
            string image = tag.Contains(':') ? tag : $"{tag}:latest";
            string name = $"{IMAGE_BASE_NAME}_{Guid.NewGuid()}";

            var gcMemoryInfo = GC.GetGCMemoryInfo();
            var memory = gcMemoryInfo.TotalAvailableMemoryBytes;

            var binds = new List<string>();
            if (!string.IsNullOrEmpty(shareHostDirectory) && !string.IsNullOrEmpty(shareContainerDirectory))
            {
                string volume = $"{shareHostDirectory}:{shareContainerDirectory}";
                binds.Add(volume);
            }

            _logger.LogDebug("Creating container");
     
            var createParams = new CreateContainerParameters { Image = image, Name = name, Tty = true, HostConfig = new HostConfig { CPUCount = Environment.ProcessorCount, Memory = memory, Binds = binds }, Cmd = arguments.ToList() };

            CreateContainerResponse container = await TryStartContainer(createParams);

            var logs = await _client.Containers.GetContainerLogsAsync(container.ID, true, new ContainerLogsParameters { ShowStderr = true, ShowStdout = true, Follow= true });

            var readLogTask = Task.Run(async () => await ContiniouslyReadLogs(logs));

            var waitResp = await _client.Containers.WaitContainerAsync(container.ID);

            await readLogTask;

            await _client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters { });
            return new ContainerRunResult { ExitCode = waitResp.StatusCode, Image = image, CPUs = createParams.HostConfig.CPUCount, Memory = createParams.HostConfig.Memory } ;
        }


        private async Task<CreateContainerResponse> TryStartContainer(CreateContainerParameters createParams)
        {
            int retryCount = 5;
            long initialMem = createParams.HostConfig.Memory;
            for (int i = 0; i < retryCount; i++)
            {
                createParams.HostConfig.Memory = initialMem - (long)(i * 0.05 * initialMem);

                var container = await _client.Containers.CreateContainerAsync(createParams);
                _logger.LogDebug("Created container with ID:" + container.ID);

                try
                {
                    var startResp = await _client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters { });
                    _logger.LogDebug("Started: " + startResp);
                    return container;
                }
                catch (DockerApiException e)
                {
                    if (e.ResponseBody.ToLower().Contains("insufficient system resources") && i != retryCount - 1)
                    {
                        _logger.LogWarning($"Failed to start container due to insufficient system resources ({createParams.HostConfig.Memory / 1024 / 1024}MB memory), retrying {i+1}/5...");
                        await _client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters { });
                        await Task.Delay(2500);
                        continue;
                    }
                    throw;
                }
            }
            return null;
        }

        public async Task<bool> HasRunningContainers()
        {
            var containers = await _client.Containers.ListContainersAsync(new ContainersListParameters { All = true });

            var runningContainres = containers.Where(x => x.State == "running").ToList();

            foreach (var container in runningContainres)
            {
                _logger.LogWarning($"Container \"{string.Concat(container.Names)}\" ({container.ID}) is currently running.");
            }

            return runningContainres.Count != 0;
        }

        private async Task ContiniouslyReadLogs(MultiplexedStream logs)
        {
            var stdOutStream = new MemoryStream();
            var stdErrStream = new MemoryStream();
            var outReader = new StreamReader(stdOutStream);
            var errReader = new StreamReader(stdErrStream);

            while (true)
            {
                var buffer = new byte[1024 * 1024];
                var result = await logs.ReadOutputAsync(buffer, 0, buffer.Length, CancellationToken.None);

                if (result.Target == MultiplexedStream.TargetStream.StandardOut)
                {
                    WriteStreamToLog(stdOutStream, outReader, buffer, result.Count, LogLevel.Information);
                }
                else if (result.Target == MultiplexedStream.TargetStream.StandardError)
                {
                    WriteStreamToLog(stdErrStream, errReader, buffer, result.Count, LogLevel.Error);
                }

                if (result.Count == 0)
                    await Task.Delay(100);
                if (result.EOF)
                {
                    break;
                }
            }
        }

        private void WriteStreamToLog(MemoryStream stdOutStream, StreamReader outReader, byte[] buffer, int length, LogLevel logLevel)
        {
            long positionBefore = stdOutStream.Position;
            stdOutStream.Write(buffer, 0, length);
            stdOutStream.Seek(positionBefore, SeekOrigin.Begin);

            string nextLine;
            do
            {
                nextLine = outReader.ReadLine();
                if (nextLine != null)
                    _logger.Log(logLevel, nextLine);
            }
            while (nextLine != null);
        }

        private static void FillDockerTarStream(Stream stream, List<(string sysPath, string tarPath)> files)
        {
            using (var tar = new TarOutputStream(stream, Encoding.ASCII) { IsStreamOwner = false })
            {
                foreach (var (sysPath, tarPath) in files)
                {
                    string relativeUnixPath = tarPath.Replace('\\', '/').Trim('/');

                    var entry = TarEntry.CreateTarEntry(relativeUnixPath);
                    using (var fs = File.OpenRead(sysPath))
                    {
                        entry.Size = fs.Length;
                        entry.TarHeader.Mode = Convert.ToInt32("100755", 8); // 755
                        tar.PutNextEntry(entry);
                        CopyToStream(fs, tar);
                    }
                    tar.CloseEntry();
                }
                tar.Close();
                stream.Position = 0;
            }
        }

        private static void CopyToStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[64 * 1024];
            while (true)
            {
                int count = input.Read(buffer, 0, buffer.Length);
                if (count == 0)
                    break;

                output.Write(buffer, 0, count);
            }
        }
    }

    class BuildLogOutput
    {
        [JsonProperty("stream")]
        public string Stream { get; set; }

        [JsonProperty("aux")]
        public JToken Aux { get; set; }
    }
}
