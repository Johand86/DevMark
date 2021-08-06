using System.Collections.Generic;

namespace DevMark
{
    public class TestSuiteConfiguration
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public List<EnvironmentConfiguration> Environment { get; set; }
        public List<DependencyCommandConfiguration> Dependencies { get; set; }
        public List<CommandConfiguration> Initializers { get; set; }
        public List<TestConfiguration> Tests { get; set; }
        public List<CommandConfiguration> Cleanup { get; set; }
    }
}
