
# IssuerIssuancesReportResponse


## Properties

Name | Type
------------ | -------------
`issuerId` | string
`period` | [PayoutsReportResponsePeriod](PayoutsReportResponsePeriod.md)
`items` | [Array&lt;IssuerReportRow&gt;](IssuerReportRow.md)
`summary` | [IssuerIssuancesReportResponseSummary](IssuerIssuancesReportResponseSummary.md)

## Example

```typescript
import type { IssuerIssuancesReportResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "issuerId": null,
  "period": null,
  "items": null,
  "summary": null,
} satisfies IssuerIssuancesReportResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as IssuerIssuancesReportResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


