# Org.OpenAPITools.Api.IssuancesApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**CloseIssuance**](IssuancesApi.md#closeissuance) | **POST** /issuances/{id}/close | Close issuance |
| [**CreateIssuance**](IssuancesApi.md#createissuance) | **POST** /issuances | Create draft issuance |
| [**GetIssuance**](IssuancesApi.md#getissuance) | **GET** /issuances/{id} | Get issuance by ID |
| [**PublishIssuance**](IssuancesApi.md#publishissuance) | **POST** /issuances/{id}/publish | Publish issuance |
| [**RedeemIssuance**](IssuancesApi.md#redeemissuance) | **POST** /v1/issuances/{id}/redeem | Redeem issuance |

<a id="closeissuance"></a>
# **CloseIssuance**
> IssuanceResponse CloseIssuance (Guid id)

Close issuance


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** | Issuance ID |  |

### Return type

[**IssuanceResponse**](IssuanceResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Issuance closed |  -  |
| **400** | Bad request |  -  |
| **404** | Resource not found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="createissuance"></a>
# **CreateIssuance**
> IssuanceResponse CreateIssuance (CreateIssuanceRequest createIssuanceRequest)

Create draft issuance


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **createIssuanceRequest** | [**CreateIssuanceRequest**](CreateIssuanceRequest.md) |  |  |

### Return type

[**IssuanceResponse**](IssuanceResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Issuance created |  -  |
| **400** | Bad request |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getissuance"></a>
# **GetIssuance**
> IssuanceResponse GetIssuance (Guid id)

Get issuance by ID


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** | Issuance ID |  |

### Return type

[**IssuanceResponse**](IssuanceResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Issuance details |  -  |
| **404** | Resource not found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="publishissuance"></a>
# **PublishIssuance**
> IssuanceResponse PublishIssuance (Guid id)

Publish issuance


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** | Issuance ID |  |

### Return type

[**IssuanceResponse**](IssuanceResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Issuance published |  -  |
| **400** | Bad request |  -  |
| **404** | Resource not found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="redeemissuance"></a>
# **RedeemIssuance**
> RedeemResponse RedeemIssuance (Guid id, RedeemRequest redeemRequest)

Redeem issuance


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** | Issuance ID |  |
| **redeemRequest** | [**RedeemRequest**](RedeemRequest.md) |  |  |

### Return type

[**RedeemResponse**](RedeemResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Issuance redeemed |  -  |
| **400** | Bad request |  -  |
| **404** | Resource not found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

