---
name: docs-product-file
description: 'Generate or refresh PRODUCT.md for this repository, including purpose, goals, current features, roadmap direction, and suggested additional topics'
argument-hint: 'Optional: audience, planning horizon, focus areas, release context'
agent: 'agent'
---

# Create or update `PRODUCT.md` for this repository

## Default stance for this prompt

- Audience priority is consumers first.
- Roadmap may include inferred likely next steps, and should be easy to manually revise later.
- Output is text-only and succinct, and should be reasonably non-technical as technical content will be covered elsewhere.

If `PRODUCT.md` already exists:

- Update it in place.
- Preserve accurate existing information.
- Remove stale details only when contradicted by current source files.
- Keep section headings stable where reasonable to minimize churn.

If `PRODUCT.md` does not exist:

- Create a complete first version.

Use repository evidence first (for example `README.md`, `TODO.md`, `docs/`, `packages/`, solution and project files). Do not invent facts. If something is unknown, state it explicitly as "Needs confirmation".

## Style requirements

- Be concise and specific.
- Use sentence case for headings.
- Prefer bullet points over long paragraphs.
- Keep wording minimal and scannable.
- Do not include diagrams or non-text artifacts, unless it would benefit comprehension.
- Use neutral, factual language.
- Separate verified facts from assumptions.

If prompt arguments are provided in chat:

- Apply them as constraints (for example audience, timeline, focus area).
- Mention which constraints were applied.

If audience is not specified in prompt arguments:

- Prioritize repository consumers over contributor/maintainer detail.

## Output

- Edit or create `PRODUCT.md` under the `.agents` folder in this repository.
- Briefly summarize what changed (or what was created).

Use this structure unless a better fit is strongly justified:

```markdown
# PRODUCT

## Purpose

- What this project is
- The problem it solves
- Intended users/consumers

## Product goals

- Primary goals
- Quality goals (for example reliability, performance, usability, maintainability)

## Current scope and features

- What is implemented today
- Key capabilities grouped by area
- Notable constraints and non-goals

## Roadmap and direction

- Planned or likely next features from available docs/issues/todo notes
- Reasonable inferred likely next steps when direct roadmap items are sparse
- Keep roadmap statements lightweight and clearly editable as planning evolves
- Directional themes when concrete roadmap items are not yet defined
- Risks or dependencies that can affect roadmap delivery

## Suggested additional topics

Include suggested sections that would strengthen this file. For each suggestion, explain why it is useful for this repository. Typical candidates:
- Target personas and usage scenarios
- Success metrics and acceptance signals
- Versioning and support policy
- Security and compliance considerations
- Release and change communication model
- Known limitations and tradeoffs
- Decision log links (ADR/design notes)

## Open questions

- List unresolved product questions that should be answered to improve planning
```
