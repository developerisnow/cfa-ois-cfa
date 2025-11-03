# РУКОВОДСТВО ПО УЧАСТИЮ В РАЗРАБОТКЕ
## ОИС ЦФА - Процессы разработки

**Версия:** {{VERSION}}  
**Дата:** {{DATE}}  
**Оператор:** {{COMPANY_NAME}}

---

## 1. ОБЗОР

### 1.1. Цели руководства

**Основные цели:**
- Стандартизация процессов разработки
- Обеспечение качества кода
- Упрощение сотрудничества
- Соблюдение требований безопасности

**Принципы:**
- Открытость и прозрачность
- Качество превыше скорости
- Безопасность по дизайну
- Непрерывное улучшение

### 1.2. Область применения

**Охватывает:**
- Backend разработку (.NET 8)
- Frontend разработку (Next.js 15)
- Chaincode разработку (Hyperledger Fabric)
- DevOps и инфраструктуру
- Тестирование и QA

**Участники:**
- Разработчики
- DevOps инженеры
- QA инженеры
- Архитекторы
- Менеджеры проектов

---

## 2. ПРОЦЕСС РАЗРАБОТКИ

### 2.1. Жизненный цикл

**Планирование:**
- Анализ требований
- Техническое проектирование
- Оценка сложности
- Планирование итераций

**Разработка:**
- Создание веток
- Написание кода
- Code review
- Тестирование

**Интеграция:**
- Merge в основную ветку
- Автоматические тесты
- Развертывание
- Мониторинг

### 2.2. Методология

**Agile/Scrum:**
- Спринты по 2 недели
- Ежедневные стендапы
- Планирование спринтов
- Ретроспективы

**DevOps:**
- Continuous Integration
- Continuous Deployment
- Infrastructure as Code
- Monitoring and Logging

---

## 3. УПРАВЛЕНИЕ КОДОМ

### 3.1. Git Workflow

**Основные ветки:**
- `main` - продакшн код
- `develop` - разработка
- `feature/*` - новые функции
- `hotfix/*` - критические исправления
- `release/*` - подготовка релизов

**Процесс:**
1. Создание feature ветки от develop
2. Разработка и коммиты
3. Создание Pull Request
4. Code review
5. Merge в develop
6. Тестирование
7. Merge в main

### 3.2. Commit Convention

**Формат:**
```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

**Типы:**
- `feat` - новая функция
- `fix` - исправление бага
- `docs` - документация
- `style` - форматирование
- `refactor` - рефакторинг
- `test` - тесты
- `chore` - обслуживание

**Примеры:**
```
feat(auth): add ESIA integration
fix(api): resolve timeout issue
docs(readme): update installation guide
test(unit): add user service tests
```

### 3.3. Pull Request Process

**Требования:**
- Описание изменений
- Связанные issues
- Скриншоты (для UI)
- Тесты
- Документация

**Template:**
```markdown
## Описание
Краткое описание изменений

## Тип изменений
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Тестирование
- [ ] Unit tests
- [ ] Integration tests
- [ ] Manual testing

## Чек-лист
- [ ] Код соответствует стандартам
- [ ] Тесты проходят
- [ ] Документация обновлена
- [ ] Безопасность проверена
```

---

## 4. СТАНДАРТЫ КОДА

### 4.1. .NET 8 / C# 12+

**Стиль кода:**
- Microsoft C# Coding Conventions
- EditorConfig для консистентности
- Analyzers для автоматической проверки
- Code formatting через dotnet format

**Пример:**
```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<User> GetUserAsync(int userId)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("User ID must be positive", nameof(userId));
        }

        _logger.LogInformation("Getting user with ID {UserId}", userId);
        
        var user = await _userRepository.GetByIdAsync(userId);
        return user ?? throw new UserNotFoundException($"User with ID {userId} not found");
    }
}
```

**Правила:**
- Использование async/await
- Proper exception handling
- Dependency injection
- Logging
- Input validation

### 4.2. Next.js 15 / TypeScript

**Стиль кода:**
- TypeScript strict mode
- ESLint + Prettier
- Functional components
- Hooks
- Type safety

**Пример:**
```typescript
interface User {
  id: number;
  name: string;
  email: string;
  role: UserRole;
}

