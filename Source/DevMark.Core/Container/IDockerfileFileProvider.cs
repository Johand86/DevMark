using System.Collections.Generic;

namespace DevMark.Core.Container
{
    public interface IDockerfileFileProvider
    {
        List<(string sysPath, string tarPath)> GetFilesForImage(string dockerfile);
        string ResolveDockerfilePath(string dockerfile);
    }
}