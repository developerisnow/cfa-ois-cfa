# InvestorsApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**getInvestorPayouts**](InvestorsApi.md#getinvestorpayouts) | **GET** /v1/investors/{id}/payouts | Get investor payout history |
| [**getInvestorTransactions**](InvestorsApi.md#getinvestortransactions) | **GET** /v1/investors/{id}/transactions | Get investor transaction history |



## getInvestorPayouts

> PayoutHistoryResponse getInvestorPayouts(id, from, to)

Get investor payout history

### Example

```ts
import {
  Configuration,
  InvestorsApi,
} from '';
import type { GetInvestorPayoutsRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new InvestorsApi(config);

  const body = {
    // string | Investor ID
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // Date | Start date (YYYY-MM-DD) (optional)
    from: 2013-10-20,
    // Date | End date (YYYY-MM-DD) (optional)
    to: 2013-10-20,
  } satisfies GetInvestorPayoutsRequest;

  try {
    const data = await api.getInvestorPayouts(body);
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
| **id** | `string` | Investor ID | [Defaults to `undefined`] |
| **from** | `Date` | Start date (YYYY-MM-DD) | [Optional] [Defaults to `undefined`] |
| **to** | `Date` | End date (YYYY-MM-DD) | [Optional] [Defaults to `undefined`] |

### Return type

[**PayoutHistoryResponse**](PayoutHistoryResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Payout history |  -  |
| **404** | Resource not found |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## getInvestorTransactions

> TransactionHistoryResponse getInvestorTransactions(id, from, to, type)

Get investor transaction history

### Example

```ts
import {
  Configuration,
  InvestorsApi,
} from '';
import type { GetInvestorTransactionsRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new InvestorsApi(config);

  const body = {
    // string | Investor ID
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // Date | Start date (YYYY-MM-DD) (optional)
    from: 2013-10-20,
    // Date | End date (YYYY-MM-DD) (optional)
    to: 2013-10-20,
    // 'transfer' | 'redeem' | 'issue' | 'all' | Filter by transaction type (optional)
    type: type_example,
  } satisfies GetInvestorTransactionsRequest;

  try {
    const data = await api.getInvestorTransactions(body);
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
| **id** | `string` | Investor ID | [Defaults to `undefined`] |
| **from** | `Date` | Start date (YYYY-MM-DD) | [Optional] [Defaults to `undefined`] |
| **to** | `Date` | End date (YYYY-MM-DD) | [Optional] [Defaults to `undefined`] |
| **type** | `transfer`, `redeem`, `issue`, `all` | Filter by transaction type | [Optional] [Defaults to `&#39;all&#39;`] [Enum: transfer, redeem, issue, all] |

### Return type

[**TransactionHistoryResponse**](TransactionHistoryResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Transaction history |  -  |
| **404** | Resource not found |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

