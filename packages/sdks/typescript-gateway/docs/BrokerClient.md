
# BrokerClient


## Properties

Name | Type
------------ | -------------
`id` | string
`name` | string
`email` | string
`inn` | string
`type` | string
`kycStatus` | string
`qualificationStatus` | string
`createdAt` | Date
`lastActivityAt` | Date

## Example

```typescript
import type { BrokerClient } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "name": null,
  "email": null,
  "inn": null,
  "type": null,
  "kycStatus": null,
  "qualificationStatus": null,
  "createdAt": null,
  "lastActivityAt": null,
} satisfies BrokerClient

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as BrokerClient
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


