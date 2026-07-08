<!-- SPDX-License-Identifier: GPL-3.0-or-later -->

# Vehimap Accessibility Conformance Report Draft

Status: internal draft for future VPAT 2.5Rev INT / ACR work. This document is
not a legal certification and must be updated after manual assistive-technology
testing.

## Product Information

- Product: Vehimap
- Version evaluated: Vehimap 2.0 nightly / current development branch
- Product type: native desktop vehicle evidence manager, with future Android
  read-only shell planned after localization and accessibility gates
- Report type: ACR-ready evidence draft for VPAT INT
- Primary evaluated platform: Windows 11 desktop
- Primary assistive technology target: NVDA
- Secondary Windows target: Narrator
- Future targets: Android accessibility services, macOS VoiceOver and Linux Orca
- Evaluation methods: source inspection, unit/accessibility guard tests,
  Appium-oriented smoke tests, manual keyboard/screen-reader protocol and
  documented exception tracking

## Standards Intended For Mapping

- WCAG 2.2 Level A and AA, interpreted for non-web software through WCAG2ICT
- Revised Section 508 software and support documentation criteria
- EN 301 549 software requirements, especially native software and platform
  accessibility interoperability sections

## Conformance Terms

- Supports: the Vehimap-provided experience has automated or manual evidence and
  no known unresolved blocker for the criterion.
- Partially Supports: Vehimap has meaningful support but still needs manual
  evidence, platform-specific validation or an open remediation item.
- Does Not Support: a known product behavior fails the criterion.
- Not Applicable: the criterion does not apply to the evaluated Vehimap
  functionality.

## Important Scope Notes

Vehimap localizes the application interface and generated system text. It does
not translate user-entered vehicle names, notes, document titles, fuel station
names or similar user data.

Vehimap stores active 2.0 data in SQLite and renders units, currency and number
separators through application preferences. Changing display units or currency
does not reinterpret historical data or perform exchange-rate conversion.

The current TextBox UIA text fallback is a documented temporary workaround for
Avalonia UIA behavior. It improves practical NVDA usability in nightly builds,
but it is not treated as a final conformance strategy.

## Evidence Pointers

- Technical accessibility checklist: `dotnet/docs/ACCESSIBILITY.md`
- Localization and formatting rules: `dotnet/docs/I18N.md`
- Manual evidence log: `dotnet/docs/accessibility-evidence/`
- WCAG2ICT matrix: `dotnet/docs/accessibility/wcag2ict-22-aa-matrix.md`
- Remediation backlog: `dotnet/docs/accessibility/a11y-remediation-backlog.md`
- Manual protocol: `dotnet/docs/accessibility/manual-test-protocol.md`

## Current Draft Position

Vehimap should not publish a final customer ACR until these gates are green:

- Windows nightly starts, restores focus and remains keyboard-operable with
  real migrated 2.0 data.
- NVDA manual smoke passes the main shell, dialog editors, Settings, About,
  update, backup/restore, audit, dashboard, documents, maintenance, service
  book and smart advisor workflows.
- Forced-colors and high-DPI evidence covers the custom workspace tab strip,
  including the app-level focus styles and automated standard-theme contrast guard
  in `App.axaml`.
- Pointer target evidence covers shell filters, workspace action panels, custom tab
  headers, dialog action buttons and installer shortcuts beyond the automated fixed
  XAML size guard.
- Non-text/icon evidence covers localized installer shortcut comments for generated
  Start menu and desktop shortcuts, with manual verification still required.
- Label-in-name evidence covers resource-expanded controls such as the main workspace
  `Open window` / `V okně` actions, including speech-input-friendly accessible names.
- Resize/reflow evidence covers explicit `CanResize` behavior for top-level desktop
  windows, including documented non-resizable transient exceptions.
- Error-prevention evidence covers consequence-oriented accessible help text for
  restore/import/delete/update actions plus managed-attachment conversion and
  starter-bundle apply actions.
- The i18n conformance gate passes English UI over Czech legacy data and Czech
  UI over the same data without translating user-entered content.
- Known exceptions are either remediated or explicitly listed in the backlog
  with customer-safe wording.
