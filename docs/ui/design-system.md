# OIS Design System

**Версия:** 1.0.0  
**Дата:** 2025-11-02

---

## 📦 Структура

Дизайн-система ОИС состоит из двух основных частей:

1. **`apps/_theme`** — токены дизайна (Tailwind preset + CSS variables)
2. **`apps/shared-ui`** — переиспользуемые React компоненты

---

## 🎨 Токены дизайна

### Цвета

#### Фоновые цвета
- `background` — основной фон
- `background-alt` — альтернативный фон

#### Поверхности
- `surface` — основная поверхность
- `surface-alt` — альтернативная поверхность
- `surface-hover` — поверхность при наведении

#### Основной цвет (Primary)
- `primary-50` ... `primary-900` — градация синего

#### Семантические цвета
- `success` (зелёный) — успешные операции
- `warning` (оранжевый) — предупреждения
- `danger` (красный) — ошибки
- `info` (голубой) — информационные сообщения

#### Текст
- `text-primary` — основной текст
- `text-secondary` — вторичный текст
- `text-tertiary` — третичный текст
- `text-disabled` — отключённый текст

#### Границы
- `border` — основная граница
- `border-focus` — граница при фокусе

#### Палитра для графиков (8 цветов)
- `chart-1` ... `chart-8` — цвета для визуализации данных

### Радиусы

- `radius-none`: 0
- `radius-sm`: 0.25rem
- `radius-md`: 0.5rem
- `radius-lg`: 0.75rem
- `radius-xl`: 1rem
- `radius-2xl`: 1.5rem
- `radius-full`: 9999px

### Тени

- `shadow-sm` — маленькая тень
- `shadow-md` — средняя тень (по умолчанию)
- `shadow-lg` — большая тень
- `shadow-xl` — очень большая тень
- `shadow-2xl` — максимальная тень
- `shadow-inner` — внутренняя тень

### Z-Index

- `z-0` ... `z-100` — базовые уровни
- `z-dropdown`: 1000
- `z-sticky`: 1020
- `z-fixed`: 1030
- `z-modal`: 1040
- `z-popover`: 1050
- `z-tooltip`: 1060

---

## 🧩 Компоненты

### Layout

#### `AppShell`
Основная обёртка приложения с хедером, сайдбаром и футером.

```tsx
import { AppShell } from '@ois/shared-ui';

<AppShell
  user={session?.user}
  onSignOut={() => signOut()}
  sidebar={{
    items: [
      { label: 'Dashboard', href: '/dashboard', icon: <HomeIcon /> },
      { label: 'Issuances', href: '/issuances', icon: <FileIcon /> },
    ],
  }}
>
  {children}
</AppShell>
```

#### `PageHeader`
Заголовок страницы с хлебными крошками и действиями.

```tsx
import { PageHeader } from '@ois/shared-ui';

<PageHeader
  title="Issuances"
  description="Manage your CFA issuances"
  breadcrumbs={[
    { label: 'Home', href: '/' },
    { label: 'Issuances' },
  ]}
  actions={<Button>Create New</Button>}
/>
```

### Data Display

#### `StatCard`
Карточка с метрикой.

```tsx
import { StatCard } from '@ois/shared-ui';
import { TrendingUp } from 'lucide-react';

<StatCard
  title="Total Issuances"
  value={42}
  description="Active this month"
  icon={TrendingUp}
  trend={{
    value: 12,
    isPositive: true,
    label: "vs last month"
  }}
/>
```

#### `KPIGrid`
Сетка карточек с метриками.

```tsx
import { KPIGrid } from '@ois/shared-ui';

<KPIGrid
  columns={3}
  items={[
    { title: 'Active', value: 10 },
    { title: 'Total', value: 42 },
    { title: 'Revenue', value: '₽1.2M' },
  ]}
/>
```

#### `DataTable`
Таблица с сортировкой, фильтрацией и пагинацией.

```tsx
import { DataTable } from '@ois/shared-ui';
import { ColumnDef } from '@tanstack/react-table';

const columns: ColumnDef<Issuance>[] = [
  {
    accessorKey: 'id',
    header: 'ID',
  },
  {
    accessorKey: 'status',
    header: 'Status',
  },
];

<DataTable
  columns={columns}
  data={issuances}
  searchable
  pageSize={10}
/>
```

### Charts

#### `LineChart`
Линейный график.

```tsx
import { LineChart } from '@ois/shared-ui';

<LineChart
  data={salesData}
  lines={[
    { dataKey: 'sales', name: 'Sales', color: '#3b82f6' },
    { dataKey: 'revenue', name: 'Revenue', color: '#22c55e' },
  ]}
  title="Sales Over Time"
  height={300}
/>
```

#### `BarChart`
Столбчатый график.

