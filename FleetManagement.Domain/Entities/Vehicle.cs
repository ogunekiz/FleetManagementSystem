namespace FleetManagement.Domain.Entities
{
	public enum VehicleStatus
	{
		Available = 1,
		Assigned = 2,
		InMaintenance = 3
	}

	public class Vehicle
	{
		public Guid Id { get; private set; }
		public string LicensePlate { get; private set; } = string.Empty;
		public string Make { get; private set; } = string.Empty;
		public string Model { get; private set; } = string.Empty;
		public int Year { get; private set; }
		public int Mileage { get; private set; }
		public VehicleStatus Status { get; private set; }
		public DateTime CreatedAt { get; private set; }

		private Vehicle() { } // EF Core için

		public Vehicle(string licensePlate, string make, string model, int year, int mileage)
		{
			Id = Guid.NewGuid();
			LicensePlate = licensePlate;
			Make = make;
			Model = model;
			Year = year;
			Mileage = mileage;
			Status = VehicleStatus.Available;
			CreatedAt = DateTime.UtcNow;
		}

		public void UpdateMileage(int newMileage)
		{
			if (newMileage < Mileage)
				throw new InvalidOperationException("Yeni kilometre mevcut kilometreden küçük olamaz.");

			Mileage = newMileage;
		}

		public void AssignToDriver()
		{
			if (Status == VehicleStatus.InMaintenance)
				throw new InvalidOperationException("Bakımdaki araç zimmetlenemez.");

			Status = VehicleStatus.Assigned;
		}
	}
}
