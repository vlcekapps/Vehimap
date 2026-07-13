# Vehimap 2.0 Accessibility Checklist

Vehimap is accessibility-oriented and screen-reader-first, but it is not yet a formal
accessibility conformance product. This document is the working checklist that keeps
the Avalonia UI aligned with the official accessibility model while we collect evidence
for a future ACR/VPAT-style report if one is needed.

## Conformance status

- Current status: accessibility-oriented / pre-conformance.
- ACR-ready evidence work has started in `dotnet/docs/accessibility/`. The current
  target format is a VPAT 2.5Rev INT draft with supporting WCAG2ICT/2.2 AA matrix,
  remediation backlog and manual test protocol.
- Primary validation target: Windows 11 with NVDA, with Narrator as a secondary check.
- Future validation targets: macOS with VoiceOver and Linux with Orca after Windows 2.0
  storage and UI stabilization.
- We do not claim formal WCAG, EN 301 549, Section 508, or VPAT conformance yet.
- Any customer-facing ACR must remain a draft until the manual assistive-technology
  evidence pass is complete and reviewed.
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
- Destructive, data-importing or data-replacing actions must use
  `AutomationProperties.HelpText` to describe the consequence in user language.
  Examples: deleting a vehicle or record, restoring from backup, importing a vehicle
  package into the current data set, or installing an update that will replace the
  running app.
- If one button can perform different actions depending on runtime state, bind its
  help text to the same decision as the visible action. For example, the update-check
  primary action must distinguish opening release notes, downloading a package and
  launching the installer that closes/replaces the running app.
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
- For controls whose visible `Content` or `Header` is localized separately from
  `AutomationProperties.Name`, keep the visible label text inside the accessible name
  whenever practical. This protects speech input and WCAG 2.2 `Label in Name`. The
  main workspace `Open window` / `V okně` buttons have an EN/CS guard because their
  accessible names add the target workspace.
- Windows installer shortcuts must include localized `Comment` metadata in the Inno
  template so icon-only Start menu and desktop surfaces have a text alternative outside
  the running UI. Keep this guarded alongside installer smoke tests.
- Non-interactive but important controls such as `ProgressBar` still need a stable
  `AutomationId`, a human accessible name and short `HelpText` describing what is being
  measured.
- Visible shell context text that orients the user, such as the main window subtitle,
  must expose the same bound text through `AutomationProperties.Name` with a stable
  `AutomationId` so it remains available in UI Automation structure checks.
- Standalone workspace windows must expose their header summary texts through
  `AutomationProperties.Name` and window-scoped `AutomationId` values; use IDs that
  do not collide with the embedded workspace view hosted in the same window.
- Dialog header context that identifies the affected item, selected plan or current
  status must expose the same text through `AutomationProperties.Name` with stable
  `AutomationId` values before the editable form controls.
- Visible form guidance, warning or instruction text must also expose a human
  accessible name and stable `AutomationId`; do not rely on styled text alone for
  orienting screen-reader users.
- Meaningful list item templates whose root exposes
  `AutomationProperties.Name="{Binding AccessibleLabel}"` must also expose
  `AutomationProperties.ItemType`, for example vehicle, document, audit item or fuel
  warning. Use `AutomationProperties.ItemStatus` only for a real status, priority or
  availability value; never use it as a duplicate title, summary or detail text.
- Use `AutomationProperties.LiveSetting` for status changes that should be announced:
  validation errors, save results, import/restore results, update progress and shell
  status. Use `Polite` for routine progress and save/status messages; reserve
  `Assertive` for validation errors, load failures and other blocking errors.
- Data-changing or data-replacing actions must expose consequence-oriented
  `AutomationProperties.HelpText`, not only short button/menu labels. The static
  guard covers restore/import/delete/update actions plus managed-attachment
  conversion and vehicle starter-bundle apply actions.
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
- Top-level windows must explicitly declare `CanResize`. Standard shell, workspace,
  service-book, settings and editor windows use `CanResize="True"` so high-DPI and
  large-font users can recover clipped content. Only short transient surfaces such as
  notifications or update progress may use `CanResize="False"`, and those exceptions
  must stay covered by tests and manual evidence.
- Prefer modal dialog editors for complex forms that can otherwise mix with unrelated
  host-window actions. Vehicle, history, fuel, reminder, maintenance and record editors
  now use this pattern. A dialog editor must have one primary heading, first focus on
  the first logical field, `Ctrl+S` for save, `Escape`/`Zrusit` for discard, a live
  status region and an explicit return-focus target chosen by the workflow that opened
  it. Workspace cards must stay overview surfaces with lists, details and actions, not
  inline form hosts.
