using System;

namespace DevMark.Model
{
    public class Benchmark
    {
        public int Iteration { get; set; }
        public DateTime? Timestamp { get; set; }
        public double? Duration { get; set; }
        public bool Success { get; set; }
    }
}
