using System.Diagnostics.Metrics;

namespace OIS.Settlement.Infrastructure;

public static class Metrics
{
    public const string MeterName = "settlement-service";
    private static readonly Meter Meter = new(MeterName);

    public static readonly Counter<long> EventsProcessed = Meter.CreateCounter<long>(
        name: "events_processed_total",
        unit: "events",
        description: "Number of events successfully processed");

    public static readonly Counter<long> EventsFailed = Meter.CreateCounter<long>(
        name: "events_failed_total",
        unit: "events",
        description: "Number of events failed to process");

    public static readonly Counter<long> OutboxPublished = Meter.CreateCounter<long>(
        name: "outbox_published_total",
        unit: "messages",
        description: "Number of outbox messages published to Kafka");
}

