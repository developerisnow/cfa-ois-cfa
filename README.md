# ОИС ЦФА - Оператор информационной системы цифровых финансовых активов

**Версия:** 1.0.0-MVP  
**Дата:** 2025-01-XX  
**Оператор:** {{COMPANY_NAME}} (ОГРН: {{OGRN}}, ИНН: {{INN}})

---

## 📋 ОБЗОР ПРОЕКТА

ОИС ЦФА - это комплексная информационная система для выпуска, учета и обращения цифровых финансовых активов в соответствии с требованиями Федерального закона № 259-ФЗ.

### 🎯 Основные функции MVP

- **Выпуск ЦФА** - создание и публикация цифровых финансовых активов
- **Покупка ЦФА** - размещение заказов инвесторами
- **Выплаты** - выполнение выплат по расписанию
- **Погашение** - погашение выпуска

---

## 🚀 БЫСТРЫЙ СТАРТ

### Предварительные требования

- .NET 9 SDK
- Node.js 20+
- Docker & Docker Compose
- Go 1.21+ (для chaincode)

### Установка и запуск

```bash
# 1. Клонирование
git clone <repo-url>
cd capital

# 2. Запуск инфраструктуры
make docker-up
# или
docker-compose up -d

# 3. Проверка здоровья сервисов
make health

# 4. Валидация спецификаций
make validate-specs

# 5. Загрузка демо-данных
make seed
```

---

## 📚 СПЕЦИФИКАЦИИ (Spec-First)

Все API контракты определены в `/packages/contracts`:

### OpenAPI (REST)
- `openapi-gateway.yaml` - Gateway API (основные endpoints)
- `openapi-identity.yaml` - Identity Service (OIDC)
- `openapi-integrations-esia.yaml` - ESIA Adapter
- `openapi-integrations-bank.yaml` - Bank Nominal
- `openapi-integrations-edo.yaml` - EDO Connector

### AsyncAPI (Events)
- `asyncapi.yaml` - Kafka события

### JSON Schemas
- `schemas/CFA.json` - Цифровой финансовый актив
- `schemas/Issuance.json` - Выпуск
- `schemas/Order.json` - Заказ
- `schemas/Payout.json` - Выплата
- `schemas/AuditEvent.json` - Событие аудита

---

## 🔗 SWAGGER URLs

После запуска `docker-compose up`:

- **Gateway**: http://localhost:5000/swagger
- **Identity**: http://localhost:5001/swagger
- **ESIA Adapter**: http://localhost:5002/swagger
- **Bank Nominal**: http://localhost:5003/swagger
- **EDO Connector**: http://localhost:5004/swagger

---

## 🧪 ТЕСТИРОВАНИЕ

```bash
# Unit tests
make test

# E2E tests (Playwright)
make e2e

# Load tests (k6)
make load

# Contract tests (Pact)
cd tests/contracts && npm test
```

---

## 🔄 ГЕНЕРАЦИЯ SDK

SDK генерируются из OpenAPI спецификаций:

```bash
# Установить openapi-generator-cli
npm install -g @openapitools/openapi-generator-cli

# Генерировать SDK
make generate-sdks
```

SDK будут в `/packages/sdks/`:

- `typescript-gateway/` - TypeScript клиент для Gateway API

---

## 📁 СТРУКТУРА ПРОЕКТА

```
/apps
  /portal-issuer      - Next.js 15 (эмитент)
  /portal-investor    - Next.js 15 (инвестор)
  /backoffice         - Next.js 15 (админка)
  /api-gateway        - ASP.NET Core (YARP)

/services
  /identity           - .NET 9 (OIDC/аутентификация)
  /issuance           - .NET 9 (выпуск ЦФА)
  /registry           - .NET 9 (реестр/трансферы)
  /settlement         - .NET 9 (выплаты)
  /compliance         - .NET 9 (KYC/AML)
  /integrations
    /esia-adapter     - .NET 9 (ЕСИА mock)
    /bank-nominal     - .NET 9 (банк mock)
    /edo-connector    - .NET 9 (ЭДО mock)

/chaincode
  /issuance           - Go (HLF chaincode)
  /registry           - Go (HLF chaincode)

/packages
  /contracts          - OpenAPI/AsyncAPI/JSON Schemas
  /sdks               - Автогенерированные клиенты

/tests
  /e2e                - Playwright
  /contracts          - Pact
  /services           - xUnit
  /load               - k6

/ops
  /infra              - K8s/Helm
  /ci                 - GitHub Actions
```

---

## 🛠️ КОМАНДЫ (Makefile)

```bash
make help              # Список всех команд
make install           # Установить зависимости
make build             # Собрать все проекты
make test              # Запустить тесты
make lint              # Линтинг
make validate-specs    # Валидация OpenAPI/AsyncAPI/JSON
make seed              # Загрузить демо-данные
make e2e               # E2E тесты
make load              # Нагрузочные тесты
make docker-up         # Запустить docker-compose
make docker-down       # Остановить docker-compose
make generate-sdks     # Генерировать SDK
```

---

## 🔐 БЕЗОПАСНОСТЬ

⚠️ **ВАЖНО**: В dev окружении используются mock-сервисы и простые пароли.  
Для production требуется:
- Vault для секретов
- mTLS между сервисами
- HSM для ключей
- Полная интеграция с ЕСИА/банком/ЭДО

---

## 📝 ЛОГИ И АУДИТ

- Логи: Serilog (JSON формат) → stdout
- Аудит: События в Kafka (`ois.audit.logged`)
- Трейсинг: OpenTelemetry (планируется)

---

## 🐛 TROUBLESHOOTING

### Сервисы не стартуют

```bash
# Проверить логи
docker-compose logs -f <service-name>

# Проверить здоровье
curl http://localhost:5000/health
```

### База данных не доступна

```bash
# Пересоздать базу
docker-compose down -v
docker-compose up -d postgres
sleep 5
docker-compose up -d
```

---

## 📖 ДОКУМЕНТАЦИЯ

- Архитектура: `/docs/architecture/`
- Правила ИС: `/docs/legal/01-ПравилаИС-template.md`
- Описание ИС: `/docs/legal/02-ОписаниеИС-template.md`

---

## 🔄 CHANGELOG

### 1.0.0-MVP (2025-01-XX)
- ✅ Monorepo структура
- ✅ Spec-first: OpenAPI/AsyncAPI/JSON Schemas
- ✅ Docker Compose инфраструктура
- ✅ API Gateway (YARP)
- ✅ Identity Service skeleton
- ⏳ Остальные сервисы (в разработке)
- ⏳ Chaincode (в разработке)
- ⏳ Frontends (в разработке)

---

## 📄 ЛИЦЕНЗИЯ

Проприетарное ПО. Все права защищены.

---

## 👥 КОНТАКТЫ

- Техподдержка: support@example.com
- Архитектор: architect@example.com
