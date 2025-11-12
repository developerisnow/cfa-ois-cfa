# Org.OpenAPITools.Api.SettlementApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**RunSettlement**](SettlementApi.md#runsettlement) | **POST** /v1/settlement/run | Run settlement for a specific date |

<a id="runsettlement"></a>
# **RunSettlement**
> SettlementResponse RunSettlement (DateOnly date = null)

Run settlement for a specific date


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **date** | **DateOnly** | Date to run settlement for (YYYY-MM-DD). Defaults to today. | [optional]  |

### Return type

[**SettlementResponse**](SettlementResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **202** | Settlement accepted |  -  |
| **400** | Bad request |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

