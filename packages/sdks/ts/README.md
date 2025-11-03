# OIS TypeScript SDKs

Generated TypeScript clients from OpenAPI specifications.

## Generation

```bash
npm install
npm run generate
```

## Usage

```typescript
import { DefaultApi } from '@ois/api-client';

const api = new DefaultApi({
  basePath: 'http://localhost:5000'
});

const response = await api.createIssuance({ ... });
```

