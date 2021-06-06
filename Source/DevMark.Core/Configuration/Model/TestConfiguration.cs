using System.Collections.Generic;

namespace DevMark
{
    public class TestConfiguration
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public int WarmupIterations { get; set; }
        public int Iterations { get; set; }
        public int InitializationDelay { get; set; }
        public List<CommandConfiguration> Initializers { get; set; }
        public List<CommandConfiguration> Benchmark { get; set; }
        public List<CommandConfiguration> Cleanup { get; set; }
    }
}
