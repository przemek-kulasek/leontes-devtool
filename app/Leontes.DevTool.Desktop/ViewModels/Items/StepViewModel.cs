using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Leontes.DevTool.Domain.Enums;

namespace Leontes.DevTool.Desktop.ViewModels.Items;

public sealed partial class StepViewModel : ObservableObject
{
    public required Guid Id { get; init; }
    public required StepKind Kind { get; init; }
    public string? SuggestedModel { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusBadge))]
    private StepStatus _status;

    public string Number => ((int)Kind + 1).ToString();

    public string Title => Kind switch
    {
        StepKind.Intake => "Intake",
        StepKind.Materials => "Materials",
        StepKind.Rules => "Rules",
        StepKind.Context => "Context",
        StepKind.SpecAndKickoff => "Spec & Kickoff",
        StepKind.Implement => "Implement",
        StepKind.SelfReview => "Self review",
        StepKind.PastPrComments => "Past PR comments",
        StepKind.PrComments => "PR comments",
        _ => Kind.ToString(),
    };

    public string StatusBadge => Status switch
    {
        StepStatus.NotStarted => "○",
        StepStatus.InProgress => "◐",
        StepStatus.Done => "●",
        StepStatus.Skipped => "—",
        _ => "○",
    };
}
