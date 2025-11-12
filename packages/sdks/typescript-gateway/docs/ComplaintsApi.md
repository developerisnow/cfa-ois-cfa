# ComplaintsApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**createComplaint**](ComplaintsApi.md#createcomplaintoperation) | **POST** /v1/complaints | Create complaint |
| [**getComplaint**](ComplaintsApi.md#getcomplaint) | **GET** /v1/complaints/{id} | Get complaint |



## createComplaint

> ComplaintResponse createComplaint(createComplaintRequest, idempotencyKey)

Create complaint

### Example

```ts
import {
  Configuration,
  ComplaintsApi,
} from '';
import type { CreateComplaintOperationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ComplaintsApi(config);

  const body = {
    // CreateComplaintRequest
    createComplaintRequest: ...,
    // string (optional)
    idempotencyKey: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies CreateComplaintOperationRequest;

  try {
    const data = await api.createComplaint(body);
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
| **createComplaintRequest** | [CreateComplaintRequest](CreateComplaintRequest.md) |  | |
| **idempotencyKey** | `string` |  | [Optional] [Defaults to `undefined`] |

### Return type

[**ComplaintResponse**](ComplaintResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Complaint created |  -  |
| **400** | Bad request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## getComplaint

> ComplaintResponse getComplaint(id)

Get complaint

### Example

```ts
import {
  Configuration,
  ComplaintsApi,
} from '';
import type { GetComplaintRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ComplaintsApi(config);

  const body = {
    // string
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies GetComplaintRequest;

  try {
    const data = await api.getComplaint(body);
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

[**ComplaintResponse**](ComplaintResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Complaint details |  -  |
| **404** | Resource not found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

