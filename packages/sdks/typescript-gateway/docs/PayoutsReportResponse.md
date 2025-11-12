
# PayoutsReportResponse


## Properties

Name | Type
------------ | -------------
`period` | [PayoutsReportResponsePeriod](PayoutsReportResponsePeriod.md)
`items` | [Array&lt;PayoutItem&gt;](PayoutItem.md)
`totalAmount` | number

## Example

```typescript
import type { PayoutsReportResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "period": null,
  "items": null,
  "totalAmount": null,
} satisfies PayoutsReportResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as PayoutsReportResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


