using Area42_1.Web.Components;
using Area42_1.Web.Backend;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddOutputCache();
var bookingConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is required for SQL booking storage.");
builder.Services.AddPooledDbContextFactory<BookingDbContext>(options =>
    options.UseSqlServer(bookingConnectionString));
builder.Services.AddScoped<BookingStore>();
builder.Services.AddSingleton<AdminTotpService>();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.AccessDeniedPath = "/admin/login";
        options.Cookie.Name = "Area42.Admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseOutputCache();

app.MapRazorPages();
app.MapBlazorHub();

app.MapGet("/api/accommodations", (BookingStore store) => store.Accommodations);
app.MapGet("/api/bookings", async (BookingStore store) => await store.GetBookingsAsync())
    .RequireAuthorization();
app.MapGet("/api/bookings/summary", async (BookingStore store) => await store.GetSummaryAsync())
    .RequireAuthorization();
app.MapGet("/api/availability", async (
    string accommodationId,
    DateOnly checkIn,
    DateOnly checkOut,
    BookingStore store) =>
    Results.Ok(new
    {
        available = await store.IsAvailableAsync(accommodationId, checkIn, checkOut)
    }));
app.MapPost("/api/bookings", async (BookingRequest request, BookingStore store) =>
{
    var result = await store.CreateAsync(request);
    return result.Success
        ? Results.Created($"/api/bookings/{result.Booking!.Id}", result.Booking)
        : Results.BadRequest(new { message = result.Error });
});
app.MapPut("/api/bookings/{id:guid}/status", async (
    Guid id,
    BookingStatus status,
    BookingStore store) =>
    await store.SetStatusAsync(id, status) ? Results.NoContent() : Results.NotFound())
    .RequireAuthorization();

app.MapFallbackToPage("/_Host");

app.Run();
