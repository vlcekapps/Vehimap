<!-- SPDX-License-Identifier: GPL-3.0-or-later -->
# Vehimap mobile architecture

This document is the implementation contract for Android and any future compact-screen
Vehimap client. Mobile parity means sharing domain behavior and workflows with desktop;
it does not mean shrinking the desktop tab strip onto a phone.

## Primary navigation

Compact screens use exactly four persistent top-level destinations:

1. `Home` provides the dashboard summary and the first item that needs attention.
2. `Vehicles` opens the fleet list and then a selected vehicle hub.
3. `Alerts` combines due dates, audit findings, maintenance, fuel warnings and Smart
   Advisor recommendations.
4. `More` contains global search, settings, backup/restore, update, About, author support
   and other application-level actions as they are implemented.

The current shell uses standard Avalonia `RadioButton` controls with persistent text
labels and 56-device-independent-pixel touch height. On larger Android windows this may
later adapt to a navigation rail without changing destination keys or viewmodels.

## Vehicle navigation

The desktop workspaces become routes under the selected vehicle hub, not top-level tabs:

- detail;
- history;
- fuel;
- documents and attachments;
- reminders;
- maintenance;
- timeline;
- costs;
- service book;
- vehicle-specific audit and recommendations.

The first mobile vertical slice provides the vehicle list, a separate read-only vehicle
hub and real evidence counts. Each evidence route will be enabled only when it has a
functional view; the shell must not expose placeholder actions that appear usable.

## Editor contract

Inline editors are prohibited on mobile. Every create, edit, completion or settings form
must be a dedicated full-screen route or a platform-appropriate modal surface with:

- an unambiguous `New ...` or `Edit ...` heading;
- a first logical field and standard forward/backward focus order;
- visible and accessible `Save` and `Cancel` actions;
- Android system Back and an explicit Back/Cancel path;
- validation that keeps the editor open and focuses the invalid field;
- focus or selection restoration to the originating list item after save/cancel;
- no host-list or host-detail controls mixed into the editor focus order.

An editor route owns a draft copy. It must not mutate the live SQLite data set before an
explicit save completes successfully.

## Shared feature rule

New Vehimap features start in `Vehimap.Domain` and `Vehimap.Application`. Desktop and
mobile clients consume the same services, stable entity kinds, localization keys,
canonical km/l storage, currency-display rules and SQLite contracts. Desktop workspace
viewmodels must never be referenced from `Vehimap.Mobile`.

Once Android reaches CRUD parity, a user-facing feature is complete only when its change
includes the applicable desktop workspace, mobile route, EN/CS resources, keyboard and
TalkBack/NVDA metadata, shared tests and platform-specific smoke coverage. A documented
platform exception is required when an operating system cannot support the behavior.

## Accessibility baseline

- Use standard controls and automation properties; do not encode roles into visible text.
- Keep controls at least 48 device-independent pixels in the touch dimension.
- Never require a swipe gesture, color, icon or pointer-only action.
- Preserve persistent text labels in primary navigation.
- Use headings, live status text and human-readable list item names.
- Handle Android system Back through Avalonia `TopLevel.BackRequested`: mark the event
  handled only after an inner route or non-home destination was actually popped. This
  contract applies to both three-button and gesture navigation; Back from `Home` remains
  unhandled so Android can leave the application normally.
- Do not add Vehimap-specific automation peers to mask the tracked Avalonia Android role
  announcement limitation.

Manual TalkBack validation remains required on a physical Android device. The role issue
and its package-version review are tracked in `ACCESSIBILITY.md`. Mobile interaction
smoke must exercise Android Back with three-button navigation and, when a test device is
available, gesture navigation as well.

## References

- [Android layout and navigation patterns](https://developer.android.com/design/ui/mobile/guides/layout-and-content/layout-and-nav-patterns)
- [Android navigation bar guidance](https://developer.android.com/develop/ui/compose/components/navigation-bar)
- [Android accessibility testing](https://developer.android.com/guide/topics/ui/accessibility/testing)
- [Avalonia Android platform guide](https://docs.avaloniaui.net/docs/platform-specific-guides/android)
