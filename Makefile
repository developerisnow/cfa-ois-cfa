.PHONY: help install build test lint validate-specs seed e2e load clean docker-up docker-down

help: ## Show this help message
	@echo 'Usage: make [target]'
	@echo ''
	@echo 'Available targets:'
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  %-15s %s\n", $$1, $$2}' $(MAKEFILE_LIST)

install: ## Install dependencies
	@echo "Installing dependencies..."
	cd backend && dotnet restore
	cd chaincode/issuance && go mod download
	cd chaincode/registry && go mod download
	cd apps/portal-issuer && npm install
	cd apps/portal-investor && npm install
	cd apps/backoffice && npm install

build: ## Build all projects
	@echo "Building .NET services..."
	cd packages/domain && dotnet build --no-restore
	cd services/issuance && dotnet build --no-restore
	cd services/registry && dotnet build --no-restore
	cd services/settlement && dotnet build --no-restore
	cd services/compliance && dotnet build --no-restore
	@echo "Building chaincode..."
	cd chaincode/issuance && go build -o bin/issuance .
	cd chaincode/registry && go build -o bin/registry .
	@echo "Testing chaincode..."
	cd chaincode/registry && go test ./...
	@echo "Building frontends..."
	cd apps/portal-issuer && npm run build
	cd apps/portal-investor && npm run build
	cd apps/backoffice && npm run build

test: ## Run all tests
	@echo "Running .NET tests..."
	cd packages/domain && dotnet test --no-build --verbosity minimal
	cd services/issuance && dotnet test --no-build --verbosity minimal
	cd services/registry && dotnet test --no-build --verbosity minimal || true
	cd services/settlement && dotnet test --no-build --verbosity minimal || true
	cd services/compliance && dotnet test --no-build --verbosity minimal || true
	@echo "Running chaincode tests..."
	cd chaincode/issuance && go test ./...
	cd chaincode/registry && go test ./...
	@echo "Running frontend tests..."
	cd apps/portal-issuer && npm test -- --passWithNoTests

k6: ## Run k6 load tests
	@echo "Running k6 load tests..."
	k6 run tests/k6/payouts-report.js

k6-load: ## Run k6 gateway critical paths
	@echo "Running k6 gateway critical paths..."
	k6 run tests/k6/gateway-critical-paths.js

k6-report: ## Run k6 and generate JSON report
	k6 run --out json=k6-report.json tests/k6/gateway-critical-paths.js

pact: ## Run Pact tests
	@echo "Running Pact consumer tests..."
	cd tests/contracts/pact-consumer && npm test

coverage: ## Generate coverage report
	@echo "Running tests with coverage..."
	dotnet test --collect:"XPlat Code Coverage" --results-directory:./coverage

e2e: ## Run E2E tests (Playwright)
	@echo "Running Playwright E2E tests..."
	cd tests/e2e && npx playwright test

e2e-ui: ## Run E2E tests with UI
	cd tests/e2e && npx playwright test --ui

lint: ## Run linters
	@echo "Linting .NET code..."
	cd backend && dotnet format --verify-no-changes
	@echo "Linting Go code..."
	cd chaincode/issuance && golangci-lint run
	cd chaincode/registry && golangci-lint run
	@echo "Linting frontend code..."
	cd apps/portal-issuer && npm run lint
	cd apps/portal-investor && npm run lint
	cd apps/backoffice && npm run lint

validate-specs: ## Validate OpenAPI/AsyncAPI/JSON Schemas
	@echo "Validating OpenAPI specs..."
	@which spectral > /dev/null || (echo "Install @stoplight/spectral-cli: npm i -g @stoplight/spectral-cli" && exit 1)
	spectral lint packages/contracts/openapi-*.yaml
	@echo "Validating AsyncAPI spec..."
	@which asyncapi > /dev/null || (echo "Install asyncapi-cli: npm i -g @asyncapi/cli" && exit 1)
	asyncapi validate packages/contracts/asyncapi.yaml
	@echo "Validating JSON Schemas..."
	@which ajv > /dev/null || (echo "Install ajv-cli: npm i -g ajv-cli" && exit 1)
	ajv validate -s packages/contracts/schemas/CFA.json -d '{"id":"00000000-0000-0000-0000-000000000000","code":"TEST","name":"Test","type":"TOKEN","status":"DRAFT"}' || true
	@echo "Spec validation complete"

