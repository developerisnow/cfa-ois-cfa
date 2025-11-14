# ReportsApi

All URIs are relative to *http://localhost:5000*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**getIssuerIssuancesReport**](ReportsApi.md#getissuerissuancesreport) | **GET** /v1/reports/issuances | Get issuer report for issuances |
| [**getIssuerPayoutsReport**](ReportsApi.md#getissuerpayoutsreport) | **GET** /v1/reports/issuer/payouts | Get issuer payouts report |
| [**getPayoutsReport**](ReportsApi.md#getpayoutsreport) | **GET** /v1/reports/payouts | Get payouts report |



## getIssuerIssuancesReport

> IssuerIssuancesReportResponse getIssuerIssuancesReport(issuerId, from, to)

Get issuer report for issuances

### Example

```ts
import {
  Configuration,
  ReportsApi,
} from '';
import type { GetIssuerIssuancesReportRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ReportsApi(config);

  const body = {
    // string | Issuer ID
    issuerId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // Date | Start date (YYYY-MM-DD) (optional)
    from: 2013-10-20,
    // Date | End date (YYYY-MM-DD) (optional)
    to: 2013-10-20,
  } satisfies GetIssuerIssuancesReportRequest;

  try {
    const data = await api.getIssuerIssuancesReport(body);
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
| **issuerId** | `string` | Issuer ID | [Defaults to `undefined`] |
| **from** | `Date` | Start date (YYYY-MM-DD) | [Optional] [Defaults to `undefined`] |
| **to** | `Date` | End date (YYYY-MM-DD) | [Optional] [Defaults to `undefined`] |

### Return type

[**IssuerIssuancesReportResponse**](IssuerIssuancesReportResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Issuer issuances report |  -  |
| **400** | Bad request |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## getIssuerPayoutsReport

> IssuerPayoutsReportResponse getIssuerPayoutsReport(issuerId, from, to, granularity)

Get issuer payouts report

### Example

```ts
import {
  Configuration,
  ReportsApi,
} from '';
import type { GetIssuerPayoutsReportRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ReportsApi(config);

  const body = {
    // string | Issuer ID
    issuerId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // Date | Start date (YYYY-MM-DD) (optional)
    from: 2013-10-20,
    // Date | End date (YYYY-MM-DD) (optional)
    to: 2013-10-20,
    // 'day' | 'week' | 'month' | 'year' | Report granularity (optional)
    granularity: granularity_example,
  } satisfies GetIssuerPayoutsReportRequest;

  try {
    const data = await api.getIssuerPayoutsReport(body);
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
| **issuerId** | `string` | Issuer ID | [Defaults to `undefined`] |
| **from** | `Date` | Start date (YYYY-MM-DD) | [Optional] [Defaults to `undefined`] |
| **to** | `Date` | End date (YYYY-MM-DD) | [Optional] [Defaults to `undefined`] |
| **granularity** | `day`, `week`, `month`, `year` | Report granularity | [Optional] [Defaults to `&#39;month&#39;`] [Enum: day, week, month, year] |

### Return type

[**IssuerPayoutsReportResponse**](IssuerPayoutsReportResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Issuer payouts report |  -  |
| **400** | Bad request |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## getPayoutsReport

> PayoutsReportResponse getPayoutsReport(from, to)

Get payouts report

### Example

```ts
import {
  Configuration,
  ReportsApi,
} from '';
import type { GetPayoutsReportRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: BearerAuth
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ReportsApi(config);

  const body = {
    // Date | Start date (YYYY-MM-DD) (optional)
    from: 2013-10-20,
    // Date | End date (YYYY-MM-DD) (optional)
    to: 2013-10-20,
  } satisfies GetPayoutsReportRequest;

  try {
    const data = await api.getPayoutsReport(body);
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
| **from** | `Date` | Start date (YYYY-MM-DD) | [Optional] [Defaults to `undefined`] |
| **to** | `Date` | End date (YYYY-MM-DD) | [Optional] [Defaults to `undefined`] |

### Return type

[**PayoutsReportResponse**](PayoutsReportResponse.md)

### Authorization

[BearerAuth](../README.md#BearerAuth)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Payouts report |  -  |
| **400** | Bad request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

