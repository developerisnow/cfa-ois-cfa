
# ComplaintResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`investorId` | string
`category` | string
`text` | string
`status` | string
`slaDue` | Date
`createdAt` | Date
`resolvedAt` | Date

## Example

```typescript
import type { ComplaintResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "investorId": null,
  "category": null,
  "text": null,
  "status": null,
  "slaDue": null,
  "createdAt": null,
  "resolvedAt": null,
} satisfies ComplaintResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as ComplaintResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


