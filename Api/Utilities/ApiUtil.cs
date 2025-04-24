using System.Text;
using System.Text.Json;

namespace SkyveApi.Utilities;
public static class ApiUtil
{
	public static async Task<T?> Get<T>(string url, params (string, object)[] queryParams)
	{
		return await Get<T>(url, new (string, string)[0], queryParams);
	}

	public static async Task<T?> Get<T>(string url, (string, string)[] headers, params (string, object)[] queryParams)
	{
		return await Send<T>("GET", url, headers, queryParams);
	}

	public static async Task<T?> Delete<T>(string url, params (string, object)[] queryParams)
	{
		return await Delete<T>(url, new (string, string)[0], queryParams);
	}

	public static async Task<T?> Delete<T>(string url, (string, string)[] headers, params (string, object)[] queryParams)
	{
		return await Send<T>("DELETE", url, headers, queryParams);
	}

	private static async Task<T?> Send<T>(string method, string baseUrl, (string, string)[] headers, params (string, object)[] queryParams)
	{
		var url = baseUrl;

		if (queryParams.Length > 0)
		{
			var query = queryParams.Select(x => $"{Uri.EscapeDataString(x.Item1)}={Uri.EscapeDataString(x.Item2.ToString()!)}");
			url += "?" + string.Join("&", query);
		}

		using var httpClient = new HttpClient();

		foreach (var item in headers)
		{
			httpClient.DefaultRequestHeaders.Add(item.Item1, item.Item2);
		}

		var httpResponse = await httpClient.SendAsync(new HttpRequestMessage(new HttpMethod(method), new Uri(url)));

		if (httpResponse.IsSuccessStatusCode)
		{
			if (typeof(T) == typeof(byte[]))
			{
				return (T)(object)await httpResponse.Content.ReadAsByteArrayAsync();
			}

			var response = await httpResponse.Content.ReadAsStringAsync();

			return JsonSerializer.Deserialize<T>(response);
		}

		return default;
	}

	public static async Task<T?> Post<TBody, T>(string url, TBody body, params (string, object)[] queryParams)
	{
		return await Post<TBody, T>(url, body, new (string, string)[0], queryParams);
	}

	public static async Task<T?> Post<TBody, T>(string baseUrl, TBody body, (string, string)[] headers, params (string, object)[] queryParams)
	{
		var url = baseUrl;
		var json = JsonSerializer.Serialize(body);

		if (queryParams.Length > 0)
		{
			var query = queryParams.Select(x => $"{Uri.EscapeDataString(x.Item1)}={Uri.EscapeDataString(x.Item2.ToString()!)}");
			url += "?" + string.Join("&", query);
		}

		using var httpClient = new HttpClient();

		foreach (var item in headers)
		{
			httpClient.DefaultRequestHeaders.Add(item.Item1, item.Item2);
		}

		var content = new StringContent(json, Encoding.UTF8, "application/json");
		var httpResponse = await httpClient.PostAsync(url, content);

		if (httpResponse.IsSuccessStatusCode)
		{
			if (typeof(T) == typeof(byte[]))
			{
				return (T)(object)await httpResponse.Content.ReadAsByteArrayAsync();
			}

			var response = await httpResponse.Content.ReadAsStringAsync();

			return JsonSerializer.Deserialize<T>(response);
		}

		return default;
	}
}
