using System.Text;
using System.Text.Json;

namespace Leontes.DevTool.Infrastructure.Jira;

/// <summary>
/// Minimal converter from Atlassian Document Format (the shape Jira Cloud returns for rich text) to
/// Markdown. Handles the common block and inline node types; unknown nodes degrade to their text.
/// </summary>
public static class AdfToMarkdown
{
    public static string Convert(JsonElement doc)
    {
        if (doc.ValueKind != JsonValueKind.Object)
            return string.Empty;

        var sb = new StringBuilder();
        AppendChildren(sb, doc);
        return sb.ToString().Trim();
    }

    private static void AppendChildren(StringBuilder sb, JsonElement node)
    {
        if (!node.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            return;

        foreach (var child in content.EnumerateArray())
            AppendBlock(sb, child);
    }

    private static void AppendBlock(StringBuilder sb, JsonElement node)
    {
        switch (TypeOf(node))
        {
            case "paragraph":
                sb.AppendLine(Inline(node)).AppendLine();
                break;
            case "heading":
                var level = node.TryGetProperty("attrs", out var a) && a.TryGetProperty("level", out var l) ? l.GetInt32() : 1;
                sb.AppendLine($"{new string('#', Math.Clamp(level, 1, 6))} {Inline(node)}").AppendLine();
                break;
            case "bulletList":
                AppendList(sb, node, ordered: false);
                sb.AppendLine();
                break;
            case "orderedList":
                AppendList(sb, node, ordered: true);
                sb.AppendLine();
                break;
            case "codeBlock":
                sb.AppendLine("```").AppendLine(Inline(node)).AppendLine("```").AppendLine();
                break;
            case "blockquote":
                foreach (var line in Block(node).Split('\n'))
                    sb.AppendLine($"> {line}");
                sb.AppendLine();
                break;
            case "rule":
                sb.AppendLine("---").AppendLine();
                break;
            default:
                var text = Inline(node);
                if (!string.IsNullOrWhiteSpace(text))
                    sb.AppendLine(text).AppendLine();
                break;
        }
    }

    private static void AppendList(StringBuilder sb, JsonElement node, bool ordered)
    {
        if (!node.TryGetProperty("content", out var items) || items.ValueKind != JsonValueKind.Array)
            return;

        var index = 1;
        foreach (var item in items.EnumerateArray())
        {
            var marker = ordered ? $"{index++}." : "-";
            sb.AppendLine($"{marker} {Block(item).Replace("\n", " ").Trim()}");
        }
    }

    private static string Block(JsonElement node)
    {
        var sb = new StringBuilder();
        AppendChildren(sb, node);
        return sb.ToString().Trim();
    }

    private static string Inline(JsonElement node)
    {
        if (!node.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var child in content.EnumerateArray())
        {
            switch (TypeOf(child))
            {
                case "text":
                    sb.Append(ApplyMarks(child));
                    break;
                case "hardBreak":
                    sb.Append("  \n");
                    break;
                default:
                    sb.Append(Inline(child));
                    break;
            }
        }

        return sb.ToString();
    }

    private static string ApplyMarks(JsonElement textNode)
    {
        var text = textNode.TryGetProperty("text", out var t) ? t.GetString() ?? string.Empty : string.Empty;
        if (!textNode.TryGetProperty("marks", out var marks) || marks.ValueKind != JsonValueKind.Array)
            return text;

        foreach (var mark in marks.EnumerateArray())
        {
            text = TypeOf(mark) switch
            {
                "strong" => $"**{text}**",
                "em" => $"*{text}*",
                "code" => $"`{text}`",
                "strike" => $"~~{text}~~",
                "link" when mark.TryGetProperty("attrs", out var attrs) && attrs.TryGetProperty("href", out var href)
                    => $"[{text}]({href.GetString()})",
                _ => text,
            };
        }

        return text;
    }

    private static string TypeOf(JsonElement node) =>
        node.TryGetProperty("type", out var type) ? type.GetString() ?? string.Empty : string.Empty;
}
