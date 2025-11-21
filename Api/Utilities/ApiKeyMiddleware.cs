using Extensions.Sql;

using SkyveApi.Domain.Generic;

using System.Text.RegularExpressions;

namespace SkyveApi.Utilities;

public class ApiKeyMiddleware(RequestDelegate next)
{
	private readonly RequestDelegate _next = next;
	private readonly Regex _regex = new(@"^/\w+/api", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	private readonly Dictionary<string, Regex> _keys = DynamicSql.SqlGet<ApiKeys>()?.ToDictionary(x => x.ApiKey!, x => new Regex(x.AllowedDirectories ?? ".", RegexOptions.IgnoreCase | RegexOptions.Compiled), StringComparer.InvariantCultureIgnoreCase) ?? [];

	public async Task Invoke(HttpContext context)
	{
		var match = _regex.Match(context.Request.Path.Value ?? string.Empty);

		if (match.Success &&
			(!context.Request.Headers.TryGetValue("API_KEY", out var apiKeyValues)
			|| apiKeyValues.Count == 0
			|| !_keys.ContainsKey(apiKeyValues[0])
			|| !_keys[apiKeyValues[0]].IsMatch(context.Request.Path.Value ?? string.Empty)))
		{
			context.Response.StatusCode = 401;
			await context.Response.WriteAsync("Unauthorized");
			return;
		}

		await _next.Invoke(context);
	}
}
