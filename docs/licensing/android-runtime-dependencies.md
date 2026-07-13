# Android runtime dependency notes

Generated: **2026-07-13**

Command used:

```text
dotnet list dotnet/Vehimap.Android.sln package --include-transitive
```

The experimental Android APK contains the shared Vehimap SQLite and Avalonia stack already listed in `THIRD-PARTY-NOTICES.md`, plus these Android-specific runtime families:

| Family | Observed packages | License from NuGet metadata |
|---|---|---|
| Avalonia Android | `Avalonia.Android` 12.0.4 | MIT |
| Android native rendering | `HarfBuzzSharp.NativeAssets.Android` 8.3.1.3, `SkiaSharp.NativeAssets.Android` 3.119.4 | MIT |
| AndroidX bindings | `Xamarin.AndroidX.*`, including Activity, AppCompat, Core, Fragment, Lifecycle, SavedState, SplashScreen and Window | MIT AND Apache-2.0 |
| Kotlin bindings | `Xamarin.Kotlin.StdLib`, `Xamarin.KotlinX.Coroutines.*`, `Xamarin.KotlinX.Serialization.*` | MIT AND Apache-2.0 |
| Supporting bindings | `Xamarin.Google.Guava.ListenableFuture`, `Xamarin.Jetbrains.Annotations`, `Xamarin.JSpecify` | MIT AND Apache-2.0 |

`Microsoft.NET.ILLink.Tasks` is an automatically referenced build/linker package. It is part of the Android build toolchain rather than an independently exposed application feature. The APK includes the resulting trimmed/AOT runtime payload and the .NET runtime notices covered by the main third-party notice.

The Android readiness gate inspects the final APK for both target ABI directories and bundled legal assets. Re-run the package command and update this note whenever the Android target framework, Avalonia Android, AndroidX or Kotlin dependency graph changes.
