using System.Collections.Generic;

namespace DevMark
{
    public class NPMDepdendencyConfiguration : BaseDependencyVersionConfiguration
    {
        public List<NPMGlobalModuleConfiguration> GlobalModules { get; set; }
    }
}
