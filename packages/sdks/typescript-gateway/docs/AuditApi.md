# AuditApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**exportAuditCsv**](AuditApi.md#exportauditcsv) | **GET** /v1/audit/export.csv | Export audit events as CSV |
| [**getAuditEvent**](AuditApi.md#getauditevent) | **GET** /v1/audit/{id} | Get audit event by ID |
| [**getAuditEvents**](AuditApi.md#getauditevents) | **GET** /v1/audit | Get audit events |



## exportAuditCsv

> Blob exportAuditCsv(actor, action, entity, from, to)

Export audit events as CSV

### Example

```ts
import {
  Configuration,
  AuditApi,
} from '';
import type { ExportAuditCsvRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new AuditApi(config);

  const body = {
    // string (optional)
    actor: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string (optional)
    action: action_example,
    // string (optional)
    entity: entity_example,
    // Date (optional)
    from: 2013-10-20T19:20:30+01:00,
    // Date (optional)
    to: 2013-10-20T19:20:30+01:00,
  } satisfies ExportAuditCsvRequest;

  try {
    const data = await api.exportAuditCsv(body);
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
| **actor** | `string` |  | [Optional] [Defaults to `undefined`] |
| **action** | `string` |  | [Optional] [Defaults to `undefined`] |
| **entity** | `string` |  | [Optional] [Defaults to `undefined`] |
| **from** | `Date` |  | [Optional] [Defaults to `undefined`] |
| **to** | `Date` |  | [Optional] [Defaults to `undefined`] |

### Return type

**Blob**

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `text/csv`, `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | CSV export |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## getAuditEvent

> AuditEvent getAuditEvent(id)

Get audit event by ID

### Example

```ts
import {
  Configuration,
  AuditApi,
} from '';
import type { GetAuditEventRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new AuditApi(config);

  const body = {
    // string | Audit event ID
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies GetAuditEventRequest;

  try {
    const data = await api.getAuditEvent(body);
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
| **id** | `string` | Audit event ID | [Defaults to `undefined`] |

### Return type

[**AuditEvent**](AuditEvent.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Audit event details |  -  |
| **404** | Resource not found |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## getAuditEvents

> AuditEventsResponse getAuditEvents(actor, action, entity, from, to, limit, offset)

Get audit events

### Example

```ts
import {
  Configuration,
  AuditApi,
} from '';
import type { GetAuditEventsRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new AuditApi(config);

  const body = {
    // string | Filter by actor ID (optional)
    actor: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string | Filter by action type (optional)
    action: action_example,
    // string | Filter by entity type (optional)
    entity: entity_example,
    // Date | Start datetime (optional)
    from: 2013-10-20T19:20:30+01:00,
    // Date | End datetime (optional)
    to: 2013-10-20T19:20:30+01:00,
    // number | Page size (optional)
    limit: 56,
    // number | Page offset (optional)
    offset: 56,
  } satisfies GetAuditEventsRequest;

  try {
    const data = await api.getAuditEvents(body);
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
| **actor** | `string` | Filter by actor ID | [Optional] [Defaults to `undefined`] |
| **action** | `string` | Filter by action type | [Optional] [Defaults to `undefined`] |
| **entity** | `string` | Filter by entity type | [Optional] [Defaults to `undefined`] |
| **from** | `Date` | Start datetime | [Optional] [Defaults to `undefined`] |
| **to** | `Date` | End datetime | [Optional] [Defaults to `undefined`] |
| **limit** | `number` | Page size | [Optional] [Defaults to `20`] |
| **offset** | `number` | Page offset | [Optional] [Defaults to `0`] |

### Return type

[**AuditEventsResponse**](AuditEventsResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Audit events list |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

