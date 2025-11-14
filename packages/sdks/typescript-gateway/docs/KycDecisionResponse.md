
# KycDecisionResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`investorId` | string
`status` | string
`comment` | string
`decisionBy` | string
`decisionAt` | Date

## Example

```typescript
import type { KycDecisionResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "investorId": null,
  "status": null,
  "comment": null,
  "decisionBy": null,
  "decisionAt": null,
} satisfies KycDecisionResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as KycDecisionResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