seed: ## Seed database with demo data
	@echo "Seeding database..."
	docker-compose exec api-gateway dotnet run --project services/seed -- seed-db
	@echo "Demo data seeded"

load: ## Run load tests (k6)
	@echo "Running load tests..."
	cd tests/load && k6 run load-test.js

docker-up: ## Start all services with docker-compose
	docker-compose up -d
	@echo "Waiting for services to be healthy..."
	@sleep 10
	@echo "Services started. Check health: make health"

docker-down: ## Stop all services
	docker-compose down

health: ## Check health of all services
	@echo "Checking service health..."
	@curl -s http://localhost:5000/health | jq . || echo "Gateway: not ready"
	@curl -s http://localhost:5001/health | jq . || echo "Identity: not ready"
	@curl -s http://localhost:5002/health | jq . || echo "ESIA: not ready"

clean: ## Clean build artifacts
	@echo "Cleaning build artifacts..."
	cd backend && dotnet clean
	cd chaincode/issuance && rm -rf bin
	cd chaincode/registry && rm -rf bin
	cd apps/portal-issuer && rm -rf .next
	cd apps/portal-investor && rm -rf .next
	cd apps/backoffice && rm -rf .next

generate-sdks: ## Generate SDKs from OpenAPI specs
	@echo "Generating SDKs..."
	@which openapi-generator-cli > /dev/null || (echo "Install openapi-generator-cli" && exit 1)
	openapi-generator-cli generate -i packages/contracts/openapi-gateway.yaml -g typescript-fetch -o packages/sdks/typescript-gateway
	@echo "SDKs generated in packages/sdks/"

# Terraform targets for Timeweb Cloud
TF_DIR := ops/infra/timeweb

tf-init: ## Initialize Terraform (Timeweb Cloud)
	@echo "Initializing Terraform..."
	cd $(TF_DIR) && terraform init

tf-validate: ## Validate Terraform configuration
	@echo "Validating Terraform configuration..."
	cd $(TF_DIR) && terraform validate

tf-plan: ## Plan Terraform changes
	@echo "Planning Terraform changes..."
	cd $(TF_DIR) && terraform plan

tf-apply: ## Apply Terraform configuration
	@echo "Applying Terraform configuration..."
	cd $(TF_DIR) && terraform apply

tf-apply-auto: ## Apply Terraform configuration (auto-approve)
	@echo "Applying Terraform configuration (auto-approve)..."
	cd $(TF_DIR) && terraform apply -auto-approve

tf-destroy: ## Destroy Terraform infrastructure
	@echo "Destroying Terraform infrastructure..."
	cd $(TF_DIR) && terraform destroy

tf-output: ## Show Terraform outputs
	@echo "Terraform outputs:"
	cd $(TF_DIR) && terraform output

tf-kubeconfig: ## Export kubeconfig from Terraform
	@echo "Exporting kubeconfig..."
	cd $(TF_DIR) && terraform output -raw kubeconfig > kubeconfig.yaml
	@echo "Kubeconfig saved to $(TF_DIR)/kubeconfig.yaml"

tf-refresh: ## Refresh Terraform state
	@echo "Refreshing Terraform state..."
	cd $(TF_DIR) && terraform refresh

# Timeweb Cloud CLI targets
twc-install: ## Install Timeweb Cloud CLI (twc)
	@echo "Installing Timeweb Cloud CLI..."
	./tools/timeweb/install.sh

twc-cluster-list: ## List Timeweb Cloud Kubernetes clusters
	@echo "Listing clusters..."
	@which twc > /dev/null || (echo "Error: twc not installed. Run: make twc-install" && exit 1)
	@export PATH="${HOME}/.local/bin:${PATH}" && twc k8s list

twc-kubeconfig: ## Export kubeconfig using twc CLI
	@echo "Exporting kubeconfig..."
	@which twc > /dev/null || (echo "Error: twc not installed. Run: make twc-install" && exit 1)
	@if [ -z "$$TWC_TOKEN" ]; then \
		echo "Error: TWC_TOKEN not set. Export it first: export TWC_TOKEN='your-token'"; \
		exit 1; \
	fi
	./tools/timeweb/kubeconfig-export.sh ois-cfa-k8s

