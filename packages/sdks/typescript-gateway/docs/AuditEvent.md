
# AuditEvent


## Properties

Name | Type
------------ | -------------
`id` | string
`actor` | string
`actorName` | string
`action` | string
`entity` | string
`entityId` | string
`payload` | { [key: string]: any; }
`ip` | string
`userAgent` | string
`timestamp` | Date
`result` | string

## Example

```typescript
import type { AuditEvent } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "actor": null,
  "actorName": null,
  "action": null,
  "entity": null,
  "entityId": null,
  "payload": null,
  "ip": null,
  "userAgent": null,
  "timestamp": null,
  "result": null,
} satisfies AuditEvent

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as AuditEvent
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


