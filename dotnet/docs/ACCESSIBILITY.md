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
- The current `TextBox UIA text fallback` is an explicitly temporary workaround for
  [AvaloniaUI/Avalonia#9770](https://github.com/AvaloniaUI/Avalonia/issues/9770). It
  improves practical NVDA usability in nightly builds, but it is not acceptable as a
  final answer for a formal ACR/VPAT claim.

## Avalonia rules for new UI

- Prefer standard Avalonia controls before custom controls. If a custom `Control` or
  `TemplatedControl` becomes necessary, it must have an automation peer strategy before
  it ships.
- Every interactive control must have a stable `AutomationProperties.AutomationId`,
  including menu items and radio-button based navigation controls.
- Every interactive control must have a human accessible name through visible content
  or `AutomationProperties.Name`. `AutomationProperties.LabeledBy` is allowed for
  targeted experiments, but it is not the mandatory baseline yet because the local
  Avalonia 12.0.4 UIA documentation marks `LabeledByPropertyId` as not implemented.
- Any `TextBlock` with an `AutomationProperties.AutomationId` is considered important
  content or diagnostics and must also expose an explicit `AutomationProperties.Name`.
  Do not rely on implicit text extraction for support-oriented summaries or details.
- Any `SelectableTextBlock` with an `AutomationId` is considered a copyable value. Its
  accessible name must include both the user-facing label and the current value through
  a dedicated `*AccessibleName` binding.
- For label + field forms, keep the visible label and accessible name synchronized. Use
  `LabeledBy` where it is stable in our target Avalonia version; otherwise keep the
  explicit `Name` and note the reason in tests or comments.
- Fields that runtime validation truly rejects as empty must expose
  `AutomationProperties.IsRequiredForForm`. Use it only for real required fields or
  explicit conditional requirements, never for helpful-but-optional values.
- Use `AutomationProperties.HelpText` only for extra instructions, never as the only
  label for a field.
- Conditionally disabled controls must use `AutomationProperties.HelpText` to explain
  the prerequisite that enables them. A screen-reader user must not have to infer why a
  field is disabled from visual grouping alone.
- Destructive or data-replacing actions must use `AutomationProperties.HelpText` to
  describe the consequence in user language. Examples: deleting a vehicle or record,
  restoring from backup, or installing an update that will replace the running app.
- If a field uses `PlaceholderText` for an example value or filter hint, expose the
  same instruction through `AutomationProperties.HelpText`. Placeholder text is a
  visual hint, not a reliable accessible instruction once the field has focus or
  contains a value.
- `ComboBox` controls inherit a global `AutomationProperties.HelpText` that explains
  arrow-key opening and selection. Keep that global hint unless a specific combo box
  needs a more precise local instruction.
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
- Every heading must expose a stable `AutomationId` and a human accessible name. Every
  top-level window or modal dialog must expose exactly one primary heading with
  `AutomationProperties.HeadingLevel="1"`. Long dialogs and dense workspaces may use
  `HeadingLevel="2"` for visible section headings.
- Use `AutomationProperties.LandmarkType` conservatively for the main shell,
  navigation, search areas and primary content. Every landmark must also set
  `AutomationProperties.AccessibilityView="Control"` so it is exposed reliably through
  UI Automation.
- Keyboard access must work without a mouse. `Tab` moves forward, `Shift+Tab` moves
  backward, `Alt`/`F10` opens and closes the main menu, and form text boxes keep normal
  editing navigation.
- Prefer modal dialog editors for complex forms that can otherwise mix with unrelated
  host-window actions. A dialog editor must have one primary heading, first focus on the
  first logical field, `Escape`/`Zrusit` for discard, a live status region and an
  explicit return-focus target chosen by the workflow that opened it.
- Text fields must stay standard Avalonia `TextBox` controls. Until Avalonia exposes
  enough native UIA text/caret information for our NVDA target, see
  [AvaloniaUI/Avalonia#9770](https://github.com/AvaloniaUI/Avalonia/issues/9770), the
  desktop shell may add a tested live-region fallback that announces field name, caret
  position and nearby characters after keyboard navigation. This is a temporary
  `TextBox UIA text fallback`, not a replacement for standard controls.
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
- `KeyboardAccessibilityHelper.cs`: let text boxes keep standard cursor/editing keys,
  let combo boxes open with plain up/down arrows and provide the temporary `TextBox UIA
  text fallback` live region for caret context. The helper is registered on every
  top-level window that has keyboard shortcuts and resolves controls through logical,
  templated and visual tree parents before a global shortcut can handle the key.
- `EditorDialogFocusHelpers.cs`: shared dialog-editor boundary behavior for first
  focus, `Escape` discard and `Shift+Tab` from the first field to the cancel button.
  This exists so vehicle editing can leave the read-only detail screen without losing
  the predictable keyboard loop; future editor dialogs should reuse the helper instead
  of adding one-off tab traps.
- `VehicleEditorWindow.axaml.cs`: `Ctrl+S` save shortcut and validation-aware close
  behavior for the first migrated dialog editor. It keeps invalid forms open and lets
  the host restore focus to the origin after successful save or cancel.
- `ModalWorkspaceWindowHelpers.cs` and app-level dialogs: `Escape` closes modal windows
  only when it is safe for the current workflow.
- `VehicleStarterBundleWindow.axaml.cs`: list keyboard shortcuts for selecting/clearing
  bundle items.
- `ServiceBookWindow.axaml.cs`: modal service-book keyboard commands that mirror the
  visible buttons.

## Temporary TextBox fallback retirement

The `KeyboardAccessibilityHelper.cs` live-region fallback for text editing exists only
because Avalonia currently does not expose enough native UIA caret/text navigation
information for screen readers in our tested scenario:
[AvaloniaUI/Avalonia#9770](https://github.com/AvaloniaUI/Avalonia/issues/9770).

Retire this fallback when all of the following are true:

- The upstream Avalonia issue is closed or otherwise confirmed fixed for the desktop UIA
  path.
- Vehimap has upgraded to an Avalonia version that contains the fix.
- Manual NVDA testing confirms that standard `TextBox` cursor navigation announces
  characters, words, selection and caret context without the Vehimap live region.
- The Appium accessibility regressions still pass after removing the fallback.

Retirement work must remove the live-region code from `KeyboardAccessibilityHelper.cs`,
remove or rewrite tests that assert `TextEditingLiveRegion`, and update this document
from `accessibility-oriented / pre-conformance` toward the then-current conformance
position. Until that happens, the fallback remains a documented exception, not a
conformance strategy.

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
