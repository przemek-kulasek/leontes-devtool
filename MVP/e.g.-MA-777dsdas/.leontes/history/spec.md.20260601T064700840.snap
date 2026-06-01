# e.g. MA-777dsdas — e.g. MA-777dsdas

## Ticket
_No ticket fetched — paste the relevant details here._

## Context
## Project context
A cross-platform Avalonia/MVVM desktop app (Windows/Mac/Linux first, mobile later) that turns your ad-hoc Jira→LLM→review workflow into a structured, non-linear pipeline. It organizes work as Projects → Features → Tasks, where each Task owns a folder on disk (spec.md, images/, implementation/, review-self/,
  review-against-past-comments/, review-pr-comments/). It pulls Jira tickets (Atlassian SDK or manual paste), references
  disk locations + context blurbs, generates a ready-to-use spec.md + an LLM kickoff prompt, integrates GitHub
  (Octokit) for past closed-PR comments and PR review comments, uses local Ollama (Microsoft.Extensions.AI / Agents
  framework) for typo-fixing, context optimization, and per-step model suggestions, has an in-app markdown editor with versioned saves, and inherits knowledge upward (task → feature → project). Clean layered architecture so features can later be exposed as an MCP server.

## Related tasks in this feature
- KAN-4: Intake Screen (Draft)
- KAN-NA1: Materials improvements (Draft)
- MA-NA2: Context dialog (Draft)
- e.g. MA-777: test (Draft)

## Rules for the assistant
- Do not ask about trivial things you can find in the given materials and locations.
- Always ask if something is unclear and you cannot find it in the materials. Never make assumptions or hallucinate.
- Unless specified otherwise, use clean code, DRY, YAGNI and SOLID. Match the existing codebase style and reuse its patterns.
- Always ask before using an external package and present its license information.
- Do not add tests at this stage.
- Keep changes minimal and scoped to this ticket. Do not refactor unrelated code or add speculative features.
- Prefer extending existing code and components over rewriting; reuse what is already there.
- Follow the project's existing error-handling, validation and logging patterns.
- Never put secrets, API keys or credentials in source, config or commits.
- Stop and confirm before any large refactor, dependency change, or schema/migration change.
- Update README/relevant docs when you add, remove or change behavior, dependencies or setup.

## Workspace layout
Files for this task live under the task folder below. Read from and write to these locations:
- `C:\WIP\leontes-devtool\MVP\e.g.-MA-777dsdas` — this task's folder.
- `C:\WIP\leontes-devtool\MVP\e.g.-MA-777dsdas\spec.md` — the spec for this task.
- `C:\WIP\leontes-devtool\MVP\e.g.-MA-777dsdas\images` — designs, screenshots and reference images.
- `C:\WIP\leontes-devtool\MVP\e.g.-MA-777dsdas\implementation\progress.md` — your running progress/decision log (keep this updated).
- `C:\WIP\leontes-devtool\MVP\e.g.-MA-777dsdas\review-self` — write self-review findings here.
- `C:\WIP\leontes-devtool\MVP\e.g.-MA-777dsdas\review-against-past-comments` — notes from checking against past PR comments.
- `C:\WIP\leontes-devtool\MVP\e.g.-MA-777dsdas\review-pr-comments` — responses to this PR's review comments.

## What to do
_Specify what should be done here — the concrete changes, deliverables and what "done" looks like for this task._
