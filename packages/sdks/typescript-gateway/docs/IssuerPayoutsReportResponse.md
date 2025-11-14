
# IssuerPayoutsReportResponse


## Properties

Name | Type
------------ | -------------
`issuerId` | string
`period` | [PayoutsReportResponsePeriod](PayoutsReportResponsePeriod.md)
`granularity` | string
`items` | [Array&lt;IssuerPayoutsReportResponseItemsInner&gt;](IssuerPayoutsReportResponseItemsInner.md)
`summary` | [IssuerPayoutsReportResponseSummary](IssuerPayoutsReportResponseSummary.md)

## Example

```typescript
import type { IssuerPayoutsReportResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "issuerId": null,
  "period": null,
  "granularity": null,
  "items": null,
  "summary": null,
} satisfies IssuerPayoutsReportResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as IssuerPayoutsReportResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


