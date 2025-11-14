# Org.OpenAPITools.Api.WalletsApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**GetWallet**](WalletsApi.md#getwallet) | **GET** /v1/wallets/{investorId} | Get wallet portfolio |

<a id="getwallet"></a>
# **GetWallet**
> WalletResponse GetWallet (Guid investorId)

Get wallet portfolio


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **investorId** | **Guid** |  |  |

### Return type

[**WalletResponse**](WalletResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Wallet portfolio |  -  |
| **404** | Resource not found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

