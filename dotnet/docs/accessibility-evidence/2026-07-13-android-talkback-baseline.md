<!-- SPDX-License-Identifier: GPL-3.0-or-later -->

# Android TalkBack Baseline - 2026-07-13

## Environment

- Vehimap commit: `28a7b3ab`
- Channel: local Android nightly
- Device: Samsung SM-S948B
- Android API: 36
- UI framework: Avalonia 12.0.4
- Assistive technology: TalkBack

## Result

The read-only mobile shell is navigable and TalkBack reads its visible content. The
reload action exposes its name and activation hint and can be activated by double tap.
TalkBack does not, however, announce that this standard Avalonia `Button` is a button.
Selectable card/list role announcements require the same framework-level follow-up once
the mobile data set contains representative records.

An Android UI Automator dump confirmed that the reload node was exposed with text,
`clickable=true` and class name `Button`, while the vehicle list used class name
`ListBox`. These short Avalonia class names are not native Android widget class names.

## Framework analysis

Avalonia creates a `ButtonAutomationPeer` whose automation control type is `Button`.
The Avalonia Android accessibility bridge in 12.0.4 writes
`AccessibilityNodeInfo.ClassName = peer.GetClassName()` and does not consume
`peer.GetAutomationControlType()` for native role mapping. The released 12.1.0 source
still follows the same class-name path.

## Decision

- Keep the controls standard and keep their accessible names free of role words.
- Do not add Vehimap-specific Android automation peers or class-name overrides.
- Treat role announcement as a known framework exception and retest it on every
  Avalonia package upgrade.
- Do not claim Android `Supports` for affected role semantics until TalkBack announces
  them from the standard control metadata.

The separate empty-data startup focus observation is intentionally deferred until the
Android shell has a representative data set and does not change this role baseline.