- The editor dialog tab order must stay standard. The only intentional boundary override
  is `Shift+Tab` from the first logical field to the `Zrusit` button; from every other
  field, `Shift+Tab` moves exactly one previous control in normal tab order.
- Text fields must stay standard Avalonia `TextBox` controls. Until Avalonia exposes
  enough native UIA text/caret information for our NVDA target, see
  [AvaloniaUI/Avalonia#9770](https://github.com/AvaloniaUI/Avalonia/issues/9770), the
  desktop shell may add a tested live-region fallback that announces field name, caret
  position and nearby characters after keyboard navigation. This is a temporary
  `TextBox UIA text fallback`, not a replacement for standard controls. While present,
  its EN/CS selection counts must use the shared pluralization service and its caret,
  selection and snippet boundaries must use Unicode text elements so emoji and combining
  sequences are not announced as split UTF-16 code units.
- Prefer Avalonia `HotKey`, `KeyBinding` and commands. A manual `KeyDown` handler is an
  exception, not the default.
- Interactive controls with explicit fixed `Width`, `Height`, `MinWidth` or
  `MinHeight` must not drop below 24 px. Action panels that size button slots through
  `WrapPanel.ItemWidth` or `WrapPanel.ItemHeight` must follow the same floor. The guard
  test intentionally treats this as a minimum target-size floor; manual pointer testing
  is still required before a customer-facing ACR.
- Custom visual styles that override standard controls must keep an explicit visible
  focus state. The app-level `RadioButton.tab-header:focus` and
  `RadioButton.tab-header:checked:focus` styles provide the workspace tab strip with a
  stable 2 px focus border and higher-contrast focused background in the standard theme.
  The automated accessibility guard checks that the focus border keeps at least 3:1
  non-text contrast against the focused background; do not remove the styles unless a
  manually verified replacement covers standard, forced-colors and high-DPI modes.
- Do not encode critical state only with color, icon shape, visual position or tooltip.

## Documented keyboard/focus exceptions

These exceptions exist because they protect observed NVDA/Appium behavior in the current
Avalonia shell. New entries require a regression test.

- `MainWindow.axaml.cs`: global `Alt`/`F10` menu open/close, return focus to the previous
  non-menu control and do not put menu roots in normal `Tab` order.
- `MainWindow.axaml.cs` and `App.axaml`: tab header keyboard behavior and visible focus
  styling for the custom radio-button card strip. Keep until a native `TabControl`
  prototype proves better with NVDA and forced-colors evidence.
- `MainWindow.axaml.cs`: boundary focus between vehicle filters, the vehicle list and
  selected workspace tab header.
- `WorkspaceViewBase.cs`: reverse tab boundary from embedded workspace content back to
  the selected shell tab header when a workspace is hosted inside the main window.
- `KeyboardAccessibilityHelper.cs`: let text boxes keep standard cursor/editing keys,
  let combo boxes open with plain up/down arrows and provide the temporary `TextBox UIA
  text fallback` live region for caret context. The helper is registered on every
  top-level window that has keyboard shortcuts and resolves controls through logical,
  templated and visual tree parents before a global shortcut can handle the key.
- `EditorDialogFocusHelpers.cs`: shared dialog-editor lifecycle and boundary behavior
  for first focus, `Ctrl+S` save, `Escape` discard and `Shift+Tab` only from the first
  logical field to the cancel button. This exists so all evidence editors can leave
  read-only overview screens without losing the predictable keyboard loop; new editor
  dialogs should reuse the helper instead of adding one-off tab traps.
- `AvaloniaTrayService.cs`: native Windows notification-area context menus are exposed
  by Avalonia as `TrayIcon` + `NativeMenu`, not as normal Avalonia controls. In NVDA
  testing the menu can open without announcing itself until the user presses an arrow
  key, so Vehimap does not treat the native tray menu as a screen-reader-first path.
  The accessible path is `Aplikace -> Akce na liste` or `Ctrl+Shift+Y`, which opens
  the standard Avalonia `TrayActionsWindow`. Keep the native tray menu short and do not
  add accessibility-only items inside it, because they are discovered only after the
  problematic menu-open step.
- `ModalWorkspaceWindowHelpers.cs` and app-level dialogs: `Escape` closes modal windows
  only when it is safe for the current workflow.
- `VehicleStarterBundleWindow.axaml.cs`: list keyboard shortcuts for selecting/clearing
  bundle items.
- `ServiceBookWindow.axaml.cs`: modal service-book keyboard commands that mirror the
  visible buttons.

## Android and TalkBack framework status

Last verified package: `Avalonia 12.0.4`.

The first Android nightly uses standard Avalonia controls and exposes accessible names,
actions and selection state through Avalonia automation peers. On the tested Android 16
device, TalkBack reads the control name and the double-tap action hint, but does not
always announce the standard role. For example, the reload action is a real Avalonia
`Button` with a `ButtonAutomationPeer`, yet TalkBack does not say "button".

This is tracked as an Avalonia Android backend limitation, not as a reason to append
localized words such as "button" to visible labels, accessible names or help text.
Avalonia 12.0.4 populates Android `AccessibilityNodeInfo.ClassName` from
`peer.GetClassName()` but does not map `peer.GetAutomationControlType()` to a native
Android role. Source inspection confirmed that Avalonia 12.1.0 still uses the same
class-name path and does not yet provide the missing role mapping. Vehimap therefore
must not add application-specific automation peers or Android class-name overrides just
to mask this framework behavior.

The compact shell keeps four persistent text-labelled destinations (`Home`, `Vehicles`,
`Alerts`, `More`) with at least 48 device-independent pixels of touch height. Vehicle
evidence is reached through a separate vehicle hub instead of a desktop tab strip.
Inline mobile editors are prohibited: every future create/edit workflow must be a
dedicated full-screen or modal route with explicit Save, Cancel and Android Back behavior.
System Back is handled through Avalonia `TopLevel.BackRequested`, not a Vehimap-specific
Android dispatcher callback. A physical Android 16 smoke confirmed that three-button Back
returns `Vehicles` to `Home`, while Back from `Home` exits; gesture navigation must use the
same route contract.
The complete mobile interaction contract is in `MOBILE.md`.

Framework source references:

- [Avalonia 12.0.4 Android accessibility bridge](https://github.com/AvaloniaUI/Avalonia/blob/12.0.4/src/Android/Avalonia.Android/AvaloniaAccessHelper.cs#L195)
- [Avalonia 12.1.0 Android accessibility bridge](https://github.com/AvaloniaUI/Avalonia/blob/12.1.0/src/Android/Avalonia.Android/AvaloniaAccessHelper.cs#L234)

Every Avalonia package upgrade must repeat the Android TalkBack role smoke before the
version is accepted:

- install the current Android nightly on a physical device with TalkBack enabled;
- verify that buttons, selectable vehicle cards/lists and future tab controls announce
  their role, name, state and available action;
- inspect the Android accessibility tree when speech and control semantics disagree;
- update the verified package version above and the Android evidence log;
- retire this exception only after the standard Avalonia controls expose correct roles
  without Vehimap-specific label or peer workarounds.

This limitation prevents an Android `Supports` claim for affected name/role/value
requirements. It does not change the Windows evidence status, and it is not addressed
by adding the control type to user-visible text.

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

ACR-ready planning documents live in `dotnet/docs/accessibility/`:

- `README.md` describes the evidence set and its non-certification status.
- `acr-vpat-int-draft.md` is the working VPAT 2.5Rev INT draft.
- `wcag2ict-22-aa-matrix.md` tracks WCAG2ICT / WCAG 2.2 AA criteria for non-web software.
- `a11y-remediation-backlog.md` keeps known gaps and temporary exceptions visible.
- `manual-test-protocol.md` defines the repeatable Windows NVDA/Narrator and keyboard-only pass.

## Official references

- [Avalonia accessibility](https://docs.avaloniaui.net/docs/app-development/accessibility)
- [Avalonia focus](https://docs.avaloniaui.net/docs/input-interaction/focus)
- [Avalonia keyboard and hotkeys](https://docs.avaloniaui.net/docs/input-interaction/keyboard-and-hotkeys)
- [Avalonia TrayIcon](https://docs.avaloniaui.net/controls/navigation/trayicon/)
- [Avalonia Android platform guide](https://docs.avaloniaui.net/docs/platform-specific-guides/android)
- [Android custom view accessibility](https://developer.android.com/guide/topics/ui/accessibility/views/custom-views)
- [Avalonia Linux platform guide](https://docs.avaloniaui.net/docs/platform-specific-guides/linux)
