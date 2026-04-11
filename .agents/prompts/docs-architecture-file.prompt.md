---
name: docs-architecture-file
description: "Generate or refresh ARCHITECTURE.md for this repository, including the repository's structure, components, interactions, and key design decisions without duplicating existing documentation."
agent: "agent"
---

# Create or update ARCHITECTURE.md

## Goal

Produce or refresh an ARCHITECTURE.md file that helps developers quickly understand:
- What the system is
- How it is organized
- How major parts interact
- Why key architectural decisions were made
- Where to find deeper documentation

## How to approach the task

1. Discover the repository structure and major modules/packages.
2. Identify architectural layers and main runtime/build flows.
3. Identify the most important entry points and integration boundaries.
4. Capture design patterns and non-obvious trade-offs.
5. Link to existing docs instead of repeating detailed instructions.
6. Write concise, scannable sections with diagrams and short examples.

## Required sections

1. Project overview
- One to three paragraphs describing purpose, scope, and architectural style.

2. Technology stack
- Languages, frameworks, runtimes, package managers, and major dependencies.
- Build/test/tooling ecosystem at a high level.

3. Repository structure
- Annotated top-level folder and file map.
- Brief purpose for each major directory/module.

4. Main components and responsibilities
- Core packages/services/classes and what each owns.
- Boundaries between components.

5. Architecture diagram
- Include an ASCII or Mermaid diagram showing layers and dependency direction.
- Annotate data/control flow at a high level.

6. Runtime/Data flow
- Step-by-step flow for a typical request or operation.
- Mention where validation, caching, retries, persistence, and error handling happen (if applicable).

7. Key design decisions
- Patterns used (for example DI, CQRS, repository, message-driven, etc.).
- Important trade-offs and constraints.

8. Extension guide
- Where and how to add new features safely.
- Any conventions that new components should follow.

9. Related documentation
- Link to README, CONTRIBUTING, PRODUCT docs, and docs/* topics where relevant.
- Do not duplicate large sections from those files.

## Optional sections (include if relevant)

- Security model
- Configuration and environment strategy
- Caching strategy and invalidation model
- Resilience/fault-tolerance strategy
- Observability (logging/metrics/tracing)
- Deployment topology and scaling
- Versioning and compatibility policy
- Testing strategy and test architecture
- Known limitations and roadmap notes

## Update mode rules (when ARCHITECTURE.md already exists)

1. Preserve useful structure and headings where possible.
2. Refresh only stale or incorrect sections first.
3. Update diagrams when module boundaries or flows changed.
4. Verify all links still resolve.
5. Remove obsolete statements and dead references.
6. Keep terminology consistent with current code and docs.

## Writing style rules

- Prefer clarity over completeness.
- Use sentence case for headings.
- Use short paragraphs and flat bullet lists.
- Keep examples minimal and representative.
- Be explicit about assumptions.
- Avoid speculative claims not supported by the repository.

## Output constraints

- Output valid Markdown.
- Keep it concise but complete for onboarding.
- Use relative links for internal documentation.
- Include at least one architecture diagram.
- Include at least one annotated repository structure diagram.

## Quality checklist

Before finalizing, verify:

- Every major module has a clear responsibility.
- Data/runtime flow is understandable end-to-end.
- Key architectural decisions are documented with rationale.
- Internal links are valid and non-duplicative.
- Content matches current repository state.
