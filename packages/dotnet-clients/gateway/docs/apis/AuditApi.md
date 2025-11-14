# Org.OpenAPITools.Api.AuditApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ExportAuditCsv**](AuditApi.md#exportauditcsv) | **GET** /v1/audit/export.csv | Export audit events as CSV |
| [**GetAuditEvent**](AuditApi.md#getauditevent) | **GET** /v1/audit/{id} | Get audit event by ID |
| [**GetAuditEvents**](AuditApi.md#getauditevents) | **GET** /v1/audit | Get audit events |

<a id="exportauditcsv"></a>
# **ExportAuditCsv**
> System.IO.Stream ExportAuditCsv (Guid actor = null, string action = null, string entity = null, DateTime from = null, DateTime to = null)

Export audit events as CSV


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **actor** | **Guid** |  | [optional]  |
| **action** | **string** |  | [optional]  |
| **entity** | **string** |  | [optional]  |
| **from** | **DateTime** |  | [optional]  |
| **to** | **DateTime** |  | [optional]  |

### Return type

**System.IO.Stream**

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/csv, application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | CSV export |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getauditevent"></a>
# **GetAuditEvent**
> AuditEvent GetAuditEvent (Guid id)

Get audit event by ID


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** | Audit event ID |  |

### Return type

[**AuditEvent**](AuditEvent.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Audit event details |  -  |
| **404** | Resource not found |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getauditevents"></a>
# **GetAuditEvents**
> AuditEventsResponse GetAuditEvents (Guid actor = null, string action = null, string entity = null, DateTime from = null, DateTime to = null, int limit = null, int offset = null)

Get audit events


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **actor** | **Guid** | Filter by actor ID | [optional]  |
| **action** | **string** | Filter by action type | [optional]  |
| **entity** | **string** | Filter by entity type | [optional]  |
| **from** | **DateTime** | Start datetime | [optional]  |
| **to** | **DateTime** | End datetime | [optional]  |
| **limit** | **int** | Page size | [optional] [default to 20] |
| **offset** | **int** | Page offset | [optional] [default to 0] |

### Return type

[**AuditEventsResponse**](AuditEventsResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Audit events list |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

