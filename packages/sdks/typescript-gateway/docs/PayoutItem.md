
# PayoutItem


## Properties

Name | Type
------------ | -------------
`id` | string
`batchId` | string
`issuanceId` | string
`investorId` | string
`amount` | number
`status` | string
`executedAt` | Date

## Example

```typescript
import type { PayoutItem } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "batchId": null,
  "issuanceId": null,
  "investorId": null,
  "amount": null,
  "status": null,
  "executedAt": null,
} satisfies PayoutItem

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as PayoutItem
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


