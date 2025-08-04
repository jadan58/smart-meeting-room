using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartMeetingRoomAPI.Data;
using SmartMeetingRoomAPI.Mappers;
using SmartMeetingRoomAPI.Models;
using SmartMeetingRoomAPI.Repositories;
using SmartMeetingRoomAPI.Seeders;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with SQL Server connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Identity with ApplicationUser and IdentityRole<Guid>
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        RoleClaimType = ClaimTypes.Role
    };
});

// Register repositories
builder.Services.AddScoped<IRoomRepository, SqlRoomRepository>();
builder.Services.AddScoped<IMeetingRepository, SqlMeetingRepository>();
builder.Services.AddScoped<IUserRepository, SQLUserRepository>();
builder.Services.AddScoped<IFeatureRepository, SqlFeatureRepository>();
builder.Services.AddScoped<INotificationsRepository, SQLNotificationsRepository>();
builder.Services.AddScoped<IRecurringBookingRepository, SQLRecurringRepository>();

// Register AutoMapper profiles
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "SmartMeetingRoomAPI", Version = "v1" });

    var jwtSecurityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "Put **_ONLY_** your JWT Bearer token here:",

        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Id = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Use Swagger only in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();   // <-- show stack traces in-browser
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

// Add authentication/authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed test user on startup safely
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await Seeder.SeedTestUserAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding database: {ex.Message}");
    }
}


app.Run();
