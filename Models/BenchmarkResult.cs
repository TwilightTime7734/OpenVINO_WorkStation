namespace WorkStationC_.Models
{
    internal sealed class BenchmarkResult
    {
        public DateTimeOffset CompletedAtUtc { get; init; }
        public string Label { get; init; } = string.Empty;
        public string ModelPath { get; init; } = string.Empty;
        public string Device { get; init; } = string.Empty;
        public string PerformanceHint { get; init; } = string.Empty;
        public int Iterations { get; init; }
        public int Warmup { get; init; }
        public int Runs { get; init; }
        public int SampleCount { get; init; }
        public double ReadMilliseconds { get; init; }
        public double CompileMilliseconds { get; init; }
        public double FirstInferenceMilliseconds { get; init; }
        public double MinimumMilliseconds { get; init; }
        public double AverageMilliseconds { get; init; }
        public double MedianMilliseconds { get; init; }
        public double P90Milliseconds { get; init; }
        public double P95Milliseconds { get; init; }
        public double P99Milliseconds { get; init; }
        public double MaximumMilliseconds { get; init; }
        public double ThroughputPerSecond { get; init; }
        public IReadOnlyList<double> SamplesMilliseconds { get; init; } = Array.Empty<double>();
    }
}
