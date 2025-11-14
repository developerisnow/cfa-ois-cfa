
# KycResult


## Properties

Name | Type
------------ | -------------
`investorId` | string
`status` | string
`checkedAt` | Date
`reason` | string

## Example

```typescript
import type { KycResult } from ''

// TODO: Update the object below with actual values
const example = {
  "investorId": null,
  "status": null,
  "checkedAt": null,
  "reason": null,
} satisfies KycResult

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as KycResult
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


