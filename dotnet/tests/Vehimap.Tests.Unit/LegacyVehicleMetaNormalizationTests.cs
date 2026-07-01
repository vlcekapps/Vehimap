// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Storage.Legacy;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class LegacyVehicleMetaNormalizationTests
{
    [Fact]
    public void Normalize_tag_list_matches_legacy_ahk_rules()
    {
        var normalized = LegacyVehicleMetaNormalization.NormalizeTagList(" rodina; servis, RODINA, , veterán ;servis ");

        Assert.Equal("rodina, servis, veterán", normalized);
    }

    [Fact]
    public void Normalize_tag_list_returns_empty_for_blank_input()
    {
        Assert.Equal(string.Empty, LegacyVehicleMetaNormalization.NormalizeTagList(" ; ,  "));
    }
}
