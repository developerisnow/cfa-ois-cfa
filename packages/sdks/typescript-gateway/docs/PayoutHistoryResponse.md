
# PayoutHistoryResponse


## Properties

Name | Type
------------ | -------------
`items` | [Array&lt;PayoutItem&gt;](PayoutItem.md)
`total` | number
`totalAmount` | number

## Example

```typescript
import type { PayoutHistoryResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "items": null,
  "total": null,
  "totalAmount": null,
} satisfies PayoutHistoryResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as PayoutHistoryResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


