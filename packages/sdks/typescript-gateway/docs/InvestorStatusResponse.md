
# InvestorStatusResponse


## Properties

Name | Type
------------ | -------------
`investorId` | string
`kyc` | string
`qualificationTier` | string
`qualificationLimit` | number
`qualificationUsed` | number
`updatedAt` | Date

## Example

```typescript
import type { InvestorStatusResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "investorId": null,
  "kyc": null,
  "qualificationTier": null,
  "qualificationLimit": null,
  "qualificationUsed": null,
  "updatedAt": null,
} satisfies InvestorStatusResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as InvestorStatusResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