interface UserCardProps {
  user: User;
  onEdit: (user: User) => void;
  onDelete: (userId: number) => void;
}

export const UserCard: React.FC<UserCardProps> = ({ user, onEdit, onDelete }) => {
  const handleEdit = useCallback(() => {
    onEdit(user);
  }, [user, onEdit]);

  const handleDelete = useCallback(() => {
    onDelete(user.id);
  }, [user.id, onDelete]);

  return (
    <div className="user-card">
      <h3>{user.name}</h3>
      <p>{user.email}</p>
      <div className="actions">
        <button onClick={handleEdit}>Edit</button>
        <button onClick={handleDelete}>Delete</button>
      </div>
    </div>
  );
};
```

### 4.3. Hyperledger Fabric Chaincode

**Стиль кода:**
- Go или TypeScript
- Proper error handling
- Input validation
- State management
- Event emission

**Пример (Go):**
```go
package main

import (
    "encoding/json"
    "fmt"
    "github.com/hyperledger/fabric-contract-api-go/contractapi"
)

type Asset struct {
    ID          string `json:"id"`
    Owner       string `json:"owner"`
    Amount      int    `json:"amount"`
    CreatedAt   string `json:"createdAt"`
}

type AssetContract struct {
    contractapi.Contract
}

func (ac *AssetContract) CreateAsset(ctx contractapi.TransactionContextInterface, id, owner string, amount int) error {
    if id == "" || owner == "" || amount <= 0 {
        return fmt.Errorf("invalid input parameters")
    }

    asset := Asset{
        ID:        id,
        Owner:     owner,
        Amount:    amount,
        CreatedAt: ctx.GetStub().GetTxTimestamp().AsTime().Format(time.RFC3339),
    }

    assetJSON, err := json.Marshal(asset)
    if err != nil {
        return fmt.Errorf("failed to marshal asset: %v", err)
    }

    return ctx.GetStub().PutState(id, assetJSON)
}
```

---

## 5. ТЕСТИРОВАНИЕ

### 5.1. Стратегия тестирования

**Пирамида тестирования:**
- Unit tests (70%)
- Integration tests (20%)
- E2E tests (10%)

**Типы тестов:**
- Unit tests
- Integration tests
- Contract tests
- Performance tests
- Security tests

### 5.2. Unit Testing

**Backend (.NET):**
```csharp
[Test]
public async Task GetUserAsync_ValidId_ReturnsUser()
{
    // Arrange
    var userId = 1;
    var expectedUser = new User { Id = userId, Name = "Test User" };
    _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
        .ReturnsAsync(expectedUser);

    // Act
    var result = await _userService.GetUserAsync(userId);

    // Assert
    Assert.That(result, Is.EqualTo(expectedUser));
    _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
}
```

**Frontend (React):**
```typescript
import { render, screen, fireEvent } from '@testing-library/react';
import { UserCard } from './UserCard';

describe('UserCard', () => {
  const mockUser: User = {
    id: 1,
    name: 'Test User',
    email: 'test@example.com',
    role: 'USER'
  };

  it('renders user information correctly', () => {
    render(<UserCard user={mockUser} onEdit={jest.fn()} onDelete={jest.fn()} />);
    
    expect(screen.getByText('Test User')).toBeInTheDocument();
    expect(screen.getByText('test@example.com')).toBeInTheDocument();
  });

  it('calls onEdit when edit button is clicked', () => {
    const onEdit = jest.fn();
    render(<UserCard user={mockUser} onEdit={onEdit} onDelete={jest.fn()} />);
    
    fireEvent.click(screen.getByText('Edit'));
    expect(onEdit).toHaveBeenCalledWith(mockUser);
  });
});
```

### 5.3. Integration Testing

**API Testing:**
```csharp
[Test]
public async Task CreateUser_ValidData_ReturnsCreatedUser()
{
    // Arrange
    var client = _factory.CreateClient();
    var userData = new { name = "Test User", email = "test@example.com" };

    // Act
    var response = await client.PostAsJsonAsync("/api/users", userData);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var user = await response.Content.ReadFromJsonAsync<User>();
    user.Name.Should().Be("Test User");
}
```

### 5.4. E2E Testing

**Playwright (Frontend):**
```typescript
import { test, expect } from '@playwright/test';

