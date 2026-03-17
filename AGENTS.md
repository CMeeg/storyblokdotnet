# Project Guidelines

## Architecture

* This workspace is a .NET solution with three top-level concerns:
  * Aspire app host in `apphost.cs` for local orchestration.
  * Main package in `packages/StoryblokDotNet.ContentDeliveryApi`.
  * Test package in `packages/StoryblokDotNet.ContentDeliveryApi.Tests`.
* Keep package version definitions centralized in `Directory.Packages.props`.
* Keep shared build and package metadata in `Directory.Build.props`.

## Build and Test

* Primary solution build command: `dotnet build StoryblokDotNet.slnx`
* Test command: `dotnet test`
* App host run command: `dotnet run --project apphost.cs`
* When adding tests, place xUnit v3 test classes under `packages/StoryblokDotNet.ContentDeliveryApi.Tests` with public test classes and `[Fact]` or `[Theory]` methods so `dotnet test` can discover them.

## Code Style

* Follow `.editorconfig` strictly.
* Use file-scoped namespaces for C#.
* Use Allman braces and prefer explicit types over `var` unless clarity is improved.
* Keep `using` directives outside namespaces and keep `System` directives first.

## Conventions

* New NuGet dependencies should be added with central package management in `Directory.Packages.props`.
* Preserve existing folder boundaries: package code in `packages/StoryblokDotNet.ContentDeliveryApi` and tests in `packages/StoryblokDotNet.ContentDeliveryApi.Tests`.
* Organize tests by system under test: keep each production type's tests in a dedicated file (for example, `StoryblokContentDeliveryHttpClientFactoryTests.cs` and `StoryblokContentDeliveryServiceCollectionExtensionsTests.cs`) instead of combining unrelated test groups in one file.
* Name test methods using a concise underscore-separated `Subject_With_ExpectedResult` style where `Subject` is usually the method under test, `With` is a brief summation of the arguments or context used, and `ExpectedResult` is a brief summation of the expected result or primary assertion (for example, `Create_WithoutOptions_UsesEuDefaults`).
