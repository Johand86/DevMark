using System.Collections.Generic;

namespace DevMark
{
    public class VisualStudioDependencyConfiguration : BaseDependencyVersionConfiguration
    {
        public List<VisualStudioDependencyComponentSetConfiguration> ComponentSets { get; set; }
    }

    public class VisualStudioDependencyComponentSetConfiguration
    {
        public List<string> Components { get; set; }
    }
}
