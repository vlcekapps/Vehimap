// SPDX-License-Identifier: GPL-3.0-or-later
using Xunit;

// Live sessions share the Windows desktop and keyboard, even with isolated application data.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