test('user can create new asset', async ({ page }) => {
  await page.goto('/dashboard');
  
  await page.click('[data-testid="create-asset-button"]');
  await page.fill('[data-testid="asset-name"]', 'Test Asset');
  await page.fill('[data-testid="asset-amount"]', '1000');
  await page.click('[data-testid="submit-button"]');
  
  await expect(page.locator('[data-testid="success-message"]')).toBeVisible();
  await expect(page.locator('[data-testid="asset-list"]')).toContainText('Test Asset');
});
```

---

## 6. БЕЗОПАСНОСТЬ

### 6.1. Secure Coding Practices

**Общие принципы:**
- Input validation
- Output encoding
- Authentication
- Authorization
- Encryption
- Logging

**Примеры:**
```csharp
// Input validation
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
    {
        return BadRequest("Invalid email format");
    }

    if (request.Age < 18 || request.Age > 120)
    {
        return BadRequest("Age must be between 18 and 120");
    }

    // Process request...
}

// SQL injection prevention
public async Task<User> GetUserAsync(int userId)
{
    var sql = "SELECT * FROM Users WHERE Id = @userId";
    return await _dbContext.Users
        .FromSqlRaw(sql, new SqlParameter("@userId", userId))
        .FirstOrDefaultAsync();
}
```

### 6.2. Dependency Scanning

**Автоматическое сканирование:**
- Dependabot для .NET
- npm audit для Node.js
- Snyk для комплексного анализа
- OWASP Dependency Check

**Конфигурация:**
```yaml
# .github/dependabot.yml
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "weekly"
  - package-ecosystem: "npm"
    directory: "/frontend"
    schedule:
      interval: "weekly"
```

---

## 7. ДОКУМЕНТАЦИЯ

### 7.1. Код документация

**XML Documentation:**
```csharp
/// <summary>
/// Сервис для управления пользователями
/// </summary>
public class UserService : IUserService
{
    /// <summary>
    /// Получает пользователя по идентификатору
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns>Данные пользователя</returns>
    /// <exception cref="ArgumentException">Когда userId меньше или равен 0</exception>
    /// <exception cref="UserNotFoundException">Когда пользователь не найден</exception>
    public async Task<User> GetUserAsync(int userId)
    {
        // Implementation...
    }
}
```

**JSDoc (TypeScript):**
```typescript
/**
 * Сервис для работы с пользователями
 */
export class UserService {
  /**
   * Получает пользователя по идентификатору
   * @param userId - Идентификатор пользователя
   * @returns Promise с данными пользователя
   * @throws {Error} Когда пользователь не найден
   */
  async getUser(userId: number): Promise<User> {
    // Implementation...
  }
}
```

### 7.2. API документация

**OpenAPI/Swagger:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    /// <summary>
    /// Создает нового пользователя
    /// </summary>
    /// <param name="request">Данные пользователя</param>
    /// <returns>Созданный пользователь</returns>
    /// <response code="201">Пользователь успешно создан</response>
    /// <response code="400">Некорректные данные</response>
    [HttpPost]
    [ProducesResponseType(typeof(User), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        // Implementation...
    }
}
```

---

## 8. CI/CD

### 8.1. GitHub Actions

**Workflow для .NET:**
```yaml
name: .NET CI

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
    
    - name: Security scan
      run: dotnet list package --vulnerable
```

**Workflow для Frontend:**
```yaml
name: Frontend CI

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup Node.js
      uses: actions/setup-node@v3
      with:
        node-version: '18'
        cache: 'npm'
        cache-dependency-path: frontend/package-lock.json
    
    - name: Install dependencies
      run: cd frontend && npm ci
    
    - name: Lint
      run: cd frontend && npm run lint
    
    - name: Type check
      run: cd frontend && npm run type-check
    
    - name: Test
      run: cd frontend && npm run test
    
    - name: Build
      run: cd frontend && npm run build
```

### 8.2. Quality Gates

**Требования для merge:**
- Все тесты проходят
- Code coverage > 80%
- No critical vulnerabilities
- Code review approved
- Security scan passed

**Автоматические проверки:**
- Linting
- Type checking
- Unit tests
- Integration tests
- Security scanning
- Performance testing

---

## 9. РАЗВЕРТЫВАНИЕ

### 9.1. Environments

**Development:**
- Локальная разработка
- Hot reload
- Debug режим
- Mock данные

