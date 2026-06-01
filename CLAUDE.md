# CLAUDE.md

Guidance for Claude Code working in this repo. Full conventions live in
[.github/copilot-instructions.md](.github/copilot-instructions.md) — read it; the points below are
the ones easiest to get wrong.

## What this is
**Leontes (DevTool)** — a cross-platform Avalonia/MVVM desktop app that organizes and automates the
Jira → LLM → review workflow. Work is **Projects → Features → Tasks**; each task owns a disk folder
and a non-linear set of workflow steps. (Distinct from the user's other "Leontes", an AI agent.)

## Architecture (Clean Architecture, deps flow inward)
- `backend/src/Leontes.DevTool.Domain` — entities/enums/exceptions, zero deps.
- `backend/src/Leontes.DevTool.Application` — interfaces, DTO records, use-case services, `IAppDbContext`. This is the seam a future **MCP server** plugs into — keep UI/EF out.
- `backend/src/Leontes.DevTool.Infrastructure` — EF Core (SQLite), file/snapshot, Jira/GitHub/Ollama clients, AES secrets, JSON settings.
- `app/Leontes.DevTool.Desktop` — Avalonia MVVM; composition root in `Program.cs`.

## Non-negotiables
- **Packages:** permissive licenses only (MIT/Apache/BSD). Never add copyleft/non-free. Ask first, state the license.
- **Tests:** none at this stage. Don't add test projects until asked.
- **Secrets:** only via `ISecretStore`. Never commit or write tokens into files/spec/source.
- **Build:** zero warnings, zero errors.
- **MVVM:** CommunityToolkit (`[ObservableProperty]`, `[RelayCommand]`); no logic in code-behind; dialogs via `IDialogService`.
- **EF:** services are Transient with short-lived `DbContext`; resolve inside `CreateScope()` per operation. Never block the UI thread with `.GetResult()` on async EF — DB init runs in `Program.Main` before Avalonia starts.

## Commands
```bash
dotnet build
dotnet run --project app/Leontes.DevTool.Desktop
dotnet ef migrations add <Name> --project backend/src/Leontes.DevTool.Infrastructure --startup-project backend/src/Leontes.DevTool.Infrastructure
```

Target: .NET 10, Avalonia 11.3. Local LLM: Ollama at `http://localhost:11434`.
