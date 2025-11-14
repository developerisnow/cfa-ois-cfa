
# BrokerOrderResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`clientId` | string
`issuanceId` | string
`amount` | number
`status` | string
`commission` | number
`createdAt` | Date

## Example

```typescript
import type { BrokerOrderResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "clientId": null,
  "issuanceId": null,
  "amount": null,
  "status": null,
  "commission": null,
  "createdAt": null,
} satisfies BrokerOrderResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as BrokerOrderResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


