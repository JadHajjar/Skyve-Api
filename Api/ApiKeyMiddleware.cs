﻿namespace ApiApplication;

public class ApiKeyMiddleware
{
	private readonly RequestDelegate _next;
	private readonly string _apiKey;

	public ApiKeyMiddleware(RequestDelegate next, string apiKey)
	{
		_next = next;
		_apiKey = apiKey;
	}

	public async Task Invoke(HttpContext context)
	{
		if (!context.Request.Headers.TryGetValue("API_KEY", out var apiKeyValues)
			|| apiKeyValues.Count == 0
			|| apiKeyValues[0] != _apiKey)
		{
			context.Response.StatusCode = 401;
			await context.Response.WriteAsync("Unauthorized");
			return;
		}

		await _next.Invoke(context);
	}
}
