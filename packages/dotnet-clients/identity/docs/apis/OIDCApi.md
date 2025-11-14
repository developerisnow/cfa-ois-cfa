# Org.OpenAPITools.Api.OIDCApi

All URIs are relative to *http://localhost:5001*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**Authorize**](OIDCApi.md#authorize) | **GET** /authorize | OAuth2/OIDC authorization endpoint |
| [**GetOidcConfig**](OIDCApi.md#getoidcconfig) | **GET** /.well-known/openid-configuration | OpenID Connect configuration |
| [**GetUserInfo**](OIDCApi.md#getuserinfo) | **GET** /userinfo | Get user info |
| [**Token**](OIDCApi.md#token) | **POST** /token | OAuth2 token endpoint |

<a id="authorize"></a>
# **Authorize**
> void Authorize (string responseType, string clientId, string redirectUri, string scope = null, string state = null)

OAuth2/OIDC authorization endpoint


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **responseType** | **string** |  |  |
| **clientId** | **string** |  |  |
| **redirectUri** | **string** |  |  |
| **scope** | **string** |  | [optional] [default to &quot;openid profile email&quot;] |
| **state** | **string** |  | [optional]  |

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **302** | Redirect to ESIA or callback |  -  |
| **400** | Invalid request |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getoidcconfig"></a>
# **GetOidcConfig**
> OidcConfiguration GetOidcConfig ()

OpenID Connect configuration


### Parameters
This endpoint does not need any parameter.
### Return type

[**OidcConfiguration**](OidcConfiguration.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OIDC configuration |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getuserinfo"></a>
# **GetUserInfo**
> UserInfo GetUserInfo ()

Get user info


### Parameters
This endpoint does not need any parameter.
### Return type

[**UserInfo**](UserInfo.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | User information |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="token"></a>
# **Token**
> TokenResponse Token (string grantType, string code, string redirectUri, string clientId)

OAuth2 token endpoint


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **grantType** | **string** |  |  |
| **code** | **string** |  |  |
| **redirectUri** | **string** |  |  |
| **clientId** | **string** |  |  |

### Return type

[**TokenResponse**](TokenResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/x-www-form-urlencoded
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Token response |  -  |
| **400** | Invalid request |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

