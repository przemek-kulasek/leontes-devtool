# KAN-4 Implementation Notes

## What was changed

### New model records (Application/Models)
- `JiraComment.cs` — `(Author, BodyMarkdown, Created)` for a single Jira comment.
- `JiraAttachment.cs` — `(FileName, MimeType, ContentUrl, Size)` + helper `IsImage` for filtering.
- `JiraTicket.cs` — three new positional parameters appended: `AcceptanceCriteria`, `Comments`, `Attachments`.

### Interface changes (Application)
- `IJiraClient` — new method `DownloadAttachmentsAsync(attachments, targetFolder, ct)` returns count of downloaded images.
- `AppSettings` — new property `JiraAcceptanceCriteriaFieldId` (default `"customfield_10016"`).

### JiraCloudClient (Infrastructure/Jira)
- API call now requests `comment,attachment,<acFieldId>` in addition to previous fields.
- `Parse` now accepts `acFieldId` and calls three new private parsers:
  - `ParseAcceptanceCriteria` — handles both ADF object and plain-string custom field values.
  - `ParseComments` — reads `fields.comment.comments[]`, converts ADF body via `AdfToMarkdown`.
  - `ParseAttachments` — reads `fields.attachment[]`.
- `DownloadAttachmentsAsync` — filters to `IsImage`, authenticates with Basic auth, downloads into `targetFolder`.

### PromptGenerator (Application)
- `ComposeSpecMarkdown` now emits `### Acceptance Criteria` and `### Discussion` (comments) sections when data is present.

### ViewModel changes (Desktop)
- `TaskViewModel` — two new observable properties: `TicketAcceptanceCriteria`, `TicketAttachmentsInfo`; new `ObservableCollection<JiraCommentItemViewModel> TicketComments`.
- `FetchTicketAsync` — populates all new fields, calls `DownloadAttachmentsAsync`, and sets `TicketAttachmentsInfo` summary string.
- `EffectiveTicket` — forwards `TicketAcceptanceCriteria` and `_ticket?.Comments/Attachments` to the constructed `JiraTicket`.
- `SettingsViewModel` — exposes `JiraAcceptanceCriteriaFieldId` (load + save).

### View changes (Desktop)
- `TaskView.axaml` (Intake step):
  - Attachment info banner shown below Fetch button when present.
  - New "Acceptance Criteria" editable text box (pre-filled from fetch, manually editable).
  - New "Comments" section showing fetched comments as read-only cards.
- `SettingsWindow.axaml` — new "Acceptance criteria field ID" text box under the Jira section.

## Decisions

### Acceptance criteria field ID
Jira doesn't have a standard field for acceptance criteria; the ID varies per instance.  
`customfield_10016` is the most common default in Atlassian Cloud templates.  
The field ID is exposed in Settings so teams can override it without a code change.

### Only images are auto-downloaded
Non-image attachments (PDFs, ZIPs, etc.) are listed in the info banner but not downloaded,
as they have no direct use in the `images/` folder referenced by the kickoff prompt.

### Comments in the spec
All non-empty comments are included in the `### Discussion` section of the generated spec.
This gives the LLM context about decisions made in the ticket thread.

### Acceptance criteria is editable
The AC text box is two-way bound so the user can paste or refine criteria when the
custom field is empty or uses a different field ID.
