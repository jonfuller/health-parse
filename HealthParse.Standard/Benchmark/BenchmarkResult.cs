using System;

namespace HealthParse.Standard.Benchmark
{
    public class BenchmarkResult<T> : BenchmarkResult
    {
        public T Value { get; }
        public BenchmarkResult(T value, TimeSpan elapsed) : base(elapsed)
        {
            Value = value;
        }
    }
    public class BenchmarkResult
    {
        public TimeSpan Elapsed { get; }

        public BenchmarkResult(TimeSpan elapsed)
        {
            Elapsed = elapsed;
        }
    }
}