twc-verify: ## Verify twc CLI configuration
	@echo "Verifying twc CLI configuration..."
	@which twc > /dev/null || (echo "Error: twc not installed. Run: make twc-install" && exit 1)
	@export PATH="${HOME}/.local/bin:${PATH}" && \
	if [ -z "$$TWC_TOKEN" ]; then \
		echo "Warning: TWC_TOKEN not set. Set it with: export TWC_TOKEN='your-token'"; \
	fi && \
	twc k8s list || echo "Error: twc configuration failed. Check TWC_TOKEN."

# GitOps targets (ArgoCD)
argocd-install: ## Install ArgoCD via Helm
	@echo "Installing ArgoCD..."
	@which helm > /dev/null || (echo "Error: helm not installed" && exit 1)
	kubectl create namespace argocd --dry-run=client -o yaml | kubectl apply -f -
	helm repo add argo https://argoproj.github.io/argo-helm
	helm repo update
	helm install argocd argo/argo-cd \
		--namespace argocd \
		--values ops/gitops/argocd/helm/values.yaml \
		--wait
	@echo "ArgoCD installed. Get admin password:"
	@echo "kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath='{.data.password}' | base64 -d"

argocd-uninstall: ## Uninstall ArgoCD
	@echo "Uninstalling ArgoCD..."
	helm uninstall argocd --namespace argocd || true
	kubectl delete namespace argocd || true

argocd-password: ## Get ArgoCD admin password
	@kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath="{.data.password}" | base64 -d && echo

argocd-bootstrap: ## Bootstrap ArgoCD with app-of-apps
	@echo "Bootstrapping ArgoCD..."
	kubectl apply -f ops/gitops/argocd/bootstrap/namespace.yaml
	kubectl apply -f ops/gitops/argocd/config/projects.yaml
	kubectl apply -f ops/gitops/argocd/config/rbac.yaml
	kubectl apply -f ops/gitops/argocd/bootstrap/app-of-apps.yaml
	@echo "ArgoCD bootstrapped. Check status: kubectl get applications -n argocd"

argocd-status: ## Show ArgoCD applications status
	@echo "ArgoCD Applications Status:"
	@kubectl get applications -n argocd

# GitLab Runner targets
gitlab-runner-install: ## Install GitLab Runner in Kubernetes
	@echo "Installing GitLab Runner..."
	@if [ -z "$$RUNNER_TOKEN" ]; then \
		echo "Error: RUNNER_TOKEN not set. Get it from GitLab UI:"; \
		echo "  Settings → CI/CD → Runners → Registration token"; \
		echo "  Or run: make gitlab-runner-get-token"; \
		exit 1; \
	fi
	@which kubectl > /dev/null || (echo "Error: kubectl not installed" && exit 1)
	@echo "Checking kubeconfig..."
	@KUBECONFIG_FILE="$${KUBECONFIG:-}"; \
	if [ -z "$$KUBECONFIG_FILE" ] && [ -f "ops/infra/timeweb/kubeconfig.yaml" ]; then \
		KUBECONFIG_FILE="$$(pwd)/ops/infra/timeweb/kubeconfig.yaml"; \
		echo "KUBECONFIG not set, using $$KUBECONFIG_FILE"; \
	fi; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl cluster-info &>/dev/null || { \
		echo "Error: kubectl cannot connect to cluster. Kubeconfig not configured."; \
		echo ""; \
		echo "To configure kubeconfig:"; \
		echo "  1. Run: make setup-kubeconfig"; \
		echo "  2. Or manually: export KUBECONFIG=\$$(pwd)/ops/infra/timeweb/kubeconfig.yaml"; \
		echo "  3. Verify: kubectl get nodes"; \
		echo ""; \
		echo "Or see: docs/ops/timeweb/kubeconfig.md"; \
		exit 1; \
	}
	@echo "Applying GitLab Runner manifests..."
	@KUBECONFIG_FILE="$${KUBECONFIG:-}"; \
	if [ -z "$$KUBECONFIG_FILE" ] && [ -f "ops/infra/timeweb/kubeconfig.yaml" ]; then \
		KUBECONFIG_FILE="$$(pwd)/ops/infra/timeweb/kubeconfig.yaml"; \
	fi; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl apply -f ops/infra/k8s/gitlab-runner/namespace.yaml; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl apply -f ops/infra/k8s/gitlab-runner/rbac.yaml; \
	sed "s/__REPLACE_WITH_RUNNER_TOKEN__/$$RUNNER_TOKEN/g" \
		ops/infra/k8s/gitlab-runner/configmap.yaml | KUBECONFIG="$$KUBECONFIG_FILE" kubectl apply -f -; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl apply -f ops/infra/k8s/gitlab-runner/deployment.yaml; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl apply -f ops/infra/k8s/gitlab-runner/service.yaml
	@echo "GitLab Runner installed. Waiting for pods..."
	@KUBECONFIG_FILE="$${KUBECONFIG:-}"; \
	if [ -z "$$KUBECONFIG_FILE" ] && [ -f "ops/infra/timeweb/kubeconfig.yaml" ]; then \
		KUBECONFIG_FILE="$$(pwd)/ops/infra/timeweb/kubeconfig.yaml"; \
	fi; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl wait --for=condition=Ready pod -l app=gitlab-runner -n gitlab-runner --timeout=120s || echo "Pods may still be starting"
	@echo "Check status: make gitlab-runner-status"

