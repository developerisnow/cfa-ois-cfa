# Org.OpenAPITools.Api.ComplianceApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**CheckKyc**](ComplianceApi.md#checkkyc) | **POST** /v1/compliance/kyc/check | Check KYC status |
| [**EvaluateQualification**](ComplianceApi.md#evaluatequalification) | **POST** /v1/compliance/qualification/evaluate | Evaluate qualification |
| [**GetInvestorStatus**](ComplianceApi.md#getinvestorstatus) | **GET** /v1/compliance/investors/{id}/status | Get investor compliance status |
| [**MakeKycDecision**](ComplianceApi.md#makekycdecision) | **POST** /v1/kyc/{investorId}/decision | Make KYC decision |
| [**UploadKycDocuments**](ComplianceApi.md#uploadkycdocuments) | **POST** /v1/kyc/{investorId}/documents | Upload KYC documents |

<a id="checkkyc"></a>
# **CheckKyc**
> KycResult CheckKyc (KycCheckRequest kycCheckRequest)

Check KYC status


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **kycCheckRequest** | [**KycCheckRequest**](KycCheckRequest.md) |  |  |

### Return type

[**KycResult**](KycResult.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | KYC check result |  -  |
| **400** | Bad request |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="evaluatequalification"></a>
# **EvaluateQualification**
> QualificationResult EvaluateQualification (QualificationEvaluateRequest qualificationEvaluateRequest)

Evaluate qualification


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **qualificationEvaluateRequest** | [**QualificationEvaluateRequest**](QualificationEvaluateRequest.md) |  |  |

### Return type

[**QualificationResult**](QualificationResult.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Qualification result |  -  |
| **400** | Bad request |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getinvestorstatus"></a>
# **GetInvestorStatus**
> InvestorStatusResponse GetInvestorStatus (Guid id)

Get investor compliance status


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **Guid** |  |  |

### Return type

[**InvestorStatusResponse**](InvestorStatusResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Investor status |  -  |
| **404** | Resource not found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="makekycdecision"></a>
# **MakeKycDecision**
> KycDecisionResponse MakeKycDecision (Guid investorId, KycDecisionRequest kycDecisionRequest)

Make KYC decision


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **investorId** | **Guid** | Investor ID |  |
| **kycDecisionRequest** | [**KycDecisionRequest**](KycDecisionRequest.md) |  |  |

### Return type

[**KycDecisionResponse**](KycDecisionResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | KYC decision made |  -  |
| **400** | Bad request |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="uploadkycdocuments"></a>
# **UploadKycDocuments**
> KycDocumentsResponse UploadKycDocuments (Guid investorId, List<System.IO.Stream> files, string documentType = null, string comment = null)

Upload KYC documents


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **investorId** | **Guid** | Investor ID |  |
| **files** | **List&lt;System.IO.Stream&gt;** |  |  |
| **documentType** | **string** |  | [optional]  |
| **comment** | **string** | Optional comment | [optional]  |

### Return type

[**KycDocumentsResponse**](KycDocumentsResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

 - **Content-Type**: multipart/form-data
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Documents uploaded |  -  |
| **400** | Bad request |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

