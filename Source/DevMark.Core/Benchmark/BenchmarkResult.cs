namespace DevMark.Core.Engine
{
    public class BenchmarkResult
    {
        public int ScopeIndex { get; set; }
        public BenchmarkScope Scope { get; set; }
        public CommandBase Command { get; set; }
        public TestSuiteConfiguration SuiteConfig { get; set; }
        public TestConfiguration TestConfig { get; set; }
        public CommandConfigurationBase CommandConfiguration { get; set; }
    }
}
