using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Abstractions;

public interface IGlobalSearchService
{
    IReadOnlyList<GlobalSearchResult> Search(VehimapDataRoot dataRoot, VehimapDataSet dataSet, string query);
}
