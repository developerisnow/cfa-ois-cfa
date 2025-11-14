
# IssuanceResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`assetId` | string
`issuerId` | string
`totalAmount` | number
`nominal` | number
`issueDate` | Date
`maturityDate` | Date
`status` | string
`scheduleJson` | object
`createdAt` | Date
`updatedAt` | Date

## Example

```typescript
import type { IssuanceResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "assetId": null,
  "issuerId": null,
  "totalAmount": null,
  "nominal": null,
  "issueDate": null,
  "maturityDate": null,
  "status": null,
  "scheduleJson": null,
  "createdAt": null,
  "updatedAt": null,
} satisfies IssuanceResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as IssuanceResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


