# Project Instructions — Leontes (DevTool)

A cross-platform Avalonia/MVVM desktop app that organizes and automates the Jira → LLM → review
workflow. Work is organized as **Projects → Features → Tasks**; each task owns a folder on disk and
moves through a non-linear set of workflow steps. Not to be confused with the user's other project
also named "Leontes" (an AI agent) — this is the dev tool.

---

## Agent Rules

### Packages
Only permissive (MIT / Apache-2.0 / BSD) licensed packages are allowed. **Never** add a copyleft
(GPL/LGPL/AGPL) or non-free package. If a task genuinely needs a new package, stop and ask first and
state its license. Current approved set is in *Approved Packages* below.

### When to Ask
Ask before proceeding only when the answer is not covered here — new feature scope, architectural
decisions, ambiguous business logic. Do not ask about naming, formatting, or anything already
defined; follow the spec.

### Tests
No automated tests at this stage (per the product owner). When tests are introduced later, they will
use xUnit v3 in a parallel `backend/tests` tree. Do not add test projects until asked.

### Type Safety
No `dynamic`. Nullable reference types are enabled everywhere; honor them.

### Formatting
Match the existing indentation, naming, and style of the file you edit. Do not reformat unrelated code.

### Secrets
API tokens (Jira, GitHub) are stored only through `ISecretStore` (AES-encrypted file in the app data
dir). Never write secrets into `spec.md`, settings JSON, source, or anything committed.

### Build Health
Zero errors and zero warnings in build output unless explicitly intentional and commented as such.

---

## Architecture

Clean Architecture; dependencies flow inward only. The Application layer is the seam a future MCP
server will sit on, so keep UI and EF concerns out of it.

```
backend/src/Leontes.DevTool.Domain          Entities, value objects, enums, domain exceptions. ZERO dependencies.
backend/src/Leontes.DevTool.Application      Service interfaces, DTO records, IAppDbContext, use-case services. Refs Domain.
backend/src/Leontes.DevTool.Infrastructure   EF Core (SQLite), file/snapshot, Jira/GitHub/Ollama clients, secret/settings stores. Refs Application.
app/Leontes.DevTool.Desktop                  Avalonia MVVM app (composition root). Refs Application + Infrastructure.
```

### Layer rules
- **Domain** — `Entity` base (`Guid Id`, `DateTime CreatedUtc`, `DateTime? ModifiedUtc`). No external deps. Domain exceptions: `DomainException`, `ValidationException`, `NotFoundException`.
- **Application** — Interfaces + DTO **records**. Use-case services (`ProjectService`, `FeatureService`, `TaskService`, `KnowledgeAggregator`, `PromptGenerator`, `ModelRecommendationService`) depend only on abstractions. Registered via `AddApplication()`.
- **Infrastructure** — Adapters only. EF `LeontesDbContext` implements `IAppDbContext`; file/snapshot, Jira (REST v3 + ADF→Markdown), GitHub (Octokit), Ollama (Microsoft.Extensions.AI), AES secret store, JSON settings store. Registered via `AddInfrastructure()`.
- **Desktop** — Views + ViewModels only; no business logic in code-behind. Composition root wires DI in `Program.cs`.

## MVVM Conventions
- Use **CommunityToolkit.Mvvm**: `[ObservableProperty]` for state, `[RelayCommand]` for commands. ViewModels derive from `ViewModelBase` (`ObservableObject`).
- Views are XAML + minimal code-behind (only `InitializeComponent` and trivial dialog close handlers). No logic in code-behind.
- ViewModel→View resolution is by the `ViewLocator` naming convention (`FooViewModel` → `FooView`).
- Dialogs go through `IDialogService`; never `new` a window from a ViewModel.
- Long-running work runs through a `RunAsync` busy/error wrapper; surface errors via `IDialogService`, never crash.

## Persistence
- **SQLite** via EF Core 10. DB at `%APPDATA%/Leontes/leontes.db`. Enums stored as strings. Migrations live in `Infrastructure/Persistence/Migrations` and are applied automatically on startup by `DbInitializer` (runs in `Program.Main` **before** Avalonia starts — never block the UI thread with `.GetResult()` on async EF calls).
- New migration: `dotnet ef migrations add <Name> --project backend/src/Leontes.DevTool.Infrastructure --startup-project backend/src/Leontes.DevTool.Infrastructure` (a design-time `IDesignTimeDbContextFactory` is provided).
- Read queries use `.AsNoTracking()`; explicit `.Include()` (no lazy loading); paginate anything unbounded.
- Services are registered **Transient** (each gets a short-lived `DbContext`); resolve them inside a `CreateScope()` per UI operation. Do not hold a long-lived `DbContext` in a ViewModel.

## Files & Versioning
- The app **owns** the on-disk layout: `<workspaceRoot>/<Project>/<Feature>/<Task-key>/{spec.md, images/, implementation/, review-self/, review-against-past-comments/, review-pr-comments/, .leontes/history/}`. Folder/file names live in `Application/Common/TaskLayout.cs`.
- Versioning is snapshot-based: `IVersioningService.SaveSnapshotAsync` copies the current document into `.leontes/history` and records a `DocumentVersion`. Diffs use DiffPlex; restore snapshots the current file first so it is reversible.

## Good Practices
- Meaningful names, small functions, CQS, DRY, SOLID, YAGNI. Composition over inheritance. Guard clauses, fail fast.
- No "what" comments. Only comment *why* (business logic / edge cases). No AI summary comments.
- Domain exceptions for violated contracts; don't use exceptions for control flow.
- Delete unused code immediately. No speculative abstractions or single-implementation interfaces without reason.

---

## Approved Packages (all permissive)

UI: `Avalonia` (11.3.x, MIT), `FluentAvaloniaUI` (MIT), `Avalonia.AvaloniaEdit` (MIT),
`Markdown.Avalonia` (MIT), `CommunityToolkit.Mvvm` (MIT).
Backend: `Microsoft.EntityFrameworkCore` + `.Sqlite` + `.Design` (MIT), `Microsoft.Extensions.AI`
(MIT), `OllamaSharp` (MIT), `Octokit` (MIT), `DiffPlex` (Apache-2.0),
`Microsoft.Extensions.Hosting/DependencyInjection/Configuration/Http` + `Http.Resilience` (MIT).

## Common Commands
```bash
dotnet build                                         # whole solution
dotnet run --project app/Leontes.DevTool.Desktop     # run the app
dotnet ef migrations add <Name> --project backend/src/Leontes.DevTool.Infrastructure --startup-project backend/src/Leontes.DevTool.Infrastructure
```

## Target
.NET 10 (`net10.0`), Avalonia 11.3. Local LLM via Ollama (`http://localhost:11434`).
