# Contributing to StoryblokDotNet

Thanks for contributing to this repository.

This guide covers local setup, build and test commands, style conventions, and how to submit changes.

## Development environment setup

### Prerequisites

- .NET SDK 10.0.104 or newer feature band compatible with [global.json](global.json)
- Git

### Clone and restore

```bash
git clone https://github.com/CMeeg/storyblokdotnet.git
cd storyblokdotnet
dotnet restore StoryblokDotNet.slnx
```

## Secrets and environment variables

### Required for building and tests

- No secrets are required to build or run the test suite.
- No repository-specific environment variables are required for build and test commands.

### Needed when running code against the Storyblok API

If you run sample integration code or application code that makes Storyblok requests, provide a Content Delivery API token.

Configuration key used by the library examples:

- `Storyblok:ContentDelivery:Token`

Common ways to provide it:

1. `appsettings.Development.json`
2. User Secrets
3. Environment variable using double underscore separators:
   `STORYBLOK__CONTENTDELIVERY__TOKEN`

Region can also be configured with:

- `Storyblok:ContentDelivery:Region`

If Region is omitted, defaults are documented in [docs/configuration.md](docs/configuration.md).

## Build instructions

Build the solution:

```bash
dotnet build StoryblokDotNet.slnx
```

## Test instructions

Run all tests:

```bash
dotnet test
```

## Code style and conventions

- Read [ARCHITECTURE.md](ARCHITECTURE.md) for an overview of the project architecture and repository structure.
- For .NET and C# conventions see [dotnet-style.instructions.md](.agents/instructions/dotnet-style.instructions.md).

## Submitting changes

### Branching

Use any clear branch name that communicates intent. No specific naming convention is required.

### Commits

Use clear, descriptive commit messages that explain what changed and why.

Maintainers may squash merge and apply a conventional commit message at merge time.

### Pull requests

Use this lightweight checklist before opening a PR:

- Check that the contribution aligns with [product goals](PRODUCT.md) and [architecture guidance](ARCHITECTURE.md).
- Build succeeds locally.
- Tests pass locally.
- Changes follow repository style and placement conventions.
- Documentation is updated when behaviour, configuration, or supported features are changed.
- PR description explains motivation, scope, and any noteworthy tradeoffs.

## Related documentation

Please review the main document in [README.md](README.md) as well as documentation for specific feature and scenarios under the [docs](docs) folder.
