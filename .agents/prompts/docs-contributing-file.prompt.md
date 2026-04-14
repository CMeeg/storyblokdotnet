---
name: docs-contributing-file
description: 'Generate or refresh CONTRIBUTING.md for this repository, including setup, build, run, test, style, and contribution workflow.'
argument-hint: 'Optional focus (for example: add release process, document CI checks, tighten PR checklist).'
agent: 'agent'
---

# Create or update CONTRIBUTING.md for this repository

## Goal

- Ensure contributors can quickly set up, build, run, test, and submit changes correctly.
- Keep the document accurate to the current workspace structure and commands.
- If CONTRIBUTING.md already exists, update it in place while preserving still-valid sections and improving outdated or missing content.

## Inputs

- User argument from this prompt invocation: {{input}}
- Current repository files and docs.

## Requirements

1. Discover current conventions and commands from the repository before writing:
- AGENTS.md
- README.md
- docs/* (especially setup/configuration/resilience/caching guidance)
- Directory.Build.props and Directory.Packages.props when relevant
- Test project files under packages/StoryblokDotNet.ContentDeliveryApi.Tests

2. Include these sections in CONTRIBUTING.md:
- Development environment setup
- Required secrets and environment variables (or explicitly state none are required if true)
- Build instructions
- Run instructions
- Test instructions
- Code style and conventions
- How to submit changes (branching, commit/PR expectations, review readiness)

3. For setup and execution instructions:
- Use concrete commands that match this repository.
- Include platform notes only if they are necessary.
- Prefer concise, copy-pasteable command examples.

4. For code style guidance:
- Reflect repository rules (editorconfig, naming/style conventions, testing conventions).
- Do not invent standards that are not present in the repo.

5. For submission guidance:
- Keep branching guidance generic; do not enforce a specific branch naming convention.
- Recommend clear, descriptive commit messages from contributors.
- State that maintainers may squash merge and apply a conventional commit message at merge time.
- Provide a practical, lightweight default pull request checklist.
- Mention required validation before opening a PR (at minimum build + tests).

6. Update behavior when CONTRIBUTING.md exists:
- Keep stable content that remains correct.
- Replace stale or conflicting guidance.
- Normalize structure and headings for readability.
- Avoid unnecessary rewrites of unchanged sections.

## Writing style rules

- Prefer clarity over completeness.
- Use sentence case for headings.
- Use short paragraphs and flat bullet lists.
- Keep examples minimal and representative.
- Be explicit about assumptions.
- Avoid speculative claims not supported by the repository.

## Output format

- Edit or create `CONTRIBUTING.md` in the workspace root.
- Return a concise summary containing:
  - Whether file was created or updated
  - Major sections added or changed
  - Any assumptions made (especially around secrets/env vars)
  - Suggested follow-ups if repository information is missing or ambiguous

## Quality bar

- Accurate to the current codebase.
- Clear for first-time contributors.
- Actionable and concise.
