---
title: Add Retrieve Multiple Tags endpoint
version: 1.0
date_created: 2026-04-07
last_updated: 2026-04-08
---
# Implementation plan: Add Retrieve Multiple Tags endpoint

Add support for Storyblok Content Delivery API endpoint Retrieve Multiple Tags so consumers can fetch tags with optional filtering by starts_with and version, using the same request/query/builder/result patterns already used in the library.

## Architecture and design

- Follow existing endpoint-group pattern already used by Spaces:
  - Endpoint handler class per group
  - Query model + fluent query builder
  - Request model with fixed API path
  - Response DTOs with System.Text.Json attributes
  - Client entry method returning endpoint handler

- Target API contract:
  - Method: GET
  - Path: /v2/cdn/tags
  - Query params:
    - token (inherited from base query)
    - cv (inherited from base query)
    - starts_with (optional)
    - version (optional: draft or published, default published on API side)
  - Response:
    - tags: array of objects with name and taggings_count

- Keep existing style conventions:
  - Task-returning methods without Async suffix
  - File-scoped namespaces
  - Explicit argument validation for delegate overloads
  - Serializer convention with JsonPropertyName attributes

## Decisions

- Version parameter will use an enum type.
- Tag DTO type name will be StoryblokTag.
- Validation for version values will rely on pass-through behavior to the upstream API.

## Tasks

- [ ] Create Tags endpoint models in package:
  - Add packages/StoryblokDotNet.ContentDeliveryApi/Tags/RetrieveMultipleTagsQuery.cs
  - Add packages/StoryblokDotNet.ContentDeliveryApi/Tags/RetrieveMultipleTagsQueryBuilder.cs
  - Add packages/StoryblokDotNet.ContentDeliveryApi/Tags/RetrieveMultipleTagsRequest.cs
  - Add packages/StoryblokDotNet.ContentDeliveryApi/Tags/RetrieveMultipleTagsResponse.cs
  - Add packages/StoryblokDotNet.ContentDeliveryApi/Tags/StoryblokTag.cs

- [ ] Implement query behavior:
  - RetrieveMultipleTagsQuery should inherit StoryblokContentDeliveryQuery
  - Add StartsWith and Version properties
  - Ensure GetParameters emits starts_with and version only when set

- [ ] Implement fluent builder:
  - Support setting starts_with
  - Support setting version via enum
  - Build returns immutable/correct query object

- [ ] Implement request:
  - RetrieveMultipleTagsRequest should inherit StoryblokContentDeliveryRequest
  - Fixed path should resolve to cdn/tags style used by existing request conventions

- [ ] Implement endpoint handler:
  - Add packages/StoryblokDotNet.ContentDeliveryApi/Tags/StoryblokContentDeliveryApiTags.cs
  - Add two overloads:
    - Query object overload
    - Builder delegate overload
  - Use StoryblokContentDeliveryApiHttpClient.Get pipeline and existing cache options pattern

- [ ] Expose endpoint from main client:
  - Update packages/StoryblokDotNet.ContentDeliveryApi/StoryblokContentDeliveryApiClient.cs
  - Add public Tags() factory method returning StoryblokContentDeliveryApiTags
  - Place alongside existing Spaces() endpoint grouping

- [ ] Add tests for endpoint behavior:
  - Create packages/StoryblokDotNet.ContentDeliveryApi.Tests/Tags/StoryblokContentDeliveryApiTagsTests.cs
  - Validate:
    - Correct URI path and query composition
    - Query overload and builder overload both work
    - Response deserializes tags correctly
    - Cancellation token and cache options pass-through

- [ ] Add tests for query and builder:
  - Create packages/StoryblokDotNet.ContentDeliveryApi.Tests/Tags/RetrieveMultipleTagsQueryTests.cs
  - Validate:
    - Defaults include token/cv behavior from base query
    - Optional starts_with omitted when null
    - Version included only when set
    - Builder output equals manual query creation

- [ ] Documentation and backlog hygiene:
  - Mark TODO item complete in TODO.md once implemented
  - Add short usage example in docs location used by similar endpoints (if endpoint docs folder exists)
  - If architecture/contributing docs are missing endpoint-group conventions, add a small clarification

## Open questions

None.

## Proposed implementation order

1. Models and request/query/builder
2. Endpoint handler
3. Client exposure via Tags()
4. Tests
5. Docs and TODO update
