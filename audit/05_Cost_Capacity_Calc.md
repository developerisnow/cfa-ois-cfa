# Cost & Capacity — Расчёты емкости и стоимости

Дата: 2025-11-11 (UTC)

Методика

1) Собираем по namespace и сервисам: replicaCount, requests (cpu, memory), limits (cpu, memory), storage (PVC), сетевой трафик.
2) Суммируем по средам (dev/stage/prod):
   - CPU_req_total = Σ (replicaCount_i × cpu_request_i)
   - RAM_req_total = Σ (replicaCount_i × mem_request_i)
   - CPU_lim_total = Σ (replicaCount_i × cpu_limit_i)
   - RAM_lim_total = Σ (replicaCount_i × mem_limit_i)
3) Узлы: подбираем по requests (с запасом 20–30%) и учётом overhead (kube-system, ingress, мониторинг).
4) Хранение: Σ PVC + резерв 20%.
5) Сеть: egress/ingress — оценка по метрикам (UNKNOWN → предложить сбор через Prometheus/NGINX Ingress Controller).
6) Стоимость = узлы + storage + трафик + IP/сертификаты (шаблон без тарифов).

Предварительные данные (из репозитория; требуется валидация)

- api-gateway values (примеры):
  - dev: requests 250m/256Mi, limits 500m/512Mi, replicas=1
  - staging: requests 500m/512Mi, limits 1000m/1Gi, replicas=2
  - prod: requests 1000m/1Gi, limits 2000m/2Gi, replicas=3
- Остальные сервисы: UNKNOWN (предложить аналогичный сбор из values.*.yaml)

Расчёт (пример только для api-gateway)

1) Dev
- CPU_req_total = 1 × 250m = 250m
- RAM_req_total = 1 × 256Mi ≈ 256Mi

2) Staging
- CPU_req_total = 2 × 500m = 1000m
- RAM_req_total = 2 × 512Mi ≈ 1024Mi (1Gi)

3) Prod
- CPU_req_total = 3 × 1000m = 3000m (3 vCPU)
- RAM_req_total = 3 × 1Gi = 3Gi

Общая методика подбора узлов

- Пусть Node_type = vCPU=X, RAM=Y GiB.
- N_dev ≥ ceil(CPU_req_total_dev / X, RAM_req_total_dev / Y)
- N_stage ≥ ceil(...)
- N_prod ≥ ceil( (Σ CPU_req_total всех сервисов × 1.2) / X, (Σ RAM_req_total × 1.2) / Y)
- Добавить overhead: +1 узел для отказоустойчивости (минимум 3 в проде).

Сценарии роста

- +20% нагрузки:
  - CPU_req'_total = CPU_req_total × 1.2
  - RAM_req'_total = RAM_req_total × 1.2
- +50% нагрузки:
  - Умножить на 1.5 и проверить порог масштабирования HPA/Cluster Autoscaler.

Стоимость (шаблон расчёта)

- Узлы: N × price_per_node_month (по тарифу провайдера)
- Storage: Σ(PVC_Gi × price_per_Gi_month)
- Трафик: egress_GB × price_per_GB
- IP/Certs: количество × цена

Рекомендации по емкости

- Включить HPA на бизнес-сервисах (cpu 70–80%, mem 80%).
- Рассмотреть VPA для рекомендаций requests.
- Включить Cluster Autoscaler (минимум/максимум группы узлов по средам).
- Провести rightsizing после 14 дней метрик.

Данные для заполнения (запрос)

- Перечень Deployments/StatefulSets и их values (dev/stage/prod): replicaCount, requests/limits, PVC.
- Фактический тип узлов и их спецификации.
- Средние/пиковые метрики CPU/RAM/Network (Prometheus), 7–14 дней.

