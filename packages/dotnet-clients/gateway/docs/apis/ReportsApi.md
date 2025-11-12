# Org.OpenAPITools.Api.ReportsApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**GetIssuerIssuancesReport**](ReportsApi.md#getissuerissuancesreport) | **GET** /v1/reports/issuances | Get issuer report for issuances |
| [**GetIssuerPayoutsReport**](ReportsApi.md#getissuerpayoutsreport) | **GET** /v1/reports/issuer/payouts | Get issuer payouts report |
| [**GetPayoutsReport**](ReportsApi.md#getpayoutsreport) | **GET** /v1/reports/payouts | Get payouts report |

<a id="getissuerissuancesreport"></a>
# **GetIssuerIssuancesReport**
> IssuerIssuancesReportResponse GetIssuerIssuancesReport (Guid issuerId, DateOnly from = null, DateOnly to = null)

Get issuer report for issuances


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **issuerId** | **Guid** | Issuer ID |  |
| **from** | **DateOnly** | Start date (YYYY-MM-DD) | [optional]  |
| **to** | **DateOnly** | End date (YYYY-MM-DD) | [optional]  |

### Return type

[**IssuerIssuancesReportResponse**](IssuerIssuancesReportResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Issuer issuances report |  -  |
| **400** | Bad request |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getissuerpayoutsreport"></a>
# **GetIssuerPayoutsReport**
> IssuerPayoutsReportResponse GetIssuerPayoutsReport (Guid issuerId, DateOnly from = null, DateOnly to = null, string granularity = null)

Get issuer payouts report


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **issuerId** | **Guid** | Issuer ID |  |
| **from** | **DateOnly** | Start date (YYYY-MM-DD) | [optional]  |
| **to** | **DateOnly** | End date (YYYY-MM-DD) | [optional]  |
| **granularity** | **string** | Report granularity | [optional] [default to month] |

### Return type

[**IssuerPayoutsReportResponse**](IssuerPayoutsReportResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Issuer payouts report |  -  |
| **400** | Bad request |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getpayoutsreport"></a>
# **GetPayoutsReport**
> PayoutsReportResponse GetPayoutsReport (DateOnly from = null, DateOnly to = null)

Get payouts report


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **from** | **DateOnly** | Start date (YYYY-MM-DD) | [optional]  |
| **to** | **DateOnly** | End date (YYYY-MM-DD) | [optional]  |

### Return type

[**PayoutsReportResponse**](PayoutsReportResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Payouts report |  -  |
| **400** | Bad request |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

