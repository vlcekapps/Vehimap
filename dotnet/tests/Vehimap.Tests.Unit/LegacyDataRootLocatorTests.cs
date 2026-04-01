using Vehimap.Storage.Legacy;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class LegacyDataRootLocatorTests
{
    [Fact]
    public void Resolve_Uses_portable_data_when_data_folder_exists()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-root-" + Guid.NewGuid());
        Directory.CreateDirectory(Path.Combine(tempRoot, "data"));

        try
        {
            var locator = new LegacyDataRootLocator();
            var root = locator.Resolve(tempRoot);

            Assert.True(root.IsPortable);
            Assert.Equal(Path.Combine(tempRoot, "data"), root.DataPath);
        }
        finally
        {
            Directory.Delete(tempRoot, true);
        }
    }
}
