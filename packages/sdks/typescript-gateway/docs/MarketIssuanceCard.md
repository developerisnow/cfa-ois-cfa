
# MarketIssuanceCard


## Properties

Name | Type
------------ | -------------
`id` | string
`assetCode` | string
`assetName` | string
`issuerName` | string
`totalAmount` | number
`nominal` | number
`availableAmount` | number
`issueDate` | Date
`maturityDate` | Date
`_yield` | number
`status` | string
`publishedAt` | Date
`scheduleJson` | object

## Example

```typescript
import type { MarketIssuanceCard } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "assetCode": null,
  "assetName": null,
  "issuerName": null,
  "totalAmount": null,
  "nominal": null,
  "availableAmount": null,
  "issueDate": null,
  "maturityDate": null,
  "_yield": null,
  "status": null,
  "publishedAt": null,
  "scheduleJson": null,
} satisfies MarketIssuanceCard

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as MarketIssuanceCard
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


