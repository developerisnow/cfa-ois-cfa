# OrdersApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**getOrder**](OrdersApi.md#getorder) | **GET** /orders/{id} | Get order by ID |
| [**placeOrder**](OrdersApi.md#placeorder) | **POST** /v1/orders | Place buy order |



## getOrder

> OrderResponse getOrder(id)

Get order by ID

### Example

```ts
import {
  Configuration,
  OrdersApi,
} from '';
import type { GetOrderRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new OrdersApi(config);

  const body = {
    // string
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies GetOrderRequest;

  try {
    const data = await api.getOrder(body);
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
| **id** | `string` |  | [Defaults to `undefined`] |

### Return type

[**OrderResponse**](OrderResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Order details |  -  |
| **404** | Resource not found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## placeOrder

> OrderResponse placeOrder(idempotencyKey, createOrderRequest)

Place buy order

### Example

```ts
import {
  Configuration,
  OrdersApi,
} from '';
import type { PlaceOrderRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new OrdersApi(config);

  const body = {
    // string | Idempotency key to prevent duplicate orders
    idempotencyKey: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // CreateOrderRequest
    createOrderRequest: ...,
  } satisfies PlaceOrderRequest;

  try {
    const data = await api.placeOrder(body);
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
| **idempotencyKey** | `string` | Idempotency key to prevent duplicate orders | [Defaults to `undefined`] |
| **createOrderRequest** | [CreateOrderRequest](CreateOrderRequest.md) |  | |

### Return type

[**OrderResponse**](OrderResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **202** | Order accepted |  -  |
| **400** | Bad request |  -  |
| **401** | Unauthorized |  -  |
| **409** | Order with this idempotency key already exists |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

