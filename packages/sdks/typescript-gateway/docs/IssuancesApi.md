# IssuancesApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**closeIssuance**](IssuancesApi.md#closeissuance) | **POST** /issuances/{id}/close | Close issuance |
| [**createIssuance**](IssuancesApi.md#createissuanceoperation) | **POST** /issuances | Create draft issuance |
| [**getIssuance**](IssuancesApi.md#getissuance) | **GET** /issuances/{id} | Get issuance by ID |
| [**publishIssuance**](IssuancesApi.md#publishissuance) | **POST** /issuances/{id}/publish | Publish issuance |
| [**redeemIssuance**](IssuancesApi.md#redeemissuance) | **POST** /v1/issuances/{id}/redeem | Redeem issuance |



## closeIssuance

> IssuanceResponse closeIssuance(id)

Close issuance

### Example

```ts
import {
  Configuration,
  IssuancesApi,
} from '';
import type { CloseIssuanceRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new IssuancesApi(config);

  const body = {
    // string | Issuance ID
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies CloseIssuanceRequest;

  try {
    const data = await api.closeIssuance(body);
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

[**IssuanceResponse**](IssuanceResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Issuance closed |  -  |
| **400** | Bad request |  -  |
| **404** | Resource not found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## createIssuance

> IssuanceResponse createIssuance(createIssuanceRequest)

Create draft issuance

### Example

```ts
import {
  Configuration,
  IssuancesApi,
} from '';
import type { CreateIssuanceOperationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new IssuancesApi(config);

  const body = {
    // CreateIssuanceRequest
    createIssuanceRequest: ...,
  } satisfies CreateIssuanceOperationRequest;

  try {
    const data = await api.createIssuance(body);
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
| **createIssuanceRequest** | [CreateIssuanceRequest](CreateIssuanceRequest.md) |  | |

### Return type

[**IssuanceResponse**](IssuanceResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Issuance created |  -  |
| **400** | Bad request |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## getIssuance

> IssuanceResponse getIssuance(id)

Get issuance by ID

### Example

```ts
import {
  Configuration,
  IssuancesApi,
} from '';
import type { GetIssuanceRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new IssuancesApi(config);

  const body = {
    // string | Issuance ID
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies GetIssuanceRequest;

  try {
    const data = await api.getIssuance(body);
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

[**IssuanceResponse**](IssuanceResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Issuance details |  -  |
| **404** | Resource not found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## publishIssuance

> IssuanceResponse publishIssuance(id)

Publish issuance

### Example

```ts
import {
  Configuration,
  IssuancesApi,
} from '';
import type { PublishIssuanceRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new IssuancesApi(config);

  const body = {
    // string | Issuance ID
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies PublishIssuanceRequest;

  try {
    const data = await api.publishIssuance(body);
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

[**IssuanceResponse**](IssuanceResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Issuance published |  -  |
| **400** | Bad request |  -  |
| **404** | Resource not found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## redeemIssuance

> RedeemResponse redeemIssuance(id, redeemRequest)

Redeem issuance

### Example

```ts
import {
  Configuration,
  IssuancesApi,
} from '';
import type { RedeemIssuanceRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new IssuancesApi(config);

  const body = {
    // string | Issuance ID
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // RedeemRequest
    redeemRequest: ...,
  } satisfies RedeemIssuanceRequest;

  try {
    const data = await api.redeemIssuance(body);
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
| **redeemRequest** | [RedeemRequest](RedeemRequest.md) |  | |

### Return type

[**RedeemResponse**](RedeemResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Issuance redeemed |  -  |
| **400** | Bad request |  -  |
| **404** | Resource not found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

