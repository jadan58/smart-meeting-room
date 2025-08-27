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

// -----------------------
// Database & Identity
// -----------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// -----------------------
// JWT Authentication
// -----------------------
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
        ValidateLifetime = true,             // ⬅️ Reject expired tokens
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.Zero            // ⬅️ No default 5 min grace period
    };
});

// -----------------------
// Repositories
// -----------------------
builder.Services.AddScoped<IRoomRepository, SqlRoomRepository>();
builder.Services.AddScoped<IMeetingRepository, SqlMeetingRepository>();
builder.Services.AddScoped<IUserRepository, SQLUserRepository>();
builder.Services.AddScoped<IFeatureRepository, SqlFeatureRepository>();
builder.Services.AddScoped<INotificationsRepository, SQLNotificationsRepository>();
builder.Services.AddScoped<IRecurringBookingRepository, SQLRecurringRepository>();

// -----------------------
// AutoMapper
// -----------------------
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

// -----------------------
// Controllers
// -----------------------
builder.Services.AddControllers();

// -----------------------
// CORS
// -----------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5178") // match your React dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// -----------------------
// Swagger
// -----------------------
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
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

// -----------------------
// Build App
// -----------------------
var app = builder.Build();

// -----------------------
// Swagger in Development
// -----------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// -----------------------
// Middleware
// -----------------------
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");   // CORS BEFORE authentication
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

// -----------------------
// Map controllers
// -----------------------
app.MapControllers();

// -----------------------
// Seed Test User
// -----------------------
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
