using System.IO;
using System.Text;
using Leontes.DevTool.Application.Common;
using Leontes.DevTool.Application.Models;
using Leontes.DevTool.Domain.Entities;

namespace Leontes.DevTool.Application.Services.Implementation;

public sealed class PromptGenerator : IPromptGenerator
{
    public string ComposeSpecMarkdown(
        WorkTask task,
        JiraTicket? ticket,
        IReadOnlyList<MaterialLink> materials,
        IReadOnlyList<RulePreset> rules,
        string composedContext)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {task.JiraKey} — {task.Title}");
        sb.AppendLine();

        sb.AppendLine("## Ticket");
        if (ticket is not null)
        {
            sb.AppendLine($"- **Key:** {ticket.Key}");
            sb.AppendLine($"- **Summary:** {ticket.Summary}");
            if (!string.IsNullOrWhiteSpace(ticket.IssueType)) sb.AppendLine($"- **Type:** {ticket.IssueType}");
            if (!string.IsNullOrWhiteSpace(ticket.Status)) sb.AppendLine($"- **Status:** {ticket.Status}");
            if (ticket.Labels.Count > 0) sb.AppendLine($"- **Labels:** {string.Join(", ", ticket.Labels)}");
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(ticket.DescriptionMarkdown))
            {
                sb.AppendLine("### Description");
                sb.AppendLine(ticket.DescriptionMarkdown.Trim());
                sb.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(ticket.AcceptanceCriteria))
            {
                sb.AppendLine("### Acceptance Criteria");
                sb.AppendLine(ticket.AcceptanceCriteria.Trim());
                sb.AppendLine();
            }

