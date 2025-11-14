
# KycDocument


## Properties

Name | Type
------------ | -------------
`id` | string
`investorId` | string
`documentType` | string
`fileName` | string
`fileSize` | number
`mimeType` | string
`storageUrl` | string
`uploadedAt` | Date
`uploadedBy` | string
`comment` | string

## Example

```typescript
import type { KycDocument } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "investorId": null,
  "documentType": null,
  "fileName": null,
  "fileSize": null,
  "mimeType": null,
  "storageUrl": null,
  "uploadedAt": null,
  "uploadedBy": null,
  "comment": null,
} satisfies KycDocument

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as KycDocument
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


