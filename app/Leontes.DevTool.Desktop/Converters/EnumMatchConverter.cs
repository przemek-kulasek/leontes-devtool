using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Leontes.DevTool.Desktop.Converters;

/// <summary>True when the bound value's name equals the converter parameter. Used to show the panel for the selected step.</summary>
public sealed class EnumMatchConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not null && parameter is not null
        && string.Equals(value.ToString(), parameter.ToString(), StringComparison.Ordinal);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
