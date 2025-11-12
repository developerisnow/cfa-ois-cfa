# WalletsApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**getWallet**](WalletsApi.md#getwallet) | **GET** /v1/wallets/{investorId} | Get wallet portfolio |



## getWallet

> WalletResponse getWallet(investorId)

Get wallet portfolio

### Example

```ts
import {
  Configuration,
  WalletsApi,
} from '';
import type { GetWalletRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new WalletsApi(config);

  const body = {
    // string
    investorId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies GetWalletRequest;

  try {
    const data = await api.getWallet(body);
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
| **investorId** | `string` |  | [Defaults to `undefined`] |

### Return type

[**WalletResponse**](WalletResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Wallet portfolio |  -  |
| **404** | Resource not found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

