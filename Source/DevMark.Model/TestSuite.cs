using System.Collections.Generic;

namespace DevMark.Model
{
    public class TestSuite
    {
        public TestSuite()
        {
            Tests = new List<Test>();
            Dependencies = new List<Dependency>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ConfigIdentifier { get; set; }
        public string Version { get; set; }
        public List<Test> Tests { get; set; }
        public List<Dependency> Dependencies { get; set; }
        public string Author { get; set; }
    }
}
