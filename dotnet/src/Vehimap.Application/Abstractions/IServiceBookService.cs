using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Abstractions;

public interface IServiceBookService
{
    ServiceBookSummary BuildVehicleServiceBook(VehimapDataSet dataSet, string vehicleId, DateOnly today);
}
