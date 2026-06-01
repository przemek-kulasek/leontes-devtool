# Leontes

A cross-platform desktop app that turns the ad-hoc *Jira → spec → LLM → implement → review* workflow
into a structured, non-linear pipeline. Organize work as **Projects → Features → Tasks**, pull Jira
tickets, gather materials, compose a versioned `spec.md`, generate ready-to-paste LLM prompts, and
cross-check against GitHub PR history — all in one place.

> Not to be confused with the separate "Leontes" AI-agent project. This repository is the dev tool.

## Features

- **Projects → Features → Tasks** tree with knowledge that inherits downward (project context →
  feature context → task).
- **Non-linear Steps pane** per task — jump between steps in any order:
  1. **Intake** — fetch a Jira Cloud ticket by key, or paste details manually.
  2. **Materials** — attach folders, links and references; mark which appear in the spec.
  3. **Rules** — checklist of assistant rules (don't assume, clean code/SOLID, ask before adding packages, …).
  4. **Context** — inherited project/feature context plus task-specific notes you define and save
     here (so they aren't repeated in the spec).
  5. **Spec & Kickoff** — markdown editor with live preview, versioned saves (snapshot + diff +
     restore), one-click `spec.md` generation, a copy-ready kickoff prompt, and an optional local-LLM
     **Optimize** that de-duplicates and tightens the spec — shown as a diff to Apply or Discard before
     it overwrites your text.
  6. **Implement** — capture your own decisions/notes into `implementation/notes.md`, and follow the
     coding agent's running log read-only from `implementation/progress.md` (the kickoff prompt asks the
     agent to keep it updated; hit **Reload** to pull the latest).
  7. **Self review** — generate a code-review prompt into `review-self/`.
  8. **Past PR comments** — pull human comments from closed GitHub PRs and build a check prompt.
  9. **PR comments** — fetch the current PR's comments and build a "what's worth implementing" prompt.
- **App-owned folder layout** on disk per task: `spec.md`, `images/`, `implementation/`,
  `review-self/`, `review-against-past-comments/`, `review-pr-comments/`, `.leontes/history/`.
- **Versioning** — every save snapshots the document; view diffs and restore previous versions.
- **Local LLM helpers** via Ollama (typo fixing, spec optimization, per-step model suggestions).
- **Secrets** stored AES-encrypted on the local machine, never in project files.

## Architecture

Clean Architecture (dependencies flow inward); the Application layer is the seam for a future MCP server.

| Project | Responsibility |
|---|---|
| `Leontes.DevTool.Domain` | Entities, enums, domain exceptions (zero deps) |
| `Leontes.DevTool.Application` | Service interfaces, DTOs, use-case services, `IAppDbContext` |
| `Leontes.DevTool.Infrastructure` | EF Core (SQLite), file/snapshot, Jira/GitHub/Ollama clients, secret/settings stores |
| `Leontes.DevTool.Desktop` | Avalonia 11 MVVM UI (composition root) |

## Download

Prebuilt, self-contained builds (no .NET install required) are attached to each
[GitHub Release](../../releases) — grab the archive for your platform:

| Platform | Asset |
|---|---|
| Windows (x64) | `leontes-<version>-win-x64.zip` |
| Linux (x64) | `leontes-<version>-linux-x64.tar.gz` |

Unpack and run the `Leontes.DevTool.Desktop` executable (on Linux, `chmod +x` it first).

> macOS builds are paused for now; see the matrix in
> [`.github/workflows/release.yml`](.github/workflows/release.yml) for how to re-enable them.

### Cutting a release (maintainers)

Releases are built by [`.github/workflows/release.yml`](.github/workflows/release.yml). Push a version
tag and the workflow builds all platforms and publishes the Release automatically:

```bash
git tag v0.1.0
git push origin v0.1.0
```

Run the workflow manually (**Actions → Release → Run workflow**) to produce build artifacts for testing
without publishing a Release.

## Requirements

To build from source:

- .NET 10 SDK
- (Optional) [Ollama](https://ollama.com) running locally for LLM helpers — default
  `http://localhost:11434`, default model `qwen2.5:7b-instruct`.

## Run

```bash
dotnet build
dotnet run --project app/Leontes.DevTool.Desktop
```

On first launch the SQLite database and config are created under `%APPDATA%/Leontes`
(`~/.config/Leontes` on Linux/macOS). Open **Settings** to set your workspace root folder, Jira
Cloud credentials, GitHub token, and Ollama endpoint.

## Configuration

| Setting | Where |
|---|---|
| Workspace root, Jira base URL/email, Ollama endpoint/model, GitHub owner/repo | `%APPDATA%/Leontes/settings.json` |
| Jira API token, GitHub PAT | AES-encrypted `%APPDATA%/Leontes/secrets.dat` |
| Database | `%APPDATA%/Leontes/leontes.db` (SQLite) |

## Development

```bash
# Add an EF Core migration
dotnet ef migrations add <Name> \
  --project backend/src/Leontes.DevTool.Infrastructure \
  --startup-project backend/src/Leontes.DevTool.Infrastructure
```

See [.github/copilot-instructions.md](.github/copilot-instructions.md) for full conventions.

## Tech & licenses

Avalonia, FluentAvalonia, AvaloniaEdit, Markdown.Avalonia, CommunityToolkit.Mvvm (MIT); DiffPlex
(Apache-2.0); EF Core + SQLite, Microsoft.Extensions.AI, OllamaSharp, Octokit, Microsoft.Extensions.*
(MIT). All dependencies use permissive licenses.
