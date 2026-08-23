using FleetManagement.Application.Interfaces;
using FleetManagement.Infrastructure.Persistence;
using FleetManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext Entegrasyonu
builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection (DI)
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
	// .NET 9 OpenAPI Endpoint
	app.MapOpenApi();

	// Swagger UI yerine SCALAR Entegrasyonu
	app.MapScalarApiReference(options =>
	{
		options.WithTitle("Fleet Management API Reference")
					 .WithTheme(ScalarTheme.Purple)
					 .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
	});
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
