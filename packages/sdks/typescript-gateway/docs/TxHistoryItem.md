
# TxHistoryItem


## Properties

Name | Type
------------ | -------------
`id` | string
`type` | string
`issuanceId` | string
`issuanceCode` | string
`amount` | number
`status` | string
`dltTxHash` | string
`createdAt` | Date
`confirmedAt` | Date

## Example

```typescript
import type { TxHistoryItem } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "type": null,
  "issuanceId": null,
  "issuanceCode": null,
  "amount": null,
  "status": null,
  "dltTxHash": null,
  "createdAt": null,
  "confirmedAt": null,
} satisfies TxHistoryItem

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as TxHistoryItem
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


