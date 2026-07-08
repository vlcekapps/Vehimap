<!-- SPDX-License-Identifier: GPL-3.0-or-later -->

# Vehimap WCAG2ICT 2.2 AA Matrix

Status: draft baseline for Vehimap 2.0. This matrix maps product evidence to
WCAG 2.2 A/AA using WCAG2ICT guidance for non-web software. It is not a legal
certification or a substitute for independent audit.

## Methodology

- Scope: Windows desktop nightly, installer/update flow, main shell, workspace
  views, modal editors, Settings, About, backup/restore, health diagnostics,
  tray alternatives, notifications and generated reports.
- Out of scope for this draft: future Android UI, macOS/Linux validation and
  third-party screen-reader bugs outside documented workarounds.
- User-entered data is evaluated as content preserved by the product, not as UI
  text authored by Vehimap.
- Status values use the ACR terms `Supports`, `Partially Supports`, `Does Not
  Support` and `Not Applicable`.

## Representative Test Set

- Fresh install, update install and migrated 1.x data set.
- Empty data set and real multi-vehicle data set.
- Main shell: vehicle list, filters, tabs, menu, quick actions and tray actions
  window.
- Workspaces: detail, history, fuel, reminders, maintenance, documents, audit,
  timeline, costs, dashboard, global search, smart advisor and service book.
- Dialogs: all evidence editors, Settings, About, update progress, data health,
  backup/restore confirmations and vehicle starter bundle.
- Outputs: notifications, tray tooltip/status, calendar export, cost reports,
  printable vehicle report, service book HTML and vehicle packages.

## Draft Matrix

