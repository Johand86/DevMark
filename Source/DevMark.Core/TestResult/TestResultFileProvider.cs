using DevMark.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Globalization;
using System.IO;

namespace DevMark.Core.TestResult
{
    public class TestResultFileProvider
    {
        private readonly JsonSerializerSettings _settings;
        ILogger<TestResultFileProvider> _logger;

        public TestResultFileProvider(ILogger<TestResultFileProvider> logger, JsonSerializerSettings jsonSettings)
        {
            _logger = logger;
            _settings = jsonSettings;
        }

        public string Save(TestRun testRun, string path)
        {
            string content = JsonConvert.SerializeObject(testRun, _settings);
            
            string filename = !string.IsNullOrEmpty(path) ? Path.GetFileName(path) : GenerateFilename();

            string appFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            string sysTempFolder = Path.Combine(Path.GetTempPath(), filename);


            if (!string.IsNullOrEmpty(path) && TrySaveResult(path, content, false))
            {
                return path;
            }

            if (TrySaveResult(appFolder, content, false))
            {
                return appFolder;
            }

            TrySaveResult(sysTempFolder, content, true);
            return sysTempFolder;
        }

        private string GenerateFilename()
        {
            return $"DevMark_TestResult_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.json";
        }

        private bool TrySaveResult(string path, string content, bool throwOnError)
        {
            try
            {
                string dirName = Path.GetDirectoryName(path);
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }

                File.WriteAllText(path, content);
                return true;

            }
            catch (Exception)
            {
                _logger.LogInformation($"Failed to use \"{path}\" for temporary storage.");
                if (throwOnError)
                    throw;

                return false;
            }
        }

        public TestRun Load(string resultPath)
        {
            if (!File.Exists(resultPath))
            {
                throw new ApplicationException($"Could not find result file at \"{resultPath}\".");
            }

            try
            {
                string content = File.ReadAllText(resultPath);
                return JsonConvert.DeserializeObject<TestRun>(content, _settings);
            }
            catch (Exception e)
            {
                string message = $"Could not load result file from \"{resultPath}\".";
                _logger.LogError(e.ToString());
                throw new ApplicationException(message, e);
            }

        }
    }
}
