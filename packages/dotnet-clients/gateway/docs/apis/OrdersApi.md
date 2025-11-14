# Org.OpenAPITools.Api.OrdersApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**GetOrder**](OrdersApi.md#getorder) | **GET** /orders/{id} | Get order by ID |
| [**PlaceOrder**](OrdersApi.md#placeorder) | **POST** /v1/orders | Place buy order |

<a id="getorder"></a>
# **GetOrder**
> OrderResponse GetOrder (Guid id)

Get order by ID


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** |  |  |

### Return type

[**OrderResponse**](OrderResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Order details |  -  |
| **404** | Resource not found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="placeorder"></a>
# **PlaceOrder**
> OrderResponse PlaceOrder (Guid idempotencyKey, CreateOrderRequest createOrderRequest)

Place buy order


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **idempotencyKey** | **Guid** | Idempotency key to prevent duplicate orders |  |
| **createOrderRequest** | [**CreateOrderRequest**](CreateOrderRequest.md) |  |  |

### Return type

[**OrderResponse**](OrderResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **202** | Order accepted |  -  |
| **400** | Bad request |  -  |
| **401** | Unauthorized |  -  |
| **409** | Order with this idempotency key already exists |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

