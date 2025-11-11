# Keycloak Setup for Local Development

## ✅ Quick Start

### 1. Создание базы данных (ВАЖНО!)

Keycloak требует отдельную базу данных. Создайте её перед запуском:

```bash
docker exec ois-postgres psql -U ois -d postgres -c "CREATE DATABASE keycloak;"
```

### 2. Запуск Keycloak

```bash
docker-compose up -d keycloak
```

**Важно:** Первый запуск может занять 1-2 минуты для инициализации схемы БД (создание 94+ таблиц).

### 3. Доступ к админке

- **URL:** http://localhost:8080/admin
- **Username:** `admin`
- **Password:** `admin`

Проверка готовности:
```bash
# Health check
curl http://localhost:8080/health/ready

# Или просто откройте в браузере
http://localhost:8080/admin
```

## Create Realm: `ois-dev`

1. После входа в админку, нажмите на выпадающий список вверху слева (показывает "Master")
2. Выберите **"Create Realm"**
3. Name: `ois-dev`
4. **Save**

## Create Clients

### portal-issuer
1. В realm `ois-dev`, перейдите: **Clients** → **Create Client**
2. Client ID: `portal-issuer`
3. Client Protocol: `openid-connect`
4. **Next**
5. Access Type: `public`
6. Valid Redirect URIs: `http://localhost:3001/*`
7. Web Origins: `http://localhost:3001`
8. **Save**

### portal-investor
1. Clients → Create Client
2. Client ID: `portal-investor`
3. Client Protocol: `openid-connect`
4. **Next**
5. Access Type: `public`
6. Valid Redirect URIs: `http://localhost:3002/*`
7. Web Origins: `http://localhost:3002`
8. **Save**

### backoffice
1. Clients → Create Client
2. Client ID: `backoffice`
3. Client Protocol: `openid-connect`
4. **Next**
5. Access Type: `public`
6. Valid Redirect URIs: `http://localhost:3003/*`
7. Web Origins: `http://localhost:3003`
8. **Save**

## Create Roles

1. В realm `ois-dev`, перейдите: **Realm Roles** → **Create Role**
2. Создайте следующие роли (по одной):
   - `issuer`
   - `investor`
   - `admin`
   - `backoffice`

## Create Test Users

### Issuer User
1. **Users** → **Create User**
2. Username: `issuer@test.com`
3. Email: `issuer@test.com`
4. Email Verified: `ON`
5. **Create**
6. Вкладка **Credentials**:
   - Password: `password123`
   - Password Confirmation: `password123`
   - Temporary: `OFF`
   - **Save**
7. Вкладка **Role Mappings**:
   - Assign role: `issuer`
   - **Assign**

### Investor User
1. Users → Create User
2. Username: `investor@test.com`
3. Email: `investor@test.com`
4. Email Verified: `ON`
5. Create
6. Credentials: Password `password123`, Temporary: OFF
7. Role Mappings: Assign role `investor`

### Admin User
1. Users → Create User
2. Username: `admin@test.com`
3. Email: `admin@test.com`
4. Email Verified: `ON`
5. Create
6. Credentials: Password `password123`, Temporary: OFF
7. Role Mappings: Assign roles `admin` и `backoffice`

## Troubleshooting

### База данных не существует
```bash
docker exec ois-postgres psql -U ois -d postgres -c "CREATE DATABASE keycloak;"
docker-compose restart keycloak
```

### Keycloak не запускается
```bash
# Проверьте логи
docker logs ois-keycloak --tail 50

# Проверьте статус
docker ps --filter "name=keycloak"
```

### Не могу зайти в админку
- Убедитесь, что используете правильный URL: http://localhost:8080/admin (не просто /)
- Проверьте, что контейнер запущен: `docker ps`
- Проверьте health: `curl http://localhost:8080/health/ready`

---

**Note:** Для production используйте правильные секреты и HTTPS.
