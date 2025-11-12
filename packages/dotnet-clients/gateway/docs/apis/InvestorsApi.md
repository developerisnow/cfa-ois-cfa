# Org.OpenAPITools.Api.InvestorsApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**GetInvestorPayouts**](InvestorsApi.md#getinvestorpayouts) | **GET** /v1/investors/{id}/payouts | Get investor payout history |
| [**GetInvestorTransactions**](InvestorsApi.md#getinvestortransactions) | **GET** /v1/investors/{id}/transactions | Get investor transaction history |

<a id="getinvestorpayouts"></a>
# **GetInvestorPayouts**
> PayoutHistoryResponse GetInvestorPayouts (Guid id, DateOnly from = null, DateOnly to = null)

Get investor payout history


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** | Investor ID |  |
| **from** | **DateOnly** | Start date (YYYY-MM-DD) | [optional]  |
| **to** | **DateOnly** | End date (YYYY-MM-DD) | [optional]  |

### Return type

[**PayoutHistoryResponse**](PayoutHistoryResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Payout history |  -  |
| **404** | Resource not found |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getinvestortransactions"></a>
# **GetInvestorTransactions**
> TransactionHistoryResponse GetInvestorTransactions (Guid id, DateOnly from = null, DateOnly to = null, string type = null)

Get investor transaction history


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** | Investor ID |  |
| **from** | **DateOnly** | Start date (YYYY-MM-DD) | [optional]  |
| **to** | **DateOnly** | End date (YYYY-MM-DD) | [optional]  |
| **type** | **string** | Filter by transaction type | [optional] [default to all] |

### Return type

[**TransactionHistoryResponse**](TransactionHistoryResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Transaction history |  -  |
| **404** | Resource not found |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

