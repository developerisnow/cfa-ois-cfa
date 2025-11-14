# Risk Register

Дата: 2025-11-11 (UTC)

| Риск | Вероятность | Влияние | Владелец | План реакций |
|---|---:|---:|---|---|
| Privileged runner с docker.sock | Высокая | Высокое | DevOps | Перевести сборки на Kaniko; отключить privileged и hostPath; ограничить RBAC runner |
| Образы с тегом latest | Высокая | Высокое | DevOps | Ввести immutable теги ($CI_COMMIT_SHA); запрет latest; политики проверок в CI |
| Отсутствие baseline NetworkPolicy | Средняя | Высокое | DevOps | Ввести deny-all + allow; DNS egress; ingress к нужным портам |
| Недостаточный PodSecurity | Средняя | Высокое | DevOps | PSA restricted; readOnlyRootFilesystem=true; seccomp RuntimeDefault |
| Нет формализованного rollback | Средняя | Среднее | SRE | Добавить rollback job (ArgoCD/Helm) + runbook |
| Нет сканирования образов в fail mode | Средняя | Среднее | AppSec/DevOps | Trivy HIGH,CRITICAL = fail; отчёты в artifacts |
| Статический прометеус скрейп | Низкая | Среднее | SRE | Перейти на ServiceMonitor; PrometheusRule |
| Неопределённая система секретов | Средняя | Высокое | SecOps | Выбрать и внедрить SealedSecrets/ExternalSecrets/Vault |

Замечания: пересмотр вероятностей после получения kubeconfig и последних 10 пайплайнов.

