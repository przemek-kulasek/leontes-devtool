using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Leontes.DevTool.Domain.Enums;

namespace Leontes.DevTool.Desktop.Converters;

/// <summary>Maps a step status to its dot colour (green done, blue in-progress, grey otherwise).</summary>
public sealed class StepStatusToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush Done = new(Color.Parse("#35D29A"));
    private static readonly SolidColorBrush InProgress = new(Color.Parse("#3D7BFF"));
    private static readonly SolidColorBrush Idle = new(Color.Parse("#3A465F"));
    private static readonly SolidColorBrush Skipped = new(Color.Parse("#6B7794"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        StepStatus.Done => Done,
        StepStatus.InProgress => InProgress,
        StepStatus.Skipped => Skipped,
        _ => Idle,
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
