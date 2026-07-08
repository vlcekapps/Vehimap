<!-- SPDX-License-Identifier: GPL-3.0-or-later -->

# Vehimap Accessibility Evidence

This folder contains the accessibility evidence set prepared for Vehimap 2.0.
It is intentionally modeled after the `www` project accessibility documentation,
but adjusted for native desktop software and future mobile shells.

Current status: **ACR-ready evidence draft, not a formal certification**.

## Documents

- `wcag2ict-22-aa-matrix.md` maps the Vehimap 2.0 desktop product to WCAG 2.2
  A/AA using WCAG2ICT guidance for non-web software.
- `acr-vpat-int-draft.md` is the working ACR/VPAT INT draft outline.
- `a11y-remediation-backlog.md` tracks known gaps and temporary exceptions that
  can affect future conformance statements.
- `manual-test-protocol.md` defines the screen-reader, keyboard, zoom and
  platform scenarios that must be executed before a customer-facing ACR.

## Reporting Position

Vehimap 2.0 is developed as a screen-reader-first and keyboard-first product.
Automated tests cover resource localization, accessible names, live regions,
keyboard/focus regressions and many UI Automation properties. Manual evidence is
still required for a formal report, especially for NVDA/Narrator behavior,
forced-colors, 200/400% scaling, installer/update flows and the documented
temporary TextBox UIA fallback.

The intended customer-facing format is **VPAT 2.5Rev INT** because it covers
WCAG, Revised Section 508 and EN 301 549 in one Accessibility Conformance Report
structure.