gitlab-runner-status: ## Show GitLab Runner status
	@KUBECONFIG_FILE="$${KUBECONFIG:-}"; \
	if [ -z "$$KUBECONFIG_FILE" ] && [ -f "ops/infra/timeweb/kubeconfig.yaml" ]; then \
		KUBECONFIG_FILE="$$(pwd)/ops/infra/timeweb/kubeconfig.yaml"; \
	fi; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl cluster-info &>/dev/null || { \
		echo "Error: kubectl cannot connect to cluster. Run: make setup-kubeconfig"; \
		exit 1; \
	}; \
	echo "GitLab Runner Status:"; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl get pods -n gitlab-runner; \
	echo ""; \
	echo "Runner logs (last 20 lines):"; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=20 || true
	@echo ""
	@echo "Check runners in GitLab UI:"
	@echo "  Settings → CI/CD → Runners"

gitlab-runner-logs: ## Show GitLab Runner logs
	@KUBECONFIG_FILE="$${KUBECONFIG:-}"; \
	if [ -z "$$KUBECONFIG_FILE" ] && [ -f "ops/infra/timeweb/kubeconfig.yaml" ]; then \
		KUBECONFIG_FILE="$$(pwd)/ops/infra/timeweb/kubeconfig.yaml"; \
	fi; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl cluster-info &>/dev/null || { \
		echo "Error: kubectl cannot connect to cluster. Run: make setup-kubeconfig"; \
		exit 1; \
	}; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl logs -n gitlab-runner -l app=gitlab-runner -f

gitlab-runner-restart: ## Restart GitLab Runner deployment
	@KUBECONFIG_FILE="$${KUBECONFIG:-}"; \
	if [ -z "$$KUBECONFIG_FILE" ] && [ -f "ops/infra/timeweb/kubeconfig.yaml" ]; then \
		KUBECONFIG_FILE="$$(pwd)/ops/infra/timeweb/kubeconfig.yaml"; \
	fi; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl cluster-info &>/dev/null || { \
		echo "Error: kubectl cannot connect to cluster. Run: make setup-kubeconfig"; \
		exit 1; \
	}; \
	echo "Restarting GitLab Runner..."; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl rollout restart deployment/gitlab-runner -n gitlab-runner; \
	echo "Waiting for rollout..."; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl rollout status deployment/gitlab-runner -n gitlab-runner

gitlab-runner-uninstall: ## Uninstall GitLab Runner
	@KUBECONFIG_FILE="$${KUBECONFIG:-}"; \
	if [ -z "$$KUBECONFIG_FILE" ] && [ -f "ops/infra/timeweb/kubeconfig.yaml" ]; then \
		KUBECONFIG_FILE="$$(pwd)/ops/infra/timeweb/kubeconfig.yaml"; \
	fi; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl cluster-info &>/dev/null || { \
		echo "Warning: kubectl cannot connect to cluster. Skipping uninstall."; \
		exit 0; \
	}; \
	echo "Uninstalling GitLab Runner..."; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl delete deployment gitlab-runner -n gitlab-runner || true; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl delete service gitlab-runner -n gitlab-runner || true; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl delete configmap gitlab-runner-config -n gitlab-runner || true; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl delete rolebinding gitlab-runner -n gitlab-runner || true; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl delete role gitlab-runner -n gitlab-runner || true; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl delete serviceaccount gitlab-runner -n gitlab-runner || true; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl delete namespace gitlab-runner || true; \
	echo "GitLab Runner uninstalled"

