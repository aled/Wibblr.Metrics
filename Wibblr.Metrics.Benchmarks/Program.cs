using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Wibblr.Metrics.Core;
using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Benchmarks
{
    public class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<CountingBenchmark>();
        }
    }

    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    public class CountingBenchmark
    {
        MetricsCollector metrics;
        string[] names;
        Random random = new Random();
        IMetricsSink sink;

        [GlobalSetup]
        public void Setup()
        {
            names = Enumerable.Range(0, 50)
                              .Select(x => Guid.NewGuid().ToString())
                              .ToArray();

            sink = new TextWriterSink(Console.Out, new LineSerializer());
            metrics = new MetricsCollector(sink, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            foreach (var name in names)
                metrics.RegisterThresholds(name, new[] { 0, 10, 20, 30, 50, 75, 100 });
        }

        [GlobalCleanup]
        public void Cleanup() =>
            metrics.Dispose();

        [Benchmark]
        public void IncrementCounter() => 
            metrics.IncrementCounter(names[random.Next() % 50]);

        [Benchmark]
        public void IncrementBucket() =>
            metrics.IncrementBucket(names[random.Next() % 50], random.Next() % 100);
    }
}
