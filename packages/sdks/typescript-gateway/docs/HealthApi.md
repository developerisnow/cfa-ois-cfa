# HealthApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**healthCheck**](HealthApi.md#healthcheck) | **GET** /health | Health check |



## healthCheck

> HealthStatus healthCheck()

Health check

### Example

```ts
import {
  Configuration,
  HealthApi,
} from '';
import type { HealthCheckRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new HealthApi();

  try {
    const data = await api.healthCheck();
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters

This endpoint does not need any parameter.

### Return type

[**HealthStatus**](HealthStatus.md)

### Authorization

No authorization required

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Service is healthy |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

