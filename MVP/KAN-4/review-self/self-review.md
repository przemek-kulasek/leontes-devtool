# Self-review — KAN-4 Intake Screen

Reviewed against: correctness, clean code, DRY, YAGNI, SOLID, codebase consistency, security.

---

## 🔴 Issues found — both fixed during review

### Path traversal in `DownloadAttachmentsAsync`
**File:** `Infrastructure/Jira/JiraCloudClient.cs`

`att.FileName` is supplied directly by the Jira REST API. A crafted attachment filename
such as `../../evil.sh` or `..\..\..\Windows\System32\hosts` would have caused
`Path.Combine(targetFolder, att.FileName)` to resolve outside the intended `images/` folder,
allowing arbitrary file writes anywhere the process has permission.

**Fix applied:** `Path.GetFileName(att.FileName)` strips any directory components before
the path is combined. An empty result (e.g. filename that is purely path separators)
is now skipped with `continue`.

```csharp
// before
var dest = Path.Combine(targetFolder, att.FileName);

// after
var safeFileName = Path.GetFileName(att.FileName);
if (string.IsNullOrWhiteSpace(safeFileName))
    continue;
var dest = Path.Combine(targetFolder, safeFileName);
```

---

## ✅ No further issues found

### HTTP authentication — correct
`DownloadAttachmentsAsync` reconstructs the Basic auth header from live settings/secrets,
consistent with how `GetIssueAsync` does it. Credentials are never stored in a field.

### Cancellation token threading — correct
Every `await` that accepts a `CancellationToken` passes `ct` through, matching the existing
client code.

### Error handling — consistent
`EnsureSuccessStatusCode()` is used for individual attachment downloads, same as the issue
fetch. The caller (`TaskViewModel.FetchTicketAsync`) is wrapped in the `RunAsync` guard which
catches and surfaces exceptions via `IDialogService` — no change needed.

### Null / empty-string handling — correct
`ParseAcceptanceCriteria` returns `null` for missing, null-kind, or whitespace-only values.
`ParseComments` skips entries with empty bodies. `ParseAttachments` skips entries with
missing filename or content URL. All consistent with defensive patterns in `AdfToMarkdown`.

### ADF reuse — DRY
Comment bodies and acceptance-criteria rich-text fields are converted through the existing
`AdfToMarkdown.Convert` helper. No duplication of conversion logic.

### `JiraTicket` record extension — backward compatible
New positional parameters are appended at the end. The only other constructor call site is
`EffectiveTicket()` in `TaskViewModel`, which was updated in the same commit. No other
code constructs `JiraTicket` directly.

### `TicketComments` visibility in XAML — fixed
`{Binding TicketComments.Count, Converter={x:Static ObjectConverters.IsNotNull}}` was
incorrect: `Count` is a non-nullable `int`, so `IsNotNull` always evaluates to `true`,
making the "Comments" header permanently visible even when there are no comments.

**Fix applied:** Added `[ObservableProperty] private bool _hasTicketComments;` to
`TaskViewModel`, set to `TicketComments.Count > 0` immediately after populating the
collection. The XAML binding is now simply `IsVisible="{Binding HasTicketComments}"`.
This follows the established CommunityToolkit.Mvvm pattern in the codebase for derived
boolean state.

### Settings default — safe
`JiraAcceptanceCriteriaFieldId` defaults to `"customfield_10016"` in `AppSettings` (not in
`AppSettings`'s JSON file on disk). If the persisted settings JSON predates this field, the
default kicks in on deserialization — no migration needed.

### `DownloadAttachmentsAsync` on `IJiraClient` — appropriate
Placing the download method on `IJiraClient` keeps the Infrastructure detail (HTTP + auth)
inside the Infrastructure layer. The Application layer only sees the interface. Consistent
with the `IGitHubClient` pattern.

### Spec generation — no regressions
`PromptGenerator.ComposeSpecMarkdown` only emits the new sections when `AcceptanceCriteria`
and `Comments` are non-empty/non-null, so existing callers that pass a manually-typed
`JiraTicket` with those fields empty are unaffected.
