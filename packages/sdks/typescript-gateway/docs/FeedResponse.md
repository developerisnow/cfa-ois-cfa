
# FeedResponse


## Properties

Name | Type
------------ | -------------
`items` | [Array&lt;FeedItem&gt;](FeedItem.md)
`total` | number
`hasMore` | boolean

## Example

```typescript
import type { FeedResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "items": null,
  "total": null,
  "hasMore": null,
} satisfies FeedResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as FeedResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


