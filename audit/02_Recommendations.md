# Recommendations — Меры и примеры

Дата: 2025-11-11 (UTC)

Формат: наблюдение → мера → пример → ожидаемый эффект → сложность (S/M/L)

## CI/CD

1) DinD/privileged → Kaniko
- Мера: заменить docker:dind на Kaniko (rootless), убрать docker.sock/privileged.
- Пример (фрагмент `.gitlab-ci.yml`):
```yaml
image: gcr.io/kaniko-project/executor:v1.23.2-debug
variables:
  DOCKER_CONFIG: /kaniko/.docker/
before_script:
  - mkdir -p /kaniko/.docker
  - |
    cat > /kaniko/.docker/config.json <<EOF
    {"auths":{"$CI_REGISTRY":{"username":"$CI_REGISTRY_USER","password":"$CI_REGISTRY_PASSWORD"}}}
    EOF
script:
  - >
    /kaniko/executor
    --context "$BUILD_CONTEXT"
    --dockerfile "$DOCKERFILE"
    --destination "$CI_REGISTRY_IMAGE/$IMAGE_NAME:$CI_COMMIT_SHA"
    --destination "$CI_REGISTRY_IMAGE/$IMAGE_NAME:$CI_COMMIT_REF_SLUG"
    --cache=true --cache-ttl=168h
```
- Эффект: безопасность ↑, отказ от privileged; совместимость с k8s runner.
- Сложность: M.

2) Удалить latest, внедрить immutable теги
- Мера: в Helm values использовать `tag: $CI_COMMIT_SHA`; для релизов — семвер-тег.
- Пример (CI патч манифестов GitOps):
```bash
sed -i "s/tag:.*/tag: \"$CI_COMMIT_SHA\"/" ops/infra/helm/api-gateway/values-*.yaml
git commit -m "chore: pin images to $CI_COMMIT_SHA [skip ci]" || true
```
- Эффект: воспроизводимость, корректные откаты.
- Сложность: S-M.

3) Trivy scan + SBOM
- Мера: добавить сканирование Dockerfile/образа и SBOM.
- Пример job:
```yaml
scan:trivy:
  stage: scan
  image: aquasec/trivy:0.51.0
  script:
    - trivy image --severity HIGH,CRITICAL --ignore-unfixed --exit-code 1 "$CI_REGISTRY_IMAGE/$IMAGE_NAME:$CI_COMMIT_SHA" || exit 1
    - trivy image --format cyclonedx --output sbom.cdx.json "$CI_REGISTRY_IMAGE/$IMAGE_NAME:$CI_COMMIT_SHA"
  artifacts:
    when: always
    paths: [sbom.cdx.json]
```
- Эффект: уязвимости контролируются; SBOM для лицензий.
- Сложность: S.

4) Rollback job (ArgoCD/Helm)
- Мера: добавить job для отката, сохранять helm diff.
- Пример:
```yaml
rollback:prod:
  stage: rollback
  image: argoproj/argocd:v2.12.3
  when: manual
  script:
    - argocd login "$ARGOCD_SERVER" --username "$ARGOCD_USER" --password "$ARGOCD_PASSWORD" --grpc-web --insecure
    - argocd app rollback ois-prod --grpc-web --revision "$REVISION"
```
- Эффект: MTTR ↓, управляемый откат.
- Сложность: S.

5) Review Apps
- Мера: включить environments c `on_stop` и TTL.
- Пример:
```yaml
deploy:review:
  environment:
    name: review/$CI_COMMIT_REF_SLUG
    url: https://$CI_COMMIT_REF_SLUG.review.example.com
    on_stop: stop:review
    auto_stop_in: 24 hours
```
- Эффект: изоляция, автоматическая очистка.
- Сложность: M.

## Kubernetes/Security

6) NetworkPolicy: deny-all + allow egress/ingress
- Мера: базовая deny-all и выборочные allow в namespace `ois-cfa`.
- Пример: см. `audit/09_Artifacts/k8s/networkpolicy.sample.yaml`.
- Эффект: сетевой zero-trust; уменьшение blast radius.
- Сложность: M.

7) PodSecurity (restricted)
- Мера: PSA уровня restricted и securityContext ужесточить: runAsNonRoot, readOnlyRootFilesystem=true, drop ALL, seccomp.
- Пример: см. `audit/09_Artifacts/k8s/podsecurity.sample.yaml`.
- Эффект: снижение RCE/escape рисков.
- Сложность: S-M.

8) HPA
- Мера: включить HPA по CPU (+ опц. кастомная метрика p95 latency).
- Пример: см. `audit/09_Artifacts/k8s/hpa.sample.yaml`.
- Эффект: эластичность под нагрузкой.
- Сложность: S.

9) Закрепить версии инструментов
- Мера: `argoproj/argocd:vX.Y.Z`, `bitnami/kubectl:1.30`, убрать `latest` у служебных образов.
- Пример: заменить в `.gitlab-ci.yml` images на pinned.
- Эффект: предсказуемость и безопасность.
- Сложность: S.

10) Secrets Management
- Мера: стандартизовать (SealedSecrets/ExternalSecrets/Vault). Исключить plain secrets из Git.
- Пример: внедрить ESO + SecretStore, описать в чартах.
- Эффект: снижение риска утечки.
- Сложность: M.

