// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Markup.Xaml;

namespace Vehimap.Desktop.Localization;

public sealed class LocExtension : MarkupExtension
{
    public LocExtension()
    {
    }

    public LocExtension(string key)
    {
        Key = key;
    }

    public string Key { get; set; } = string.Empty;

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        DesktopLocalization.GetString(Key);
}
