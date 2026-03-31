---
name: "C# and .NET Conventions"
description: "C# and .NET Style and Formatting Conventions for use in this repository."
applyTo: "{**/*.{cs,csproj,props,targets,sln,slnx},**/global.json,**/nuget.config}"
---

# C# and .NET Conventions

- Treat `.editorconfig` as the source of truth for formatting and code style.
- When `.editorconfig` leaves room for judgment, follow the conventions already used in `packages/StoryblokDotNet.ContentDeliveryApi` and `packages/StoryblokDotNet.ContentDeliveryApi.Tests`.
- Use file-scoped namespaces in C# files.
- Use Allman braces.
- Keep `using` directives outside the namespace and place `System` usings first.
- Prefer explicit types over `var` unless the type is immediately obvious and readability improves.
- Keep private and internal fields in camelCase and const fields in PascalCase.
- Preserve the existing package layout: production code in `packages/StoryblokDotNet.ContentDeliveryApi` and tests in `packages/StoryblokDotNet.ContentDeliveryApi.Tests`.
- Keep package version definitions centralized in `Directory.Packages.props`.
- Keep shared build and package metadata in `Directory.Build.props`.
- Do not add NuGet package versions directly in project files unless the repository already has a documented exception.
- Build warnings should be fixed, not ignored, unless there is a good reason to ignore them in which case that should be documented next where the exclusion appears.
- Interface names should be prefixed with an uppercase `I`.
- In this repository, do not add the `Async` suffix to method names by default, even for `Task`-returning methods. Only use it when required for compatibility with an external API or to avoid an unavoidable overload conflict.

## Test Conventions

- When adding tests, place xUnit v3 test classes under `packages/StoryblokDotNet.ContentDeliveryApi.Tests` with public test classes and `[Fact]` or `[Theory]` methods so `dotnet test` can discover them.
- Keep test classes public.
- Organize tests by system under test: keep each production type's tests in a dedicated file (for example, `StoryblokContentDeliveryApiHttpClientFactoryTests.cs` and `StoryblokContentDeliveryServiceCollectionExtensionsTests.cs`) instead of combining unrelated test groups in one file.
- Keep each production type's tests in a dedicated file that matches the name and location of the corresponding of the SUT's source file (as far as is possible).
- Name test methods using a concise underscore-separated `Subject_With_ExpectedResult` style where `Subject` is usually the method under test, `With` is a brief summation of the arguments or context used, and `ExpectedResult` is a brief summation of the expected result or primary assertion (for example, `Create_WithoutOptions_UsesEuDefaults`).