            if (ticket.Comments.Count > 0)
            {
                sb.AppendLine("### Discussion");
                foreach (var c in ticket.Comments)
                    sb.AppendLine($"**{c.Author}** ({c.Created.LocalDateTime:yyyy-MM-dd HH:mm}):  \n{c.BodyMarkdown.Trim()}\n");
                sb.AppendLine();
            }
        }
        else
        {
            sb.AppendLine("_No ticket fetched — paste the relevant details here._");
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(composedContext))
        {
            sb.AppendLine("## Context");
            sb.AppendLine(composedContext.Trim());
            sb.AppendLine();
        }

        var linked = materials.Where(m => m.LinkedInSpec).ToList();
        if (linked.Count > 0)
        {
            sb.AppendLine("## Materials");
            foreach (var m in linked)
            {
                var desc = string.IsNullOrWhiteSpace(m.Description) ? string.Empty : $" — {m.Description}";
                sb.AppendLine($"- **{m.Type}** [{m.Label}]({m.PathOrUrl}){desc}");
            }
            sb.AppendLine();
        }

        var selected = rules.Where(r => r.DefaultSelected).ToList();
        if (selected.Count > 0)
        {
            sb.AppendLine("## Rules for the assistant");
            foreach (var r in selected)
                sb.AppendLine($"- {r.Text}");
            sb.AppendLine();
        }

        sb.AppendLine("## Workspace layout");
        sb.AppendLine("Files for this task live under the task folder below. Read from and write to these locations:");
        AppendWorkspaceLayout(sb, task);
        sb.AppendLine();

        sb.AppendLine("## What to do");
        sb.AppendLine("_Specify what should be done here — the concrete changes, deliverables and what \"done\" looks like for this task._");
        sb.AppendLine();

        return sb.ToString().TrimEnd() + Environment.NewLine;
    }

    public string BuildKickoffPrompt(WorkTask task, IReadOnlyList<RulePreset> selectedRules)
    {
        var spec = Path.Combine(task.FolderPath, TaskLayout.SpecFile);
        var progress = ImplementationFile(task, TaskLayout.ProgressFile);

        var sb = new StringBuilder();
        sb.AppendLine($"Please implement the feature specified in `{spec}`.");
        sb.AppendLine("The task folder is laid out as follows — read from and write to these locations:");
        AppendWorkspaceLayout(sb, task);
        sb.AppendLine();
        sb.AppendLine($"As you work, keep `{progress}` updated with your decisions, progress and anything I should");
        sb.AppendLine("know — I follow that file in the dev tool. Create it if it doesn't exist.");
        sb.AppendLine();
        AppendRules(sb, selectedRules);
        return sb.ToString().TrimEnd();
    }

    public string BuildSelfReviewPrompt(WorkTask task)
    {
        var output = Path.Combine(task.FolderPath, TaskLayout.SelfReviewFolder);
        return $"""
            Run a code review of the changes implemented for `{task.JiraKey} — {task.Title}`.
            Use the project's code-review skill if available. Focus on correctness, clean code,
            DRY, YAGNI and SOLID, and consistency with the existing codebase style and patterns.
            Write your findings to a markdown file under `{output}`.
            """;
    }

    public string BuildPastCommentsPrompt(WorkTask task, IReadOnlyList<PullRequestComment> humanComments)
    {
        var output = Path.Combine(task.FolderPath, TaskLayout.PastCommentsFolder);
        var sb = new StringBuilder();
        sb.AppendLine($"Below are human review comments from past closed pull requests for this project.");
        sb.AppendLine($"Check whether the current implementation of `{task.JiraKey}` repeats any mistake or");
        sb.AppendLine("ignores guidance these comments established. Only flag issues that genuinely apply to");
        sb.AppendLine($"how this codebase works today. Write your conclusions to a markdown file under `{output}`.");
        sb.AppendLine();
        AppendComments(sb, humanComments);
        return sb.ToString().TrimEnd();
    }

    public string BuildPrCommentsPrompt(WorkTask task, IReadOnlyList<PullRequestComment> comments)
    {
        var output = Path.Combine(task.FolderPath, TaskLayout.PrCommentsFolder);
        var sb = new StringBuilder();
        sb.AppendLine($"Below are review comments left on the pull request for `{task.JiraKey}`.");
        sb.AppendLine("For each comment decide whether it is worth implementing given how this codebase already");
        sb.AppendLine("does things. Summarize what to fix, and for anything you would push back on, draft a short");
        sb.AppendLine($"reply explaining why. Write the summary to a markdown file under `{output}`.");
        sb.AppendLine();
        AppendComments(sb, comments);
        return sb.ToString().TrimEnd();
    }

    private static string ImplementationFile(WorkTask task, string fileName) =>
        Path.Combine(task.FolderPath, TaskLayout.ImplementationFolder, fileName);

    /// <summary>The canonical task folder layout, so the LLM knows where materials, images and outputs live.</summary>
    private static void AppendWorkspaceLayout(StringBuilder sb, WorkTask task)
    {
        sb.AppendLine($"- `{task.FolderPath}` — this task's folder.");
        sb.AppendLine($"- `{Path.Combine(task.FolderPath, TaskLayout.SpecFile)}` — the spec for this task.");
        sb.AppendLine($"- `{Path.Combine(task.FolderPath, TaskLayout.ImagesFolder)}` — designs, screenshots and reference images.");
        sb.AppendLine($"- `{ImplementationFile(task, TaskLayout.ProgressFile)}` — your running progress/decision log (keep this updated).");
        sb.AppendLine($"- `{Path.Combine(task.FolderPath, TaskLayout.SelfReviewFolder)}` — write self-review findings here.");
        sb.AppendLine($"- `{Path.Combine(task.FolderPath, TaskLayout.PastCommentsFolder)}` — notes from checking against past PR comments.");
        sb.AppendLine($"- `{Path.Combine(task.FolderPath, TaskLayout.PrCommentsFolder)}` — responses to this PR's review comments.");
    }

    private static void AppendRules(StringBuilder sb, IReadOnlyList<RulePreset> rules)
    {
        var selected = rules.Where(r => r.DefaultSelected).ToList();
        if (selected.Count == 0)
            return;

        sb.AppendLine("Follow these rules:");
        foreach (var r in selected)
            sb.AppendLine($"- {r.Text}");
    }

    private static void AppendComments(StringBuilder sb, IReadOnlyList<PullRequestComment> comments)
    {
        foreach (var c in comments)
        {
            var location = string.IsNullOrWhiteSpace(c.FilePath) ? string.Empty : $" ({c.FilePath})";
            sb.AppendLine($"- PR #{c.PullRequestNumber} — {c.Author}{location}: {c.Body.Trim()}");
        }
    }
}
