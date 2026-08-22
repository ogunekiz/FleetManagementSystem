using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Infrastructure.Persistence
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

		public DbSet<Vehicle> Vehicles { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Vehicle>(builder =>
			{
				builder.HasKey(v => v.Id);
				builder.Property(v => v.LicensePlate).IsRequired().HasMaxLength(20);
				builder.Property(v => v.Make).IsRequired().HasMaxLength(50);
				builder.Property(v => v.Model).IsRequired().HasMaxLength(50);
			});
		}
	}
}
