# SettlementApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**runSettlement**](SettlementApi.md#runsettlement) | **POST** /v1/settlement/run | Run settlement for a specific date |



## runSettlement

> SettlementResponse runSettlement(date)

Run settlement for a specific date

### Example

```ts
import {
  Configuration,
  SettlementApi,
} from '';
import type { RunSettlementRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new SettlementApi(config);

  const body = {
    // Date | Date to run settlement for (YYYY-MM-DD). Defaults to today. (optional)
    date: 2013-10-20,
  } satisfies RunSettlementRequest;

  try {
    const data = await api.runSettlement(body);
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters


| Name | Type | Description  | Notes |
|------------- | ------------- | ------------- | -------------|
| **date** | `Date` | Date to run settlement for (YYYY-MM-DD). Defaults to today. | [Optional] [Defaults to `undefined`] |

### Return type

[**SettlementResponse**](SettlementResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **202** | Settlement accepted |  -  |
| **400** | Bad request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

