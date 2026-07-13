// SPDX-License-Identifier: GPL-3.0-or-later
using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace Vehimap.Android;

[Activity(
    Theme = "@style/VehimapTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    Exported = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public sealed class MainActivity : AvaloniaMainActivity
{
}
