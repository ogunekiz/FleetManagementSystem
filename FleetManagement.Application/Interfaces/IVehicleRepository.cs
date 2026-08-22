using FleetManagement.Domain.Entities;

namespace FleetManagement.Application.Interfaces
{
	public interface IVehicleRepository
	{
		Task<Vehicle?> GetByIdAsync(Guid id);
		Task<IEnumerable<Vehicle>> GetAllAsync();
		Task AddAsync(Vehicle vehicle);
		Task UpdateAsync(Vehicle vehicle);
	}
}
