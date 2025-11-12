
# FeedItem


## Properties

Name | Type
------------ | -------------
`id` | string
`type` | string
`title` | string
`description` | string
`clientId` | string
`clientName` | string
`issuanceId` | string
`amount` | number
`status` | string
`timestamp` | Date
`metadata` | { [key: string]: any; }

## Example

```typescript
import type { FeedItem } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "type": null,
  "title": null,
  "description": null,
  "clientId": null,
  "clientName": null,
  "issuanceId": null,
  "amount": null,
  "status": null,
  "timestamp": null,
  "metadata": null,
} satisfies FeedItem

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as FeedItem
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


