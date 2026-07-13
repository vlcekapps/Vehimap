// SPDX-License-Identifier: GPL-3.0-or-later
using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Vehimap.Mobile;

namespace Vehimap.Android;

[Application]
public sealed class VehimapAndroidApplication : AvaloniaAndroidApplication<MobileApp>
{
    public VehimapAndroidApplication(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).LogToTrace();
}
