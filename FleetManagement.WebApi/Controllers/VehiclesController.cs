using FleetManagement.Application.DTOs;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.WebApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class VehiclesController : ControllerBase
	{
		private readonly IVehicleRepository _repository;

		public VehiclesController(IVehicleRepository repository)
		{
			_repository = repository;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var vehicles = await _repository.GetAllAsync();
			return Ok(vehicles);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateVehicleDto dto)
		{
			var vehicle = new Vehicle(dto.LicensePlate, dto.Make, dto.Model, dto.Year, dto.Mileage);
			await _repository.AddAsync(vehicle);
			return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
		}

		[HttpGet("{id:guid}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var vehicle = await _repository.GetByIdAsync(id);
			if (vehicle == null) return NotFound();
			return Ok(vehicle);
		}

		[HttpPatch("{id:guid}/mileage")]
		public async Task<IActionResult> UpdateMileage(Guid id, [FromBody] UpdateMileageDto dto)
		{
			var vehicle = await _repository.GetByIdAsync(id);
			if (vehicle == null) return NotFound();

			try
			{
				vehicle.UpdateMileage(dto.NewMileage);
				await _repository.UpdateAsync(vehicle);
				return NoContent();
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
	}
}
