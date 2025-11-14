# Chaincode Lifecycle Helm Chart

Helm chart для управления lifecycle chaincode в Hyperledger Fabric.

## Использование

### Установка

```bash
helm install chaincode-lifecycle ops/infra/helm/chaincode-lifecycle \
  --namespace fabric-network \
  --set imageRegistry=registry.gitlab.com/ois-cfa/ois-cfa \
  --set chaincode[0].packageId="" \
  --set chaincode[1].packageId=""
```

### Запуск Jobs

Jobs создаются как Kubernetes Jobs, но не запускаются автоматически. Запуск вручную:

```bash
# Install
kubectl create job --from=cronjob/chaincode-install-issuance-1.0 \
  chaincode-install-issuance-1.0-manual -n fabric-network

# После install, получить package ID из логов
PACKAGE_ID=$(kubectl logs -n fabric-network job/chaincode-install-issuance-1.0-peer0 | grep "Package ID" | awk '{print $3}')

# Обновить values с package ID
helm upgrade chaincode-lifecycle ops/infra/helm/chaincode-lifecycle \
  --set chaincode[0].packageId=$PACKAGE_ID

# Approve
kubectl create job --from=cronjob/chaincode-approve-issuance-1.0 \
  chaincode-approve-issuance-1.0-manual -n fabric-network

# Commit
kubectl create job --from=cronjob/chaincode-commit-issuance-1.0 \
  chaincode-commit-issuance-1.0-manual -n fabric-network
```

## Workflow

1. **Build** chaincode image (CI/CD)
2. **Install** на всех peers (Job)
3. **Approve** на всех peers (Job)
4. **Commit** в канал (Job)
5. **Invoke** через API Gateway

## Troubleshooting

```bash
# Проверить статус jobs
kubectl get jobs -n fabric-network | grep chaincode

# Логи install job
kubectl logs -n fabric-network job/chaincode-install-issuance-1.0-peer0

# Проверить установленный chaincode
kubectl exec -n fabric-network fabric-peer-0 -- \
  peer lifecycle chaincode queryinstalled
```

