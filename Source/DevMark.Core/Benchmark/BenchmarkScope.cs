namespace DevMark.Core.Engine
{
    public enum BenchmarkScope
    {
        Dependency,
        SuitInit,
        TestInit,
        Benchmark,
        BenchmarkWarmup,
        TestCleanup,
        SuitCleanup,
    }
}