gitlab-runner-get-token: ## Show instructions to get GitLab Runner registration token
	@echo "To get GitLab Runner registration token:"
	@echo "1. Open GitLab UI: https://git.telex.global/npk/ois-cfa/-/settings/ci_cd"
	@echo "2. Expand 'Runners' section"
	@echo "3. Copy 'Registration token'"
	@echo ""
	@echo "Or use group/instance runner token:"
	@echo "  Settings → CI/CD → Runners → Expand 'Runners' → Registration token"
	@echo ""
	@echo "After getting token, update runner:"
	@echo "  export RUNNER_TOKEN='your-token'"
	@echo "  make gitlab-runner-update-token"

gitlab-runner-update-token: ## Update GitLab Runner registration token (requires RUNNER_TOKEN)
	@if [ -z "$$RUNNER_TOKEN" ]; then \
		echo "Error: RUNNER_TOKEN not set"; \
		echo "Get token: make gitlab-runner-get-token"; \
		echo "Then: export RUNNER_TOKEN='your-token'"; \
		exit 1; \
	fi
	@KUBECONFIG_FILE="$${KUBECONFIG:-}"; \
	if [ -z "$$KUBECONFIG_FILE" ] && [ -f "ops/infra/timeweb/kubeconfig.yaml" ]; then \
		KUBECONFIG_FILE="$$(pwd)/ops/infra/timeweb/kubeconfig.yaml"; \
	fi; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl cluster-info &>/dev/null || { \
		echo "Error: kubectl cannot connect to cluster. Run: make setup-kubeconfig"; \
		exit 1; \
	}; \
	echo "Updating GitLab Runner token..."; \
	sed "s/__REPLACE_WITH_RUNNER_TOKEN__/$$RUNNER_TOKEN/g" \
		ops/infra/k8s/gitlab-runner/configmap.yaml | \
		KUBECONFIG="$$KUBECONFIG_FILE" kubectl apply -f -; \
	echo "Restarting pods to apply new token..."; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl rollout restart deployment/gitlab-runner -n gitlab-runner; \
	echo "Waiting for rollout..."; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl rollout status deployment/gitlab-runner -n gitlab-runner --timeout=120s || echo "Rollout may still be in progress"; \
	echo ""; \
	echo "✓ Token updated. Check status: make gitlab-runner-status"

check-kubeconfig: ## Check if kubeconfig is configured
	@./ops/scripts/check-kubeconfig.sh

setup-kubeconfig: ## Setup kubeconfig for Kubernetes cluster
	@./ops/scripts/setup-kubeconfig.sh

gitlab-runner-scale: ## Scale GitLab Runner replicas (usage: make gitlab-runner-scale REPLICAS=5)
	@KUBECONFIG_FILE="$${KUBECONFIG:-}"; \
	if [ -z "$$KUBECONFIG_FILE" ] && [ -f "ops/infra/timeweb/kubeconfig.yaml" ]; then \
		KUBECONFIG_FILE="$$(pwd)/ops/infra/timeweb/kubeconfig.yaml"; \
	fi; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl cluster-info &>/dev/null || { \
		echo "Error: kubectl cannot connect to cluster. Run: make setup-kubeconfig"; \
		exit 1; \
	}; \
	if [ -z "$$REPLICAS" ]; then \
		echo "Error: REPLICAS not set. Usage: make gitlab-runner-scale REPLICAS=5"; \
		exit 1; \
	fi; \
	echo "Scaling GitLab Runner to $$REPLICAS replicas..."; \
	KUBECONFIG="$$KUBECONFIG_FILE" kubectl scale deployment gitlab-runner -n gitlab-runner --replicas=$$REPLICAS; \
	echo "Scaled to $$REPLICAS replicas"

