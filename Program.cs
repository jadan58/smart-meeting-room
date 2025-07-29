using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartMeetingRoomAPI.Data;
using SmartMeetingRoomAPI.Mappers;
using SmartMeetingRoomAPI.Models;
using SmartMeetingRoomAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with SQL Server connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Identity with ApplicationUser and IdentityRole<Guid>
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Register repositories
builder.Services.AddScoped<IRoomRepository, SqlRoomRepository>();
builder.Services.AddScoped<IMeetingRepository, SqlMeetingRepository>();

// Register AutoMapper profiles
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Use Swagger only in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed test user on startup safely
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SmartMeetingRoomAPI.Seeders.Seeder.SeedTestUserAsync(services);
    }
    catch (Exception ex)
    {
        // Log error or handle as needed
        Console.WriteLine($"Error seeding database: {ex.Message}");
    }
}

app.Run();
