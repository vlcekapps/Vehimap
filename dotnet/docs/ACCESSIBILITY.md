# Vehimap 2.0 Accessibility Checklist

Vehimap is accessibility-oriented and screen-reader-first, but it is not yet a formal
accessibility conformance product. This document is the working checklist that keeps
the Avalonia UI aligned with the official accessibility model while we collect evidence
for a future ACR/VPAT-style report if one is needed.

## Conformance status

- Current status: accessibility-oriented / pre-conformance.
- Primary validation target: Windows 11 with NVDA, with Narrator as a secondary check.
- Future validation targets: macOS with VoiceOver and Linux with Orca after Windows 2.0
  storage and UI stabilization.
- We do not claim formal WCAG, EN 301 549, Section 508, or VPAT conformance yet.
- Known exceptions must stay documented, tested, and either retired or explicitly kept.

## Avalonia rules for new UI

- Prefer standard Avalonia controls before custom controls. If a custom `Control` or
  `TemplatedControl` becomes necessary, it must have an automation peer strategy before
  it ships.
- Every interactive control must have a stable `AutomationProperties.AutomationId`.
- Every interactive control must have a human accessible name through visible content
  or `AutomationProperties.Name`. `AutomationProperties.LabeledBy` is allowed for
  targeted experiments, but it is not the mandatory baseline yet because the local
  Avalonia 12.0.4 UIA documentation marks `LabeledByPropertyId` as not implemented.
- For label + field forms, keep the visible label and accessible name synchronized. Use
  `LabeledBy` where it is stable in our target Avalonia version; otherwise keep the
  explicit `Name` and note the reason in tests or comments.
- Use `AutomationProperties.HelpText` only for extra instructions, never as the only
  label for a field.
- If a menu item exposes a visible `InputGesture`, it must also expose the same shortcut
  through `AutomationProperties.AcceleratorKey` so assistive technologies can announce
  the accelerator consistently.
- Non-interactive but important controls such as `ProgressBar` still need a stable
  `AutomationId`, a human accessible name and short `HelpText` describing what is being
  measured.
- Meaningful list item templates whose root exposes
  `AutomationProperties.Name="{Binding AccessibleLabel}"` must also expose
  `AutomationProperties.ItemType`, for example vehicle, document, audit item or fuel
  warning. Use `AutomationProperties.ItemStatus` only for a real status, priority or
  availability value; never use it as a duplicate title, summary or detail text.
- Use `AutomationProperties.LiveSetting` for status changes that should be announced:
  validation errors, save results, import/restore results, update progress and shell
  status. Use `Polite` for routine progress and save/status messages; reserve
  `Assertive` for validation errors, load failures and other blocking errors.
- Every top-level window or modal dialog must expose exactly one primary heading with
  `AutomationProperties.HeadingLevel="1"`, a stable `AutomationId` and a human
  accessible name. Long dialogs and dense workspaces may use `HeadingLevel="2"` for
  visible section headings.
- Use `AutomationProperties.LandmarkType` conservatively for the main shell,
  navigation, search areas and primary content. Every landmark must also set
  `AutomationProperties.AccessibilityView="Control"` so it is exposed reliably through
  UI Automation.
- Keyboard access must work without a mouse. `Tab` moves forward, `Shift+Tab` moves
  backward, `Alt`/`F10` opens and closes the main menu, and form text boxes keep normal
  editing navigation.
- Prefer Avalonia `HotKey`, `KeyBinding` and commands. A manual `KeyDown` handler is an
  exception, not the default.
- Do not encode critical state only with color, icon shape, visual position or tooltip.

## Documented keyboard/focus exceptions

These exceptions exist because they protect observed NVDA/Appium behavior in the current
Avalonia shell. New entries require a regression test.

- `MainWindow.axaml.cs`: global `Alt`/`F10` menu open/close, return focus to the previous
  non-menu control and do not put menu roots in normal `Tab` order.
- `MainWindow.axaml.cs`: tab header keyboard behavior for the custom radio-button card
  strip. Keep until a native `TabControl` prototype proves better with NVDA.
- `MainWindow.axaml.cs`: boundary focus between vehicle filters, the vehicle list and
  selected workspace tab header.
- `WorkspaceViewBase.cs`: reverse tab boundary from embedded workspace content back to
  the selected shell tab header when a workspace is hosted inside the main window.
- `KeyboardAccessibilityHelper.cs`: let text boxes keep standard cursor/editing keys and
  let combo boxes open with plain up/down arrows.
- `ModalWorkspaceWindowHelpers.cs` and app-level dialogs: `Escape` closes modal windows
  only when it is safe for the current workflow.
- `VehicleStarterBundleWindow.axaml.cs`: list keyboard shortcuts for selecting/clearing
  bundle items.
- `ServiceBookWindow.axaml.cs`: modal service-book keyboard commands that mirror the
  visible buttons.

## Evidence log

Manual evidence lives in `dotnet/docs/accessibility-evidence/`. Each run should record:

- date, build, release channel and commit;
- screen reader and operating system;
- scenarios tested;
- pass/fail result;
- known issues or temporary exceptions.

## Official references

- [Avalonia accessibility](https://docs.avaloniaui.net/docs/app-development/accessibility)
- [Avalonia focus](https://docs.avaloniaui.net/docs/input-interaction/focus)
- [Avalonia keyboard and hotkeys](https://docs.avaloniaui.net/docs/input-interaction/keyboard-and-hotkeys)
- [Avalonia Linux platform guide](https://docs.avaloniaui.net/docs/platform-specific-guides/linux)
