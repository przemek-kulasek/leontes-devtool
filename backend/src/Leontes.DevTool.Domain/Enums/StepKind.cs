namespace Leontes.DevTool.Domain.Enums;

/// <summary>
/// The ordered stages of the task workflow. The UI presents these as a non-linear pane —
/// the user may visit them in any order. The numeric values fix the display order.
/// </summary>
public enum StepKind
{
    Intake = 0,
    Materials = 1,
    Rules = 2,
    Context = 3,
    SpecAndKickoff = 4,
    Implement = 5,
    SelfReview = 6,
    PastPrComments = 7,
    PrComments = 8,
}