| Criterion | Level | Draft status | Vehimap relation | Evidence / next step |
|---|---:|---|---|---|
| 1.1.1 Non-text Content | A | Partially Supports | Desktop UI is text-first and does not rely on images for critical controls. Windows installer shortcuts now include localized `Comment` metadata for icon-only Start menu and desktop surfaces. | Manual screen-reader pass for icon-only platform surfaces and generated installer shortcuts. |
| 1.2.1-1.2.5 Time-based Media | A/AA | Not Applicable | Vehimap does not create audio/video media. | Reassess if help videos or media attachments become rendered content. |
| 1.3.1 Info and Relationships | A | Partially Supports | Standard Avalonia controls, headings, shell context text, item types/statuses, landmarks and accessible names are guarded. | Continue static UIA guard tests and manual NVDA verification of dense dialogs. |
| 1.3.2 Meaningful Sequence | A | Partially Supports | Dialog editors and workspaces have intentional tab order; main shell focus order is regression-tested. | Manual pass across every tab after localization changes. |
| 1.3.3 Sensory Characteristics | A | Supports | Critical instructions are text-backed and not dependent on color, position or shape alone. | Keep guarded when new visual summaries are added. |
| 1.3.4 Orientation | AA | Supports | Desktop UI does not force orientation. | Reassess for Android shell. |
| 1.3.5 Identify Input Purpose | AA | Partially Supports | Native desktop fields expose names/help text, but HTML autocomplete tokens do not apply directly. | For ACR, explain platform difference and manually verify common personal-data fields. |
| 1.4.1 Use of Color | A | Supports | Statuses and warnings use text labels, not only color. | Manual high-contrast pass remains required. |
| 1.4.2 Audio Control | A | Not Applicable | Vehimap does not autoplay audio. | Reassess if sound alerts are introduced. |
| 1.4.3 Contrast (Minimum) | AA | Partially Supports | Avalonia styling aims for readable contrast and the custom workspace tab focus state now has an explicit higher-contrast standard-theme treatment, but measured visual evidence is not complete. | Add forced-colors/contrast screenshots or manual measurements before public ACR. |
| 1.4.4 Resize Text | AA | Partially Supports | Main, workspace, service-book, settings and editor windows now explicitly declare resize behavior; standard windows are resizable and dialog editors are scrollable. | Manual 200/400% Windows scaling pass. |
| 1.4.5 Images of Text | AA | Supports | Core UI does not use images of text. | Keep asset review in release checklist. |
| 1.4.10 Reflow | AA | Partially Supports | Desktop UI supports resize and scrollable content; top-level window resize behavior is explicit and guarded. Mobile/Android is not evaluated yet. | Manual high-DPI and small-window pass for all main screens. |
| 1.4.11 Non-text Contrast | AA | Partially Supports | Focus indicators and controls are visible in standard theme; the custom radio-button workspace tab strip has a guarded 2 px focus border and an automated 3:1 contrast check against the focused background. | Manual forced-colors and focus contrast evidence. |
| 1.4.12 Text Spacing | AA | Partially Supports | Native desktop controls are less directly affected by web text-spacing CSS. | Document non-web applicability and verify large system font behavior. |
| 1.4.13 Content on Hover or Focus | AA | Partially Supports | Workflows do not depend on hover-only content; tray native menu has a documented accessible alternative. | Keep tray exception in backlog and use `Aplikace -> Akce na liště` as supported path. |
| 2.1.1 Keyboard | A | Partially Supports | Main shell, menu, workspaces and dialog editors are keyboard-first and covered by regression tests. | Full manual NVDA keyboard pass before ACR publication. |
| 2.1.2 No Keyboard Trap | A | Partially Supports | Dialog helper controls Escape, save/cancel and focus return; known TextBox fallback remains temporary. | Retire fallback when Avalonia UIA issue is fixed, or document as exception. |
| 2.1.4 Character Key Shortcuts | A | Supports | Global shortcuts use modifiers such as Ctrl/Alt/F10 and are protected around text fields. | Guard new manual key handlers. |
| 2.2.1 Timing Adjustable | A | Supports | No timed editing expiry is introduced by Vehimap itself. | Reassess updater/download flows if timeouts become user-visible. |
| 2.2.2 Pause, Stop, Hide | A | Supports | No auto-moving content requiring pause controls. | Reassess if live dashboards or animations are added. |
| 2.3.1 Three Flashes | A | Supports | Vehimap does not generate flashing content. | Keep visual review in release checklist. |
| 2.4.1 Bypass Blocks | A | Not Applicable | Native desktop app does not have repeated web page blocks; keyboard focus starts on the main list. | Explain non-web mapping in ACR. |
| 2.4.2 Page Titled | A | Supports | Windows and dialogs have localized titles and primary headings. | Static heading/title guard remains required. |
| 2.4.3 Focus Order | A | Partially Supports | Focus order is designed and tested for shell/menu/dialogs. | Continue manual pass after every editor or shell refactor. |
| 2.4.4 Link Purpose | A | Supports | Links/buttons use descriptive labels and help text for destructive actions. | Verify donation/release links and diagnostics actions. |
| 2.4.5 Multiple Ways | AA | Supports | Major workflows are available through menu, tabs, window actions and search/advisor paths. | Reassess Android navigation later. |
| 2.4.6 Headings and Labels | AA | Supports | Primary headings, labels and accessible names are guarded. | Keep resource-based accessible names in both languages. |
| 2.4.7 Focus Visible | AA | Partially Supports | Keyboard focus is visible in tested Windows flows and the custom workspace tab headers have app-level focus styles covered by presence and contrast regression tests. | Manual forced-colors and high-DPI evidence needed. |
| 2.4.11 Focus Not Obscured | AA | Partially Supports | Main lists and dialogs are scrollable/resizable, top-level resize behavior is guarded, and previous focus regressions are covered. | Manual 400% and large system font pass. |
| 2.5.1 Pointer Gestures | A | Supports | No path-based or multipoint gestures are required. | Reassess future mobile shell. |
| 2.5.2 Pointer Cancellation | A | Supports | Actions are explicit commands, not down-event-only side effects. | Keep for future drag/drop features. |
| 2.5.3 Label in Name | A | Partially Supports | Visible button text and accessible names are generally aligned through resources; main workspace `Open window` / `V okně` buttons now keep the visible label inside the EN/CS accessible name and are guarded. | Manual check icon/automation-only controls and remaining resource-expanded labels. |
| 2.5.4 Motion Actuation | A | Not Applicable | Vehimap does not use device motion controls. | Reassess Android. |
| 2.5.7 Dragging Movements | AA | Supports | No drag-only workflow is required. | Reassess future ordering features. |
| 2.5.8 Target Size | AA | Partially Supports | Desktop controls are standard-sized, custom tab headers have an explicit 34 px minimum height, and fixed interactive XAML dimensions are guarded against dropping below 24 px. | Manual pointer target pass before public ACR. |
| 3.1.1 Language | A | Partially Supports | Application language is controlled by settings/installer seed; resources cover EN/CS. | Keep i18n gate green and verify first-render language after update. |
| 3.1.2 Language of Parts | AA | Partially Supports | User-entered mixed-language data is preserved, not translated. | ACR should explicitly define product UI vs. user data responsibility. |
| 3.2.1 On Focus | A | Supports | Focus does not intentionally trigger unexpected context changes. | Keep manual shell/dialog pass. |
| 3.2.2 On Input | A | Supports | Inputs require explicit save/action for persistence. | Keep combo/filter behavior under tests. |
| 3.2.3 Consistent Navigation | AA | Supports | Main menu/tabs/workspace patterns are consistent. | Reassess after Android navigation design. |
| 3.2.4 Consistent Identification | AA | Supports | Actions use shared resources and stable command patterns. | Guard new strings and commands. |
| 3.2.6 Consistent Help | A | Partially Supports | About, diagnostics, README and docs provide help paths; in-app user help is still limited. | Consider localized user help before final ACR. |
| 3.3.1 Error Identification | A | Supports | Validation errors and failures are text-backed and localized. | Continue raw exception/message guard. |
| 3.3.2 Labels or Instructions | A | Supports | Editor fields, visible form guidance and data-changing actions have labels, accessible names and consequence-oriented help text. | Keep static accessibility label tests green. |
| 3.3.3 Error Suggestion | AA | Partially Supports | Validation gives specific next actions in current editors. | Manual invalid-state pass for every dialog. |
| 3.3.4 Error Prevention | AA | Partially Supports | Backup/restore/update/destructive flows use confirmations and status text; static and view-model guards require user-language help text for restore, vehicle-package import, update install, delete, managed-attachment conversion and starter-bundle apply actions. | Review all data-replacing actions before ACR publication, including confirmation, cancel and status behavior. |
| 3.3.7 Redundant Entry | A | Partially Supports | Vehimap reuses existing record data where possible but does not yet have a formal redundant-entry inventory. | Add manual inventory for repeated vehicle/document fields. |
| 3.3.8 Accessible Authentication | AA | Not Applicable | Vehimap has no authentication flow. | Reassess if cloud/sync accounts are introduced. |
| 4.1.2 Name, Role, Value | A | Partially Supports | UI Automation names, shell context text, visible form guidance, item types/statuses, headings and live regions are guarded. | TextBox UIA fallback remains known exception. |
| 4.1.3 Status Messages | AA | Supports | Live regions cover status, progress and blocking errors. | Manual NVDA verification of update/restore/import statuses. |
