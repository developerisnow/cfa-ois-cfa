# MarketApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**getMarketIssuance**](MarketApi.md#getmarketissuance) | **GET** /v1/market/issuances/{id} | Get market issuance details |
| [**listMarketIssuances**](MarketApi.md#listmarketissuances) | **GET** /v1/market/issuances | List market issuances |



## getMarketIssuance

> MarketIssuanceCard getMarketIssuance(id)

Get market issuance details

### Example

```ts
import {
  Configuration,
  MarketApi,
} from '';
import type { GetMarketIssuanceRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new MarketApi(config);

  const body = {
    // string | Issuance ID
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies GetMarketIssuanceRequest;

  try {
    const data = await api.getMarketIssuance(body);
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
| **id** | `string` | Issuance ID | [Defaults to `undefined`] |

### Return type

[**MarketIssuanceCard**](MarketIssuanceCard.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Market issuance details |  -  |
| **404** | Resource not found |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## listMarketIssuances

> MarketIssuancesResponse listMarketIssuances(status, sort, limit, offset)

List market issuances

### Example

```ts
import {
  Configuration,
  MarketApi,
} from '';
import type { ListMarketIssuancesRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new MarketApi(config);

  const body = {
    // 'open' | 'closed' | 'all' | Filter by status (optional)
    status: status_example,
    // '-yield' | 'yield' | '-maturityDate' | 'maturityDate' | '-totalAmount' | 'totalAmount' | Sort order (prefix - for descending) (optional)
    sort: sort_example,
    // number | Page size (optional)
    limit: 56,
    // number | Page offset (optional)
    offset: 56,
  } satisfies ListMarketIssuancesRequest;

  try {
    const data = await api.listMarketIssuances(body);
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
| **status** | `open`, `closed`, `all` | Filter by status | [Optional] [Defaults to `&#39;open&#39;`] [Enum: open, closed, all] |
| **sort** | `-yield`, `yield`, `-maturityDate`, `maturityDate`, `-totalAmount`, `totalAmount` | Sort order (prefix - for descending) | [Optional] [Defaults to `&#39;-yield&#39;`] [Enum: -yield, yield, -maturityDate, maturityDate, -totalAmount, totalAmount] |
| **limit** | `number` | Page size | [Optional] [Defaults to `20`] |
| **offset** | `number` | Page offset | [Optional] [Defaults to `0`] |

### Return type

[**MarketIssuancesResponse**](MarketIssuancesResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Market issuances list |  -  |
| **400** | Bad request |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

