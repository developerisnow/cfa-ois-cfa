# Org.OpenAPITools.Api.MarketApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**GetMarketIssuance**](MarketApi.md#getmarketissuance) | **GET** /v1/market/issuances/{id} | Get market issuance details |
| [**ListMarketIssuances**](MarketApi.md#listmarketissuances) | **GET** /v1/market/issuances | List market issuances |

<a id="getmarketissuance"></a>
# **GetMarketIssuance**
> MarketIssuanceCard GetMarketIssuance (Guid id)

Get market issuance details


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** | Issuance ID |  |

### Return type

[**MarketIssuanceCard**](MarketIssuanceCard.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Market issuance details |  -  |
| **404** | Resource not found |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="listmarketissuances"></a>
# **ListMarketIssuances**
> MarketIssuancesResponse ListMarketIssuances (string status = null, string sort = null, int limit = null, int offset = null)

List market issuances


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **status** | **string** | Filter by status | [optional] [default to open] |
| **sort** | **string** | Sort order (prefix - for descending) | [optional] [default to -yield] |
| **limit** | **int** | Page size | [optional] [default to 20] |
| **offset** | **int** | Page offset | [optional] [default to 0] |

### Return type

[**MarketIssuancesResponse**](MarketIssuancesResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Market issuances list |  -  |
| **400** | Bad request |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

