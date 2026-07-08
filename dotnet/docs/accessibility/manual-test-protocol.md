<!-- SPDX-License-Identifier: GPL-3.0-or-later -->

# Vehimap Manual Accessibility Test Protocol

This protocol describes the manual evidence required before publishing a
customer-facing Accessibility Conformance Report.

## Test Environments

- Windows 11 + NVDA, current stable version.
- Windows 11 + Narrator.
- Keyboard-only without a mouse.
- Windows high contrast / forced-colors mode.
- Windows display scaling at 200% and 400%, plus larger system text where
  available.
- Future passes: Android screen reader, macOS VoiceOver and Linux Orca.

## Core Scenarios

1. Start Vehimap with empty data and with migrated 1.x/2.0 data; verify window
   title announcement and initial focus on the vehicle list or primary action.
2. Open and close the main menu with Alt/F10; verify normal Tab/Shift+Tab does
   not enter menu roots.
3. Navigate the vehicle list, switch every workspace tab, open every `In window`
   view and return focus to the invoking surface.
4. Create, edit, cancel and save vehicle, history, fuel, reminder, maintenance
   and document records. Verify first focus, Shift+Tab behavior, Escape,
   Ctrl+S, validation errors and return focus.
5. Exercise text fields with character, word and line navigation; record whether
   the temporary TextBox UIA fallback was needed.
6. Check combo boxes with Up/Down, Alt+Down, Enter and Escape.
7. Run data audit, smart advisor, timeline, global search, service book, cost
   reports and dashboard. Verify list item names, item type/status and open
   actions.
8. Run Settings language/unit/currency changes and confirm localized status
   messages after restart.
9. Run backup export, restore, vehicle package import/export, data health check,
   update check and update install progress.
10. Trigger notifications and tray action alternatives. Use `Aplikace -> Akce na
    liště` / `Ctrl+Shift+Y` as the supported screen-reader path.
11. Export calendar, service book HTML, cost TSV/HTML and printable vehicle
    report; verify labels, units and user-entered data preservation.

## Evidence To Record

For each run, save a note in `dotnet/docs/accessibility-evidence/` with:

- date, commit and build/channel;
- operating system and assistive technology version;
- data set used;
- scenarios passed and failed;
- screenshots or speech-viewer excerpts when useful;
- whether failures are product bugs, platform limitations or known documented
  exceptions.

