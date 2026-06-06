# Agent Instructions for Codebelt.Extensions.Swashbuckle.AspNetCore

This document provides guidance for AI agents working in this repository.

## Project Overview

Codebelt.Extensions.Swashbuckle.AspNetCore is a suite of .NET libraries providing uniform, opinionated, and extensible APIs for Swagger and OpenAPI documentation with ASP.NET Core. The solution targets .NET 10.0 and .NET 9.0. It includes:

- **Codebelt.Extensions.Swashbuckle.AspNetCore** — Core extensions for Swagger/OpenAPI generation, customization, and configuration with ASP.NET Core.
- **Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol** — Model Context Protocol (MCP) integration for Swagger/OpenAPI documentation discovery and tooling.

## Coding Standards

- **Text encoding:** UTF-8 for text files (enforced via `.editorconfig`)
- **Template rewrites:** Preserve UTF-8 explicitly when scripts or tools rewrite text files; avoid locale-dependent encoding defaults
- **Namespaces:** File-scoped namespaces are required (enforced via `.editorconfig`)
- **Top-level statements:** Not allowed (enforced via `.editorconfig`)
- **Language version:** Always use the latest C# features (`LangVersion=latest`)
- **Nullable:** Enable nullable reference types in all new code
- **XML documentation:** All public APIs must have XML documentation comments
- **Testing:** Use xUnit v3 with Codebelt.Extensions.Xunit.App base classes

## Project Structure

- `src/` — Production source code
  - `Codebelt.Extensions.Swashbuckle.AspNetCore/` — Core Swagger/OpenAPI extensions and ASP.NET Core integration
  - `Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol/` — Model Context Protocol (MCP) integration
- `test/` — Unit and functional tests (project names end with `Tests` or `FunctionalTests`)
- `tooling/` — Development tools and example applications
- `.nuget/` — Per-package NuGet metadata (icon, README, release notes)
- `.docfx/` — DocFX documentation configuration
- `.github/` — CI/CD workflows, contributing guidelines, Copilot instructions

## Test Conventions

- Test project names must end with `Tests` (unit tests) or `FunctionalTests` (integration/functional tests)
- Test classes should inherit from the `Test` base class in `Codebelt.Extensions.Xunit`
- Use `Microsoft.Testing.Platform` as the test runner (`UseMicrosoftTestingPlatformRunner=true`)
- All tests are executable (`OutputType=Exe`)
- Test namespaces must match the SUT namespace (System Under Test), without `.Tests` or `.FunctionalTests` suffix
- See `.github/copilot-instructions.md` for detailed test writing guidelines

## Build & CI

- Centralized package versions via `Directory.Packages.props`
- Resolve new or updated `Directory.Packages.props` versions from NuGet.org and keep them on the latest stable listed releases
- Centralized build configuration via `Directory.Build.props`
- MinVer for semantic versioning from Git tags
- Strong-name signing is enabled in CI environments (`CI=true`)
- Keep `.github/dependabot.yml` enabled at the repo root so central NuGet package management stays current

## .bot/ Folder

If a `.bot/` folder exists at the root, it contains **confidential, local-only** working material for AI agents — product requirement documents (PRDs), design proposals, agentic loop state, and brainstorming outputs. This folder is gitignored and never committed.

When starting creative or design work (new features, architecture decisions, PRD drafts), use the [brainstorming skill](https://skills.sh/obra/superpowers/brainstorming) and save outputs to `.bot/`. Only move finalized, non-confidential instructions into `AGENTS.md` or `.github/copilot-instructions.md`.

## Git Operations Safeguards

Agents must never automatically commit code changes or push to remote repositories. Both actions require explicit user approval:

- **Commits**: Always request confirmation from the user before staging and committing code. Present a clear summary of the changes and wait for approval before executing the commit.
- **Remote Operations**: Do not push, pull, fetch, or interact with `origin` or any remote repository without explicit user instruction. These operations modify repository history and can cause data loss if performed unexpectedly.

**Rationale:** Automatic commits can clutter history with incomplete work, temporary debugging code, or unintended changes. Unexpected remote operations risk overwriting or losing commits on shared branches. Always require explicit user approval before performing these actions.

## Official Documentation

- Public API conventions belong in `.docfx/api/namespaces/` and should be treated as the official documentation source for library behavior and naming vocabulary.
- When adding or renaming public APIs, update the relevant namespace page in `.docfx/api/namespaces/` if the change introduces or clarifies a convention.
- Keep internal reasoning, exploratory notes, and agent discussion out of DocFX pages; summarize only stable public guidance.
