using System.Collections.Generic;

namespace DevMark.Core.Engine
{
    public class BenchmarkEngineExecutionResult
    {
        public BenchmarkEngineExecutionResult()
        {
            Success = true;
            Items = new List<BenchmarkResult>();
        }
        public void Add(BenchmarkResult result)
        {
            Success &= result.Command.Successfull;
            Items.Add(result);
        }

        public bool Success { get; set; }
        public List<BenchmarkResult> Items { get; set; }
    }
}
