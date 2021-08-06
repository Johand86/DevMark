using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace DevMark
{
    public class WorkDirectoryProvider
    {
        ILogger _logger;

        public WorkDirectoryProvider(ILogger logger)
        {
            _logger = logger;
        }

        public string Setup(string workDirectory)
        {
            string sysTempFolder = Path.Combine(Path.GetTempPath(), "DevMark");
            string appFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorkDirectory");
            bool isDevEnvironment = appFolder.Contains("bin");

            if (!string.IsNullOrEmpty(workDirectory) && TryInitTempFolder(workDirectory, false))
            {
                return workDirectory;
            }

            if (!isDevEnvironment && TryInitTempFolder(appFolder, false))
            {
                return appFolder;
            }

            TryInitTempFolder(sysTempFolder, true);
            return sysTempFolder;
        }

        private bool TryInitTempFolder(string workDirectory, bool throwOnError)
        {
            try
            {
                if (!Directory.Exists(workDirectory))
                {
                    Directory.CreateDirectory(workDirectory);
                }

                File.WriteAllText(Path.Combine(workDirectory, "readme.txt"), "This folder has been used as a temp directory for DevMark.");
                return true;

            }
            catch (Exception e)
            {
                _logger.LogInformation($"Failed to use \"{workDirectory}\" for temporary storage ({e.Message}).");
                if (throwOnError)
                    throw;

                return false;
            }
        }

    }
}
