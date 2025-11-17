# 🔧 Keycloak Setup Guide - Полная инструкция

## 📋 Статус диагностики

✅ **Keycloak работает** на http://localhost:8080  
✅ **Фронтенды запущены** на портах 3001, 3002, 3003  
✅ **.env.local файлы созданы** с правильной конфигурацией  
⚠️ **Требуется настройка клиентов в Keycloak Admin Console**

---

## 🚨 Главная проблема

**Ошибка:** `invalid_redirect_uri`  
**Причина:** NextAuth не знает, на каком порту работает приложение, потому что не была задана переменная `NEXTAUTH_URL`.

**Решение:** ✅ Созданы `.env.local` файлы с правильными `NEXTAUTH_URL`.

---

## 🔧 Что нужно сделать в Keycloak Admin Console

### Шаг 1: Войти в Admin Console

1. Откройте браузер: http://localhost:8080/admin
2. Введите:
   - **Username:** `admin`
   - **Password:** `admin`

### Шаг 2: Выбрать Realm

- В левом верхнем углу выберите realm: **ois-dev**

### Шаг 3: Настроить клиента Backoffice

1. **Clients** → найдите `backoffice` → кликните
2. Откройте вкладку **Settings**
3. В поле **Valid Redirect URIs** добавьте:
   ```
   http://localhost:3003/api/auth/callback/keycloak
   ```
4. В поле **Web Origins** добавьте:
   ```
   http://localhost:3003
   ```
5. Убедитесь, что:
   - ✅ **Access Type:** `confidential`
   - ✅ **Standard Flow Enabled:** `ON`
6. **Save**

### Шаг 4: Настроить клиента Portal Issuer

1. **Clients** → найдите `portal-issuer` → кликните
2. Откройте вкладку **Settings**
3. В поле **Valid Redirect URIs** добавьте:
   ```
   http://localhost:3001/api/auth/callback/keycloak
   ```
4. В поле **Web Origins** добавьте:
   ```
   http://localhost:3001
   ```
5. **Save**

### Шаг 5: Настроить клиента Portal Investor

1. **Clients** → найдите `portal-investor` → кликните
2. Откройте вкладку **Settings**
3. В поле **Valid Redirect URIs** добавьте:
   ```
   http://localhost:3002/api/auth/callback/keycloak
   ```
4. В поле **Web Origins** добавьте:
   ```
   http://localhost:3002
   ```
5. **Save**

---

## 👤 Создать тестового пользователя

1. В realm `ois-dev` → **Users** → **Add user**
2. Заполните:
   - **Username:** `test-user`
   - **Email:** `test@example.com`
   - **Email Verified:** `ON`
   - **First Name:** `Test`
   - **Last Name:** `User`
3. **Save**
4. Перейдите на вкладку **Credentials**
5. Нажмите **Set password**
6. Заполните:
   - **Password:** `test123`
   - **Password Confirmation:** `test123`
   - **Temporary:** `OFF` (чтобы не требовалось смены при первом входе)
7. **Save**

---

## 🔄 Перезапустить фронтенды

После создания `.env.local` файлов нужно перезапустить фронтенды, чтобы они подхватили новые переменные окружения.

### Остановить текущие процессы:
```bash
# Нажмите Ctrl+C в каждом терминале с запущенным фронтендом
```

### Запустить заново:

**Terminal 1 - Backoffice:**
```bash
cd apps/backoffice
npm run dev
```

**Terminal 2 - Portal Issuer:**
```bash
cd apps/portal-issuer
npm run dev
```

**Terminal 3 - Portal Investor:**
```bash
cd apps/portal-investor
npm run dev
```

---

## ✅ Проверка

1. Откройте http://localhost:3003 (Backoffice)
2. Нажмите **"Sign in with Keycloak"**
3. Должен быть редирект на Keycloak (http://localhost:8080)
4. Войдите с `test-user` / `test123`
5. Должен быть редирект обратно на http://localhost:3003 с успешной авторизацией

---

## 🔍 Если что-то не работает

### Проверить логи Keycloak:
```bash
docker logs ois-keycloak --tail 50 | Select-String "redirect"
```

### Проверить, что .env.local файлы на месте:
```bash
# Backoffice
Get-Content apps\backoffice\.env.local

# Portal Issuer
Get-Content apps\portal-issuer\.env.local

# Portal Investor
Get-Content apps\portal-investor\.env.local
```

### Проверить, что фронтенды запущены:
```bash
Get-NetTCPConnection -LocalPort 3001,3002,3003 -ErrorAction SilentlyContinue | Select-Object LocalPort, State
```

---

## 📝 Сводка изменений

✅ Созданы `.env.local` файлы для всех трех фронтендов:
- `apps/backoffice/.env.local` → `NEXTAUTH_URL=http://localhost:3003`
- `apps/portal-issuer/.env.local` → `NEXTAUTH_URL=http://localhost:3001`
- `apps/portal-investor/.env.local` → `NEXTAUTH_URL=http://localhost:3002`

⚠️ **Требуется ручная настройка в Keycloak Admin Console** (см. выше)

---

**После выполнения всех шагов авторизация должна работать!** 🎉

