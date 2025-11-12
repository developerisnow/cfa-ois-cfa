# Org.OpenAPITools.Api.UsersApi

All URIs are relative to *http://localhost:5001*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**GetUser**](UsersApi.md#getuser) | **GET** /users/{id} | Get user by ID |
| [**ListUsers**](UsersApi.md#listusers) | **GET** /users | List users |

<a id="getuser"></a>
# **GetUser**
> User GetUser (Guid id)

Get user by ID


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** |  |  |

### Return type

[**User**](User.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | User details |  -  |
| **404** | User not found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="listusers"></a>
# **ListUsers**
> List&lt;User&gt; ListUsers (string email = null, string role = null, string status = null)

List users


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **email** | **string** |  | [optional]  |
| **role** | **string** |  | [optional]  |
| **status** | **string** |  | [optional]  |

### Return type

[**List&lt;User&gt;**](User.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | List of users |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

