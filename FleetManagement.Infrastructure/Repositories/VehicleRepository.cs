using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using FleetManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Infrastructure.Repositories
{
	public class VehicleRepository : IVehicleRepository
	{
		private readonly ApplicationDbContext _context;

		public VehicleRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<Vehicle?> GetByIdAsync(Guid id)
		{
			return await _context.Vehicles.FindAsync(id);
		}

		public async Task<IEnumerable<Vehicle>> GetAllAsync()
		{
			return await _context.Vehicles.ToListAsync();
		}

		public async Task AddAsync(Vehicle vehicle)
		{
			await _context.Vehicles.AddAsync(vehicle);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAsync(Vehicle vehicle)
		{
			_context.Vehicles.Update(vehicle);
			await _context.SaveChangesAsync();
		}
	}
}
