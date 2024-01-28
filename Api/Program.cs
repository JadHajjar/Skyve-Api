using Extensions.Sql;

using Microsoft.AspNetCore.Authentication.Cookies;

using SkyveApi;
using SkyveApi.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

app.UseMiddleware<ApiKeyMiddleware>(KEYS.API_KEY);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
