namespace FleetManagement.Application.DTOs
{
	public record CreateVehicleDto(string LicensePlate, string Make, string Model, int Year, int Mileage);
	public record VehicleResponseDto(Guid Id, string LicensePlate, string Make, string Model, int Year, int Mileage, string Status);
	public record UpdateMileageDto(int NewMileage);
}
