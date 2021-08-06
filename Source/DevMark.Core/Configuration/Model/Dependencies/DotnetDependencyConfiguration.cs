using System.Collections.Generic;

namespace DevMark
{
    public class DotnetDependencyConfiguration : BaseDependencyVersionConfiguration
    {
        public List<BaseDependencyVersionConfiguration> Sdks { get; set; }
        public List<DotnetRuntimeDependencyConfiguration> Runtimes { get; set; }
    }
}
