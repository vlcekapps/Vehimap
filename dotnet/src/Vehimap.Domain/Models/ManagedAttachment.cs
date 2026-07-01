// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Domain.Models;

public sealed record ManagedAttachment(
    string RelativePath,
    byte[] Content);
