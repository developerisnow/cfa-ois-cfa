
# SettlementResponse


## Properties

Name | Type
------------ | -------------
`batchId` | string
`runDate` | Date
`issuanceId` | string
`totalAmount` | number
`status` | string
`itemCount` | number
`createdAt` | Date

## Example

```typescript
import type { SettlementResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "batchId": null,
  "runDate": null,
  "issuanceId": null,
  "totalAmount": null,
  "status": null,
  "itemCount": null,
  "createdAt": null,
} satisfies SettlementResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as SettlementResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


