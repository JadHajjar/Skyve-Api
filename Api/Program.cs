using Extensions;
using Extensions.Sql;

using Microsoft.AspNetCore.Authentication.Cookies;

using SkyveApi;
using SkyveApi.Utilities;

using System.Globalization;

LocaleHelper.SetCultureAndCalendar(CultureInfo.CurrentCulture = new CultureInfo("en-US"));

Locale.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddControllers();

builder.Services.AddAuthentication().AddCookie((o) =>
{
	o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
});

builder.Services.AddAuthentication(options =>
{
	options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddSteam(x => x.ApplicationKey = KEYS.STEAM_API_KEY);

var app = builder.Build();

// Configure the HTTP request pipeline.

SqlHandler.ConnectionString = KEYS.CONNECTION;

app.UseMiddleware<ApiKeyMiddleware>();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.MapRazorPages();

app.Run();
