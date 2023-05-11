using ApiApplication;

using Extensions.Sql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

SqlHandler.ConnectionString = KEYS.CONNECTION;

app.UseMiddleware<ApiKeyMiddleware>(KEYS.API_KEY);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
