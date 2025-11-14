
# CommissionsResponse


## Properties

Name | Type
------------ | -------------
`items` | [Array&lt;CommissionRow&gt;](CommissionRow.md)
`total` | number
`totalAmount` | number
`totalCommission` | number

## Example

```typescript
import type { CommissionsResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "items": null,
  "total": null,
  "totalAmount": null,
  "totalCommission": null,
} satisfies CommissionsResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as CommissionsResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


