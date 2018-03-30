using System;
using System.Diagnostics;

namespace HealthParse.Standard.Benchmark
{
    public static class Benchmark
    {
        public static BenchmarkResult<T> Time<T>(Func<T> toRun)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var value = toRun();
            stopWatch.Stop();

            return new BenchmarkResult<T>(value, stopWatch.Elapsed);
        }

        public static BenchmarkResult Time(Action toRun)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            toRun();
            stopWatch.Stop();

            return new BenchmarkResult(stopWatch.Elapsed);
        }
    }
}