**Testing:**
- Автоматические тесты
- Синтетические данные
- Изолированная среда
- Быстрое развертывание

**Staging:**
- Production-like среда
- Реальные данные (анонимизированные)
- Полное тестирование
- Performance тестирование

**Production:**
- Высокая доступность
- Мониторинг
- Backup
- Disaster recovery

### 9.2. Deployment Strategy

**Blue-Green Deployment:**
- Два идентичные среды
- Переключение трафика
- Быстрый откат
- Zero downtime

**Canary Deployment:**
- Постепенное развертывание
- Мониторинг метрик
- Автоматический откат
- A/B тестирование

---

## 10. МОНИТОРИНГ

### 10.1. Application Monitoring

**Метрики:**
- Response time
- Throughput
- Error rate
- Resource usage
- Business metrics

**Инструменты:**
- Prometheus + Grafana
- Application Insights
- Jaeger (tracing)
- ELK Stack (logs)

### 10.2. Health Checks

**Backend (.NET):**
```csharp
public class HealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check database connection
            await _dbContext.Database.CanConnectAsync(cancellationToken);
            
            // Check external services
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            response.EnsureSuccessStatusCode();
            
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Health check failed", ex);
        }
    }
}
```

**Frontend (Next.js):**
```typescript
// pages/api/health.ts
export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  try {
    // Check database connection
    await db.raw('SELECT 1');
    
    // Check external services
    const response = await fetch('https://api.example.com/health');
    if (!response.ok) throw new Error('External service unavailable');
    
    res.status(200).json({ status: 'healthy', timestamp: new Date().toISOString() });
  } catch (error) {
    res.status(503).json({ status: 'unhealthy', error: error.message });
  }
}
```

---

## 11. ПРОБЛЕМЫ И РЕШЕНИЯ

### 11.1. Частые проблемы

**Performance:**
- Медленные запросы к БД
- Неэффективные алгоритмы
- Отсутствие кэширования
- Большие bundle размеры

**Security:**
- SQL injection
- XSS атаки
- CSRF атаки
- Небезопасные зависимости

**Quality:**
- Отсутствие тестов
- Плохая документация
- Нечитаемый код
- Технический долг

### 11.2. Решения

**Performance:**
- Оптимизация запросов
- Индексы БД
- Кэширование
- Code splitting

**Security:**
- Input validation
- Output encoding
- CSRF tokens
- Dependency updates

**Quality:**
- Автоматические тесты
- Code review
- Документация
- Рефакторинг

---

## 12. РЕСУРСЫ И ИНСТРУМЕНТЫ

### 12.1. Документация

**Внутренняя:**
- Архитектурные решения (ADR)
- API документация
- Руководства по развертыванию
- Troubleshooting guides

**Внешняя:**
- .NET документация
- Next.js документация
- Hyperledger Fabric docs
- Kubernetes документация

### 12.2. Инструменты

**Разработка:**
- Visual Studio / VS Code
- Git
- Docker
- Kubernetes

**Тестирование:**
- xUnit / NUnit
- Jest / Vitest
- Playwright
- k6

**Мониторинг:**
- Prometheus
- Grafana
- Jaeger
- ELK Stack

---

## 13. КОНТАКТЫ И ПОДДЕРЖКА

### 13.1. Команда разработки

**Tech Lead:**
- Email: tech-lead@{{COMPANY_DOMAIN}}
- Slack: @tech-lead

**Architecture Team:**
- Email: architecture@{{COMPANY_DOMAIN}}
- Slack: #architecture

**DevOps Team:**
- Email: devops@{{COMPANY_DOMAIN}}
- Slack: #devops

### 13.2. Каналы коммуникации

**Slack каналы:**
- #general - общие вопросы
- #development - разработка
- #architecture - архитектура
- #devops - инфраструктура
- #security - безопасность

**Meetings:**
- Daily standup: 9:00 AM
- Architecture review: Вторник, 2:00 PM
- Security review: Четверг, 3:00 PM
- Retrospective: Пятница, 4:00 PM

---

**Дата создания:** {{DATE}}  
**Автор:** {{AUTHOR}}  
**Статус:** Утверждено  
**Версия:** {{VERSION}}  
**Следующий пересмотр:** {{NEXT_REVIEW_DATE}}
