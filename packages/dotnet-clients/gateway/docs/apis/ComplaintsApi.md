# Org.OpenAPITools.Api.ComplaintsApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**CreateComplaint**](ComplaintsApi.md#createcomplaint) | **POST** /v1/complaints | Create complaint |
| [**GetComplaint**](ComplaintsApi.md#getcomplaint) | **GET** /v1/complaints/{id} | Get complaint |

<a id="createcomplaint"></a>
# **CreateComplaint**
> ComplaintResponse CreateComplaint (CreateComplaintRequest createComplaintRequest, Guid idempotencyKey = null)

Create complaint


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **createComplaintRequest** | [**CreateComplaintRequest**](CreateComplaintRequest.md) |  |  |
| **idempotencyKey** | **Guid** |  | [optional]  |

### Return type

[**ComplaintResponse**](ComplaintResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Complaint created |  -  |
| **400** | Bad request |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getcomplaint"></a>
# **GetComplaint**
> ComplaintResponse GetComplaint (Guid id)

Get complaint


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** |  |  |

### Return type

[**ComplaintResponse**](ComplaintResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Complaint details |  -  |
| **404** | Resource not found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

