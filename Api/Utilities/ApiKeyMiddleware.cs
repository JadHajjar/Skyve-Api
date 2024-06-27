using System.Text.RegularExpressions;

namespace SkyveApi.Utilities;

public class ApiKeyMiddleware
{
	private readonly RequestDelegate _next;
	private readonly string _apiKey;
	private readonly Regex _regex;

	public ApiKeyMiddleware(RequestDelegate next, string apiKey)
	{
		_next = next;
		_apiKey = apiKey;
		_regex = new Regex(@"^/\w+/api", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	}

	public async Task Invoke(HttpContext context)
	{
		var regex = Regex.Match(context.Request.Path.Value ?? string.Empty, @"^/\w+/api", RegexOptions.IgnoreCase);

		if (_regex.IsMatch(context.Request.Path.Value ?? string.Empty) &&
			(!context.Request.Headers.TryGetValue("API_KEY", out var apiKeyValues)
			|| apiKeyValues.Count == 0
			|| apiKeyValues[0] != _apiKey))
		{
			context.Response.StatusCode = 401;
			await context.Response.WriteAsync("Unauthorized");
			return;
		}

		await _next.Invoke(context);
	}
}
