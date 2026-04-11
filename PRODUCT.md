# StoryblokDotNet Product Overview

## Purpose

- Provide a .NET SDK for integrating Storyblok into .NET applications.
- Reduce integration effort for teams using Storyblok Content Delivery API in C#.
- Serve .NET developers who currently have limited Storyblok SDK choices compared with other ecosystems.

## Product goals

- Deliver a reliable, easy-to-adopt Storyblok SDK for .NET consumers.
- Reach feature parity over time with common Storyblok capabilities available in other languages/frameworks.
- Keep setup simple while supporting production needs such as caching, retries, and regional configuration.

## Current scope and features

- Current product scope is the Storyblok Content Delivery API client library.
  - DI-first setup with sensible defaults and optional manual wiring.
  - Typed result/error model for predictable request handling.
  - Built-in caching support with HybridCache integration and cache-version handling.
  - Built-in resilience support for retries, transient errors, and rate limits.
  - Multi-region configuration and region-specific client access.
  - Endpoint areas currently available in the SDK: Spaces and Tags.

## Roadmap and direction

- Planned/known next area:
  - Continue implementation of all documented Content Delivery API endpoints until full coverage is reached.
- Ongoing direction:
  - Preview bridge support helpers/components.
  - Rich text support helpers/components.
  - Coverage of Management API.
