using System.Diagnostics.Metrics;

namespace OIS.Compliance.Infrastructure;

public static class Metrics
{
    public const string MeterName = "compliance-service";
    private static readonly Meter Meter = new(MeterName);

    public static readonly Histogram<double> RequestDurationMs = Meter.CreateHistogram<double>(
        name: "request_duration_ms",
        unit: "ms",
        description: "API request latency in milliseconds");

    public static readonly Counter<long> RequestErrors = Meter.CreateCounter<long>(
        name: "request_errors_total",
        unit: "requests",
        description: "Number of API requests resulting in 5xx");
}

