
# WalletResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`ownerType` | string
`ownerId` | string
`balance` | number
`blocked` | number
`holdings` | [Array&lt;Holding&gt;](Holding.md)

## Example

```typescript
import type { WalletResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "ownerType": null,
  "ownerId": null,
  "balance": null,
  "blocked": null,
  "holdings": null,
} satisfies WalletResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as WalletResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


