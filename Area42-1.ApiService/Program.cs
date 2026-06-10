using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Area42_1.ApiService.Data;
using Area42_1.ApiService.Data.Repositories;
using Area42_1.ApiService.Services;
using Area42_1.ApiService.Models.Admin;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\mssqllocaldb;Database=Area42;Trusted_Connection=true;";
builder.Services.AddDbContext<Area42Context>(options =>
    options.UseSqlServer(connectionString)
);

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? Guid.NewGuid().ToString();
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Area42";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Area42Client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Admin portal authorization policies
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(context =>
        {
            var rankClaim = context.User.FindFirst("rank")?.Value;
            return rankClaim == "Admin" || rankClaim == "SuperAdmin" || rankClaim == "SeniorManager";
        }));

    options.AddPolicy("SuperAdminOnly", policy =>
        policy.RequireAssertion(context =>
        {
            var rankClaim = context.User.FindFirst("rank")?.Value;
            return rankClaim == "SuperAdmin";
        }));

    options.AddPolicy("SecurityAdminOnly", policy =>
        policy.RequireAssertion(context =>
        {
            var rankClaim = context.User.FindFirst("rank")?.Value;
            return rankClaim == "SuperAdmin" || rankClaim == "Admin";
        }));

    options.AddPolicy("FinancialAccessOnly", policy =>
        policy.RequireAssertion(context =>
        {
            var rankClaim = context.User.FindFirst("rank")?.Value;
            return rankClaim == "CustomerSupport" || rankClaim == "BookingManager" || rankClaim == "PropertyManager" 
                || rankClaim == "SeniorManager" || rankClaim == "Admin" || rankClaim == "SuperAdmin";
        }));

    options.AddPolicy("FinancialAuditOnly", policy =>
        policy.RequireAssertion(context =>
        {
            var rankClaim = context.User.FindFirst("rank")?.Value;
            return rankClaim == "SeniorManager" || rankClaim == "Admin" || rankClaim == "SuperAdmin";
        }));

    options.AddPolicy("FinancialApprovalOnly", policy =>
        policy.RequireAssertion(context =>
        {
            var rankClaim = context.User.FindFirst("rank")?.Value;
            return rankClaim == "SeniorManager" || rankClaim == "Admin" || rankClaim == "SuperAdmin";
        }));
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Dependency Injection
builder.Services.AddScoped<IAccommodationRepository, AccommodationRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();

builder.Services.AddScoped<IAccommodationService, AccommodationService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "Area42 Reservation API is running.");
app.MapDefaultEndpoints();

// Apply migrations and seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Area42Context>();
    db.Database.Migrate();
    DatabaseSeeder.SeedDatabase(db);
}

app.Run();
