using System;
using System.Collections.Generic;
using System.IO;

namespace DevMark.Core.Container
{
    public class DockerfileFileProvider : IDockerfileFileProvider
    {
        public List<(string sysPath, string tarPath)> GetFilesForImage(string dockerfile)
        {
            return new List<(string sysPath, string tarPath)>() { (ResolveDockerfilePath(dockerfile), Path.GetFileName(dockerfile)) };
        }

        public string ResolveDockerfilePath(string dockerfile)
        {
            if (dockerfile.Contains("/") || dockerfile.Contains("\\"))
                return dockerfile;

            if (File.Exists(dockerfile))
                return Path.GetFullPath(dockerfile);

            return Path.GetFullPath(Path.Combine("Containers", dockerfile));
        }
    }
}
