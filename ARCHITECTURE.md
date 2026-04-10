# StoryblokDotNet Architecture

## Project overview

StoryblokDotNet is a .NET SDK focused on Storyblok Content Delivery API integration for C# applications.

This document explains how the repository is organized, how the main components interact at runtime, and where to extend the SDK safely.

## Technology stack

- Language and runtime
  - C# on modern .NET (see [CONTRIBUTING.md](CONTRIBUTING.md))
- Primary package
  - StoryblokDotNet.ContentDeliveryApi (class library)
- Core dependencies
  - This project uses centralised package management, please see [Directory.Packages.props](Directory.Packages.props)
- Test stack
  - xUnit v3

## Repository structure

The repository uses a monorepo layout. The following diagram highlights the most important files and folders:

```text
storyblokdotnet/
├─ StoryblokDotNet.slnx                           # Solution entry point
├─ Directory.Build.props                          # Shared build metadata
├─ Directory.Packages.props                       # Central package version management
├─ README.md                                      # Usage-first quickstart and API examples
├─ CONTRIBUTING.md                                # Build/test workflow and contribution guidance
├─ docs/**/*                                      # In-depth feature and usage guides
└─ packages/
   └─ StoryblokDotNet.ContentDeliveryApi          # Content Delivery API client
```

## Main components and responsibilities

### StoryblokDotNet.ContentDeliveryApi

#### StoryblokContentDeliveryApiClient

- Primary entry point for consumers
- Owns regional client selection and region-specific base address mapping
- Exposes API areas
- Exposes cache invalidation helpers (clear by request, tag, or all)

#### StoryblokContentDeliveryServiceCollectionExtensions

- Provides `AddStoryblokContentDeliveryApi` overloads for:
  - Defaults
  - Delegate-based options
  - IConfiguration binding
- Wires options validation and post-configuration
- Registers the HTTP client and resilience handler once
- Registers either HybridCache-backed caching or no-op caching
- Registers keyed clients per configured region

#### HTTP Layer

- `StoryblokContentDeliveryApiHttpClient`
  - Builds request URI and query parameters
  - Resolves token and cache version (cv)
  - Executes GET requests and converts responses to typed results
  - Maps network, timeout, serialization, and API errors into a typed error model
- `StoryblokContentDeliveryApiResilience`
  - Adds retry behavior with exponential backoff, optional jitter, and optional Retry-After support
  - Retries on transient conditions and configurable status codes

#### Caching Layer

- `IStoryblokContentDeliveryApiCache` defines cache operations
- `StoryblokContentDeliveryApiHybridCache` adapts Microsoft HybridCache to SDK request/response semantics
- `StoryblokContentDeliveryNoOpApiCache` provides a no-op caching strategy that can be used when caching is disabled or opted-out from
- Cache keys are derived from region + request data
- Current space version (cv) can be cached separately and tagged for targeted invalidation

#### Spaces API Area

- `StoryblokContentDeliveryApiSpaces` encapsulates endpoint-specific logic
- Supports both direct query instances and query builder delegates

#### Tags API Area

- `StoryblokContentDeliveryApiTags` encapsulates tag endpoint logic
- Supports both direct query instances and query builder delegates

#### Test Package

- Verifies client behavior, registration behavior, options validation paths, and endpoint behavior
- Uses recording HTTP test doubles to assert outbound request behavior without external dependencies

## Runtime / data flow

Typical flow for retrieving current space:

1. Consumer resolves `StoryblokContentDeliveryApiClient` from DI.
2. Consumer calls `Spaces().RetrieveCurrentSpace(...)` or some other endpoint method.
3. HTTP layer resolves token and query values.
4. If no `token` is provided in the request query a default `token` will try to be located from default client options.
5. If no `cv` value is provided in the request query and endpoint is not `/spaces/me`, HTTP layer attempts to fetch current space version to populate `cv` automatically and cache the result.
6. Cache layer computes a request key and attempts read-through retrieval.
7. On cache miss, HTTP request runs through resilience pipeline and is sent to Storyblok API.
8. Response is deserialized into typed response model or typed error model.
9. Successful results are cached according to entry options for the current request.
10. Caller receives `StoryblokContentDeliveryResult<T>` with response (success) or error (failure) data.

## Key design decisions

- DI-first integration
  - The package is optimized for service registration in Microsoft DI, with sensible defaults and options validation.
- Typed result/error model
  - API operations return `StoryblokContentDeliveryResult<T>` rather than throwing for expected remote failures.
- Region-aware clients
  - Region-specific base addresses are explicit and accessible through `ForRegion(...)`, enabling multi-region deployments.
- Built-in cache version strategy
  - The client can resolve Storyblok `cv` automatically to improve cache freshness behavior.
- Resilience at HTTP boundary
  - Retries and delay strategies are applied where transient transport/API failures happen.
- Extension-by-area model
  - API capabilities are organized into areas (for example `Spaces` and `Tags`), allowing incremental endpoint growth.

## Integration points

- Storyblok Content Delivery API
  - Main external dependency for data retrieval
- Microsoft dependency injection and options
  - Used for configuration, validation, and runtime composition
- HybridCache
  - Optional-but-enabled-by-default cache backend when available/registered
- HTTP resilience pipeline
  - Integrates retry behavior via Microsoft.Extensions.Http.Resilience

## Extension guide

To add a new endpoint area safely:

1. Add request/query/response models under a dedicated folder in [packages/StoryblokDotNet.ContentDeliveryApi](packages/StoryblokDotNet.ContentDeliveryApi).
2. Add an area facade class similar to Spaces and route methods through `StoryblokContentDeliveryApiHttpClient`.
3. Expose the new area from `StoryblokContentDeliveryApiClient`.
4. Add focused tests under matching folder structure in [packages/StoryblokDotNet.ContentDeliveryApi.Tests](packages/StoryblokDotNet.ContentDeliveryApi.Tests).
5. Document feature usage in [README.md](README.md) and topic docs in [docs](docs).

Conventions to preserve:

- Keep request/query/response types strongly typed.
- Keep API errors in the typed result/error path.
- Keep cache and resilience behavior opt-in/opt-out through options, not hard-coded per endpoint.

## Testing strategy

The test package is organized by system-under-test and uses recording HTTP test doubles to make request behavior deterministic and assertable. This keeps tests fast and independent of external Storyblok availability.

Run and contribution workflows are documented in [CONTRIBUTING.md](CONTRIBUTING.md).

## Related documentation

- Usage and quickstart: [README.md](README.md)
- Contribution, build, and test instructions: [CONTRIBUTING.md](CONTRIBUTING.md)
- Feature guides: [docs](docs)
