
# CreateIssuanceRequest


## Properties

Name | Type
------------ | -------------
`assetId` | string
`issuerId` | string
`totalAmount` | number
`nominal` | number
`issueDate` | Date
`maturityDate` | Date
`scheduleJson` | object

## Example

```typescript
import type { CreateIssuanceRequest } from ''

// TODO: Update the object below with actual values
const example = {
  "assetId": null,
  "issuerId": null,
  "totalAmount": null,
  "nominal": null,
  "issueDate": null,
  "maturityDate": null,
  "scheduleJson": null,
} satisfies CreateIssuanceRequest

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as CreateIssuanceRequest
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


