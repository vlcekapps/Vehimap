<!-- SPDX-License-Identifier: GPL-3.0-or-later -->

# Vehimap Accessibility Remediation Backlog

This backlog tracks items that can affect a future VPAT INT / ACR statement.
Items here are not all release blockers for nightly builds, but they must be
reviewed before a customer-facing ACR is published.

| Area | Related criteria | Current risk | Next action |
|---|---|---|---|
| TextBox UIA text fallback | 2.1.1, 2.1.2, 4.1.2 | Vehimap uses a temporary live-region fallback for text editing because the tested Avalonia/NVDA path does not expose enough caret context. | Track AvaloniaUI/Avalonia#9770, retest after Avalonia upgrade, remove fallback when native UIA text behavior is sufficient. |
| Native tray menu | 1.4.13, 2.1.1, 4.1.2 | Avalonia `TrayIcon` native menu may open without screen-reader announcement until the user presses an arrow key. | Keep native tray menu short; support `Aplikace -> Akce na liště` / `Ctrl+Shift+Y` as the accessible path. |
| High DPI and 400% scaling | 1.4.4, 1.4.10, 2.4.11 | Main windows are resizable and editors are dialog-based, but formal large-font/400% evidence is not complete. | Run manual scaling pass across shell, dashboard, audit, documents, settings and all editors. |
| Forced colors / high contrast | 1.4.3, 1.4.11, 2.4.7 | The custom workspace tab strip now has a guarded standard-theme focus border/background with an automated 3:1 contrast check, but visual contrast evidence is not complete for every state and platform theme. | Capture manual Windows forced-colors pass and fix any hidden focus/status regressions. |
| Pointer target size | 2.5.8 | Fixed desktop XAML dimensions now have a 24 px static guard and workspace tab headers have explicit 34 px target height, but formal pointer target measurements are not complete. | Run manual pointer target pass across shell filters, workspace action panels, dialogs and generated installer shortcuts before publishing an ACR. |
| Dialog invalid states | 3.3.1, 3.3.3, 4.1.3 | Dialog editors have validation and live status, but every invalid field needs manual screen-reader evidence. | Execute manual protocol invalid-state scenarios for vehicle, history, fuel, reminder, maintenance and document editors. |
| Data-replacing actions | 3.3.4 | Backup restore, vehicle-package import, update install and other import workflows are guarded for accessible consequence text where practical, including context-specific help text for the update dialog primary action, but the ACR needs explicit manual evidence. | Verify review text, confirmation, cancel path and status messages for restore/update/import/package actions. |
| Future Android shell | Multiple | Android is not evaluated yet and may expose different accessibility APIs and focus behavior. | Do not start public Android accessibility claims until Windows i18n/accessibility gates are green. |
