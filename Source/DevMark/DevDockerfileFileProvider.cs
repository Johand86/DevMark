using DevMark.Core.Container;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DevMark
{
    public class DevDockerfileFileProvider : IDockerfileFileProvider
    {
        string _sourcePath;


        public DevDockerfileFileProvider()
        {

        }

        public DevDockerfileFileProvider(string sourcePath)
        {
            _sourcePath = sourcePath;
        }



        public List<(string sysPath, string tarPath)> GetFilesForImage(string dockerfile)
        {
            string srcRelativePathFromDebugDir = @"..\..\..\..\..\";

            string sourcePath = _sourcePath ?? srcRelativePathFromDebugDir;

            var files = new List<(string sysPath, string tarPath)>();

            AddRootFile(files, sourcePath, "DevMark.sln");
            AddRootFile(files, sourcePath, "LICENSE.txt");
            AddRootFile(files, sourcePath, "DevMarkWin.Dockerfile");
            AddRootFile(files, sourcePath, "DevMarkLinux.Dockerfile");
            AddRootFile(files, sourcePath, "DevMarkWinDevelopment.Dockerfile");
            AddRootFile(files, sourcePath, "DevMarkLinuxDevelopment.Dockerfile");

            foreach (var file in Directory.GetFiles(Path.Combine(sourcePath, "Source"), "*.*", SearchOption.AllDirectories))
            {
                var ignorePaths = new string[] { "\\bin\\", "\\obj\\", ".csproj.user", "\\nupkg\\" };

                if (ignorePaths.Any(x => file.Contains(x)))
                    continue;

                string sysPath = file;
                string archivePath = file.Substring(file.IndexOf("Source"));
                files.Add((sysPath, archivePath));
            }

            return files;
        }

        public string ResolveDockerfilePath(string dockerfile)
        {
            if (dockerfile.Contains("/") || dockerfile.Contains("\\"))
                return dockerfile;

            if (File.Exists(dockerfile))
                return Path.GetFullPath(dockerfile);

            return Path.GetFullPath(Path.Combine("Containers", dockerfile));
        }

        private void AddRootFile(List<(string sysPath, string tarPath)> files, string srcPathFromDebugDir, string filePath)
        {
            files.Add((Path.Combine(srcPathFromDebugDir, filePath), filePath));
        }
    }
}
