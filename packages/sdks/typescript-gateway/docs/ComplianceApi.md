# ComplianceApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**checkKyc**](ComplianceApi.md#checkkyc) | **POST** /v1/compliance/kyc/check | Check KYC status |
| [**evaluateQualification**](ComplianceApi.md#evaluatequalification) | **POST** /v1/compliance/qualification/evaluate | Evaluate qualification |
| [**getInvestorStatus**](ComplianceApi.md#getinvestorstatus) | **GET** /v1/compliance/investors/{id}/status | Get investor compliance status |
| [**makeKycDecision**](ComplianceApi.md#makekycdecision) | **POST** /v1/kyc/{investorId}/decision | Make KYC decision |
| [**uploadKycDocuments**](ComplianceApi.md#uploadkycdocuments) | **POST** /v1/kyc/{investorId}/documents | Upload KYC documents |



## checkKyc

> KycResult checkKyc(kycCheckRequest)

Check KYC status

### Example

```ts
import {
  Configuration,
  ComplianceApi,
} from '';
import type { CheckKycRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ComplianceApi(config);

  const body = {
    // KycCheckRequest
    kycCheckRequest: ...,
  } satisfies CheckKycRequest;

  try {
    const data = await api.checkKyc(body);
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters


| Name | Type | Description  | Notes |
|------------- | ------------- | ------------- | -------------|
| **kycCheckRequest** | [KycCheckRequest](KycCheckRequest.md) |  | |

### Return type

[**KycResult**](KycResult.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | KYC check result |  -  |
| **400** | Bad request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## evaluateQualification

> QualificationResult evaluateQualification(qualificationEvaluateRequest)

Evaluate qualification

### Example

```ts
import {
  Configuration,
  ComplianceApi,
} from '';
import type { EvaluateQualificationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ComplianceApi(config);

  const body = {
    // QualificationEvaluateRequest
    qualificationEvaluateRequest: ...,
  } satisfies EvaluateQualificationRequest;

  try {
    const data = await api.evaluateQualification(body);
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters


| Name | Type | Description  | Notes |
|------------- | ------------- | ------------- | -------------|
| **qualificationEvaluateRequest** | [QualificationEvaluateRequest](QualificationEvaluateRequest.md) |  | |

### Return type

[**QualificationResult**](QualificationResult.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Qualification result |  -  |
| **400** | Bad request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## getInvestorStatus

> InvestorStatusResponse getInvestorStatus(id)

Get investor compliance status

### Example

```ts
import {
  Configuration,
  ComplianceApi,
} from '';
import type { GetInvestorStatusRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ComplianceApi(config);

  const body = {
    // string
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies GetInvestorStatusRequest;

  try {
    const data = await api.getInvestorStatus(body);
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters


| Name | Type | Description  | Notes |
|------------- | ------------- | ------------- | -------------|
| **id** | `string` |  | [Defaults to `undefined`] |

### Return type

[**InvestorStatusResponse**](InvestorStatusResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Investor status |  -  |
| **404** | Resource not found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## makeKycDecision

> KycDecisionResponse makeKycDecision(investorId, kycDecisionRequest)

Make KYC decision

### Example

```ts
import {
  Configuration,
  ComplianceApi,
} from '';
import type { MakeKycDecisionRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ComplianceApi(config);

  const body = {
    // string | Investor ID
    investorId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // KycDecisionRequest
    kycDecisionRequest: ...,
  } satisfies MakeKycDecisionRequest;

  try {
    const data = await api.makeKycDecision(body);
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters


| Name | Type | Description  | Notes |
|------------- | ------------- | ------------- | -------------|
| **investorId** | `string` | Investor ID | [Defaults to `undefined`] |
| **kycDecisionRequest** | [KycDecisionRequest](KycDecisionRequest.md) |  | |

### Return type

[**KycDecisionResponse**](KycDecisionResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | KYC decision made |  -  |
| **400** | Bad request |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## uploadKycDocuments

> KycDocumentsResponse uploadKycDocuments(investorId, files, documentType, comment)

Upload KYC documents

### Example

```ts
import {
  Configuration,
  ComplianceApi,
} from '';
import type { UploadKycDocumentsRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ComplianceApi(config);

  const body = {
    // string | Investor ID
    investorId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // Array<Blob>
    files: /path/to/file.txt,
    // string (optional)
    documentType: documentType_example,
    // string | Optional comment (optional)
    comment: comment_example,
  } satisfies UploadKycDocumentsRequest;

  try {
    const data = await api.uploadKycDocuments(body);
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters


| Name | Type | Description  | Notes |
|------------- | ------------- | ------------- | -------------|
| **investorId** | `string` | Investor ID | [Defaults to `undefined`] |
| **files** | `Array<Blob>` |  | |
| **documentType** | `passport`, `inn`, `snils`, `address_proof`, `income_proof`, `other` |  | [Optional] [Defaults to `undefined`] [Enum: passport, inn, snils, address_proof, income_proof, other] |
| **comment** | `string` | Optional comment | [Optional] [Defaults to `undefined`] |

### Return type

[**KycDocumentsResponse**](KycDocumentsResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: `multipart/form-data`
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Documents uploaded |  -  |
| **400** | Bad request |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