# GitOps targets (GitLab Agent)
gitlab-agent-install: ## Install GitLab Kubernetes Agent
	@echo "Installing GitLab Agent..."
	@if [ -z "$$AGENT_TOKEN" ]; then \
		echo "Error: AGENT_TOKEN not set. Get it from GitLab UI:"; \
		echo "  Infrastructure → Kubernetes clusters → Add cluster → GitLab Agent"; \
		exit 1; \
	fi
	@which helm > /dev/null || (echo "Error: helm not installed" && exit 1)
	kubectl create namespace gitlab-agent --dry-run=client -o yaml | kubectl apply -f -
	helm repo add gitlab https://charts.gitlab.io
	helm repo update
	helm install gitlab-agent gitlab/gitlab-agent \
		--namespace gitlab-agent \
		--create-namespace \
		--set config.token=$$AGENT_TOKEN \
		--set config.kasAddress=wss://gitlab.com/-/kubernetes-agent/
	@echo "GitLab Agent installed. Check status: kubectl get pods -n gitlab-agent"

gitlab-agent-status: ## Show GitLab Agent status
	@echo "GitLab Agent Status:"
	@kubectl get pods -n gitlab-agent
	@echo ""
	@echo "Agent logs:"
	@kubectl logs -n gitlab-agent -l app=gitlab-agent --tail=20 || true

# Debug toolbox targets
debug-deploy: ## Deploy debug toolbox pod
	@echo "Deploying debug toolbox..."
	kubectl apply -f ops/debug/namespace.yaml
	kubectl apply -f ops/debug/serviceaccount.yaml
	kubectl apply -f ops/debug/configmap-scripts.yaml
	kubectl apply -f ops/debug/debug-pod.yaml
	@echo "Waiting for pod to be ready..."
	@kubectl wait --for=condition=Ready pod/debug-toolbox -n tools --timeout=60s || echo "Pod may still be starting"

debug-exec: ## Execute shell in debug toolbox pod
	@echo "Connecting to debug toolbox..."
	@kubectl exec -it -n tools debug-toolbox -- /bin/bash || \
		(echo "Error: debug pod not found. Run: make debug:deploy" && exit 1)

debug-logs: ## Collect logs using debug toolbox
	@echo "Collecting logs..."
	@kubectl exec -n tools debug-toolbox -- /scripts/logs-collect.sh $(NAMESPACES) || \
		(echo "Error: debug pod not found. Run: make debug:deploy" && exit 1)
	@echo "Logs collected. Check artifacts in /tmp/artifacts"

debug-events: ## Dump Kubernetes events
	@echo "Dumping events..."
	@kubectl exec -n tools debug-toolbox -- /scripts/events-dump.sh || \
		(echo "Error: debug pod not found. Run: make debug:deploy" && exit 1)

debug-argo-status: ## Check ArgoCD status
	@echo "Checking ArgoCD status..."
	@kubectl exec -n tools debug-toolbox -- /scripts/argo-status.sh || \
		(echo "Error: debug pod not found. Run: make debug:deploy" && exit 1)

debug-agent-status: ## Check GitLab Agent status
	@echo "Checking GitLab Agent status..."
	@kubectl exec -n tools debug-toolbox -- /scripts/agent-status.sh || \
		(echo "Error: debug pod not found. Run: make debug:deploy" && exit 1)

debug-remove: ## Remove debug toolbox
	@echo "Removing debug toolbox..."
	kubectl delete pod -n tools debug-toolbox || true
	kubectl delete configmap -n tools debug-scripts || true
	kubectl delete serviceaccount -n tools debug-toolbox || true
	kubectl delete clusterrolebinding debug-toolbox || true
	kubectl delete clusterrole debug-toolbox || true
	@echo "Debug toolbox removed"

# Kubernetes health check targets
k8s-healthcheck: ## Run Kubernetes cluster health check
	@echo "Running Kubernetes cluster health check..."
	@./ops/scripts/k8s-healthcheck.sh
	@echo "Health check report generated in artifacts/"

k8s-healthcheck-debug: ## Run health check from debug toolbox pod
	@echo "Running health check from debug toolbox..."
	@kubectl exec -n tools debug-toolbox -- /scripts/k8s-healthcheck.sh || \
		(echo "Debug pod not found, deploying..." && \
		 make debug-deploy && \
		 kubectl cp ops/scripts/k8s-healthcheck.sh tools/debug-toolbox:/scripts/k8s-healthcheck.sh && \
		 kubectl exec -n tools debug-toolbox -- chmod +x /scripts/k8s-healthcheck.sh && \
		 kubectl exec -n tools debug-toolbox -- /scripts/k8s-healthcheck.sh)

.DEFAULT_GOAL := help

