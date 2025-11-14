
# QualificationResult


## Properties

Name | Type
------------ | -------------
`investorId` | string
`tier` | string
`limit` | number
`used` | number
`allowed` | boolean
`reason` | string
`evaluatedAt` | Date

## Example

```typescript
import type { QualificationResult } from ''

// TODO: Update the object below with actual values
const example = {
  "investorId": null,
  "tier": null,
  "limit": null,
  "used": null,
  "allowed": null,
  "reason": null,
  "evaluatedAt": null,
} satisfies QualificationResult

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as QualificationResult
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


