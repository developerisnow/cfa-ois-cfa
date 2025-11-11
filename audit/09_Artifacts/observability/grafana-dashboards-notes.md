# Grafana Dashboards — Notes

Minimum set

- API Gateway: RPS, p95/p99 latency, 4xx/5xx, upstream saturation
- Service dashboards: CPU/Mem, GC (.NET), DB queries/sec, EF Core timings
- Infra: Nodes (CPU/mem/disk pressure), HPA scaling events
- Traces: top slow spans, MassTransit consumers, EF Core queries

Links and imports

- Import k8s cluster dashboards (kube-state-metrics)
- Import .NET runtime dashboards (dotnet exporter or OTLP-to-prom)

