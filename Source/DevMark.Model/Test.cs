using System.Collections.Generic;

namespace DevMark.Model
{
    public class Test
    {
        public Test()
        {
            Benchmarks = new List<Benchmark>();
        }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public List<Benchmark> Benchmarks { get; set; }
    }
}
