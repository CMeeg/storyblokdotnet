---
name: docs-finder
description: 'Locate relevant documentation for languages, frameworks, platforms, and APIs. Use when planning features, reviewing code, validating implementations, or documenting APIs.'
argument-hint: 'Describe what you need documentation for (e.g., "HttpClient resilience patterns", "Storyblok asset upload", "Entity Framework configuration")'
---

# Documentation finder

A structured workflow for discovering authoritative documentation across your tech stack without guessing or assuming knowledge.

## When to use

- Planning new features or architecture
- Reviewing code against best practices
- Validating implementation approaches
- Writing or updating documentation
- Understanding third-party APIs or frameworks

## Procedure

### 1. Identify Documentation Source

Determine the primary technology:
- **.NET / C# topics** → Use Microsoft Learn MCP tools
- **Storyblok API or CMS topics** → Use Storyblok MCP tools  
- **Libraries, packages, frameworks** → Use Context7 library resolver
- **Other domains** → Web search or user feedback

### 2. Query the Appropriate Tool

**Microsoft Learn MCP** (`microsoft_docs_search`, `microsoft_code_sample_search`, `microsoft_docs_fetch`)
- Search for topic → review results → fetch full documentation if needed
- For code examples, use `microsoft_code_sample_search` with optional language filter

**Storyblok MCP** (`mcp_storyblok_search`, `mcp_storyblok_describe`)
- Search for API operation → get details → proceed to execute if needed

**Context7** (`mcp_io_github_ups_resolve-library-id`, `mcp_io_github_ups_get-library-docs`)
- Resolve package name to library ID → fetch docs with optional topic focus

**Web Search** (as fallback)
- Use fetch_webpage when official sources don't cover the topic
- Prefer documentation sites over blog posts or Stack Overflow

### 3. Review Results

- Evaluate accuracy against your codebase context
- Note if documentation is outdated or incomplete
- Record any gaps for later research

### 4. Handle Missing Documentation

Do not invent facts. If something is unknown, or has been assumed or guessed, state it explicitly as "Needs confirmation".

If no results found:
- Refine search query with more specific terms
- Try alternative tool (e.g., Context7 if MCP tools don't cover it)
- Ask user for context: "Where should I look for X documentation?"
- Check workspace memory for prior research

## Tips

- **Be specific**: "HttpClient timeout configuration" vs. "HttpClient"
- **Include version context**: "Entity Framework 8" vs. "Entity Framework"
- **Chain tools**: Storyblok MCP search → describe → execute often works better than web search
- **Progressive disclosure**: Search → fetch → drill down, don't fetch everything upfront
- **Leverage memory**: Record important documentation discoveries in session or repo memory for future reference

## Related Skills & Customizations

- For code review workflows, combine with your Reviewer agent
- For writing, pair with Docs agent or documentation creation customizations
- For API integration, precede with architecture planning
