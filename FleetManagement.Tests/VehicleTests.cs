using FleetManagement.Domain.Entities;

namespace FleetManagement.Tests;

public class VehicleTests
{
	[Fact]
	public void Create_Vehicle_With_Valid_Parameters_Should_Initialize_Correctly()
	{
		// Arrange
		var licensePlate = "16 ABC 123";
		var make = "Mercedes-Benz";
		var model = "Actros";
		var year = 2023;
		var mileage = 50000;

		// Act
		var vehicle = new Vehicle(licensePlate, make, model, year, mileage);

		// Assert
		Assert.NotNull(vehicle);
		Assert.NotEqual(Guid.Empty, vehicle.Id);
		Assert.Equal(licensePlate, vehicle.LicensePlate);
		Assert.Equal(make, vehicle.Make);
		Assert.Equal(model, vehicle.Model);
		Assert.Equal(year, vehicle.Year);
		Assert.Equal(mileage, vehicle.Mileage);
		Assert.Equal(VehicleStatus.Available, vehicle.Status);
	}

	[Fact]
	public void UpdateMileage_With_Valid_Value_Should_Update_Mileage()
	{
		// Arrange
		var vehicle = new Vehicle("16 ABC 123", "Volvo", "FH16", 2022, 100000);
		var newMileage = 105000;

		// Act
		vehicle.UpdateMileage(newMileage);

		// Assert
		Assert.Equal(newMileage, vehicle.Mileage);
	}

	[Fact]
	public void UpdateMileage_With_Lower_Value_Should_Throw_InvalidOperationException()
	{
		// Arrange
		var vehicle = new Vehicle("16 ABC 123", "Volvo", "FH16", 2022, 100000);

		// Act & Assert
		var exception = Assert.Throws<InvalidOperationException>(() => vehicle.UpdateMileage(95000));
		Assert.Equal("Yeni kilometre mevcut kilometreden küçük olamaz.", exception.Message);
	}

	[Fact]
	public void AssignToDriver_When_Vehicle_Is_Available_Should_Set_Status_To_Assigned()
	{
		// Arrange
		var vehicle = new Vehicle("16 ABC 123", "Scania", "R500", 2024, 10000);

		// Act
		vehicle.AssignToDriver();

		// Assert
		Assert.Equal(VehicleStatus.Assigned, vehicle.Status);
	}

	[Fact]
	public void AssignToDriver_When_Vehicle_Is_InMaintenance_Should_Throw_InvalidOperationException()
	{
		// Arrange
		var vehicle = new Vehicle("16 ABC 123", "Scania", "R500", 2024, 10000);

		// Reflection kullanarak private setter olan Status'ü InMaintenance durumuna getiriyoruz
		typeof(Vehicle).GetProperty(nameof(Vehicle.Status))!
				.SetValue(vehicle, VehicleStatus.InMaintenance);

		// Act & Assert
		var exception = Assert.Throws<InvalidOperationException>(() => vehicle.AssignToDriver());
		Assert.Equal("Bakımdaki araç zimmetlenemez.", exception.Message);
	}
}