```tsx
import { BarChart } from '@ois/shared-ui';

<BarChart
  data={monthlyData}
  bars={[
    { dataKey: 'issued', name: 'Issued' },
    { dataKey: 'redeemed', name: 'Redeemed' },
  ]}
  title="Monthly Activity"
/>
```

#### `PieChart`
Круговая диаграмма.

```tsx
import { PieChart } from '@ois/shared-ui';

<PieChart
  data={[
    { name: 'Active', value: 60, color: '#22c55e' },
    { name: 'Closed', value: 40, color: '#ef4444' },
  ]}
  title="Issuance Status"
/>
```

### Forms

#### `OrderForm`
Форма размещения ордера.

```tsx
import { OrderForm } from '@ois/shared-ui';

<OrderForm
  onSubmit={async (data) => {
    await placeOrder(data);
  }}
  onCancel={() => router.back()}
  isLoading={isPending}
/>
```

### Feedback

#### `EmptyState`
Пустое состояние.

```tsx
import { EmptyState } from '@ois/shared-ui';
import { Inbox } from 'lucide-react';

<EmptyState
  icon={Inbox}
  title="No issuances"
  description="Create your first issuance to get started"
  action={<Button>Create Issuance</Button>}
/>
```

#### `Skeleton`
Загрузочный плейсхолдер.

```tsx
import { Skeleton } from '@ois/shared-ui';

<Skeleton className="h-4 w-full" variant="text" />
<Skeleton className="h-12 w-12" variant="circular" />
<Skeleton className="h-32 w-full" variant="rectangular" />
```

### Timeline

#### `Timeline`
Временная шкала событий.

```tsx
import { Timeline } from '@ois/shared-ui';

<Timeline
  items={[
    {
      id: '1',
      title: 'Issuance created',
      description: 'By John Doe',
      timestamp: new Date(),
      status: 'success',
    },
  ]}
/>
```

#### `AuditLog`
Журнал аудита.

```tsx
import { AuditLog } from '@ois/shared-ui';

<AuditLog
  entries={[
    {
      id: '1',
      actor: 'admin@example.com',
      action: 'create',
      entity: 'issuance',
      entityId: '123',
      timestamp: new Date(),
      ip: '192.168.1.1',
    },
  ]}
/>
```

### Widgets

#### `MiniTicker`
Мини-тикер с метриками.

```tsx
import { MiniTicker } from '@ois/shared-ui';

<MiniTicker
  items={[
    { label: 'Total', value: '₽10M' },
    { label: 'Active', value: 42, change: { value: 5, isPositive: true } },
  ]}
/>
```

---

## 🌓 Темы

Поддерживаются три темы:
- `light` (по умолчанию)
- `dark`
- `light-alt`

### Использование ThemeProvider

```tsx
import { ThemeProvider } from '@ois/shared-ui';

<ThemeProvider defaultTheme="light">
  <App />
</ThemeProvider>
```

### Переключение темы

```tsx
import { useTheme } from '@ois/shared-ui';

function ThemeToggle() {
  const { theme, setTheme, toggleTheme } = useTheme();
  
  return (
    <button onClick={toggleTheme}>
      Switch to {theme === 'dark' ? 'light' : 'dark'}
    </button>
  );
}
```

---

## ♿ Доступность

Все компоненты следуют принципам WCAG AA:

- ✅ Focus states для всех интерактивных элементов
- ✅ Keyboard navigation
- ✅ ARIA labels и roles
- ✅ Semantic HTML
- ✅ Sufficient color contrast

### Примеры

```tsx
// Правильно: aria-label для кнопок без текста
<button aria-label="Close modal">
  <XIcon />
</button>

// Правильно: role для списков
<div role="list">
  <div role="listitem">Item 1</div>
</div>

// Правильно: aria-describedby для ошибок
<input
  aria-invalid={!!error}
  aria-describedby={error ? 'error-id' : undefined}
/>
```

---

## 📦 Установка и использование

### 1. Подключение темы в портале

В `tailwind.config.ts`:

```ts
import preset from '../_theme/tailwind-preset.js';

export default {
  presets: [preset],
  // ...
};
```

В `globals.css`:

```css
@import '../../_theme/tokens.css';
```

### 2. Использование компонентов

```tsx
import { AppShell, PageHeader, StatCard } from '@ois/shared-ui';
```

**Примечание:** Для работы необходимо настроить пути импорта в `tsconfig.json` или использовать относительные пути.

---

## 🧪 Тестирование

### Lighthouse

Целевые показатели:
- Performance: ≥85
- Accessibility: ≥85
- Best Practices: ≥85
- SEO: ≥85

### Команды

```bash
# Запуск Storybook (опционально)
npm run storybook --prefix apps/shared-ui

# Lighthouse CI
npm run lh
```

---

## 📚 Дополнительные ресурсы

- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [Recharts Documentation](https://recharts.org/)
- [TanStack Table](https://tanstack.com/table)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)

---

**Последнее обновление:** 2025-11-02

