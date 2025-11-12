
# IssuerReportRow


## Properties

Name | Type
------------ | -------------
`issuanceId` | string
`assetCode` | string
`assetName` | string
`totalAmount` | number
`soldAmount` | number
`investorsCount` | number
`status` | string
`issueDate` | Date
`maturityDate` | Date
`publishedAt` | Date

## Example

```typescript
import type { IssuerReportRow } from ''

// TODO: Update the object below with actual values
const example = {
  "issuanceId": null,
  "assetCode": null,
  "assetName": null,
  "totalAmount": null,
  "soldAmount": null,
  "investorsCount": null,
  "status": null,
  "issueDate": null,
  "maturityDate": null,
  "publishedAt": null,
} satisfies IssuerReportRow

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as IssuerReportRow
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


