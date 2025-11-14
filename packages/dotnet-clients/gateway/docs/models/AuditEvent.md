# Org.OpenAPITools.Model.AuditEvent

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **Guid** |  | 
**Actor** | **Guid** | User/system that performed the action | 
**Action** | **string** | Action type (e.g., &#39;create&#39;, &#39;update&#39;, &#39;delete&#39;, &#39;approve&#39;, &#39;reject&#39;) | 
**Entity** | **string** | Entity type (e.g., &#39;issuance&#39;, &#39;order&#39;, &#39;kyc&#39;, &#39;investor&#39;) | 
**Timestamp** | **DateTime** |  | 
**ActorName** | **string** | Actor name or identifier | [optional] 
**EntityId** | **Guid** | Entity ID | [optional] 
**Payload** | **Dictionary&lt;string, Object&gt;** | Additional event data | [optional] 
**Ip** | **string** | IP address | [optional] 
**UserAgent** | **string** | User agent string | [optional] 
**Result** | **string** | Action result | [optional] 

[[Back to Model list]](../../README.md#documentation-for-models) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to README]](../../README.md)

