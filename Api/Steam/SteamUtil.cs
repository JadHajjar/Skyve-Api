using Extensions.Sql;

using SkyveApi;

using SkyveApp.Domain.CS1.Steam;
using SkyveApp.Systems.Compatibility.Domain.Api;

using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkyveApp.Systems.CS1.Utilities;

public static class SteamUtil
{
	private static readonly List<ulong> _idsToUpdate = new();

	static SteamUtil()
	{
		var timer = new System.Timers.Timer
		{
			Interval = 60000,
			Enabled = true
		};

		timer.Elapsed += Timer_Elapsed;
	}

	private static async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
	{
		var idsToUpdate = _idsToUpdate.ToList();

		_idsToUpdate.Clear();

		if (idsToUpdate.Count > 0)
		{
			var remainingUsers = await GetSteamUsersAsync(idsToUpdate);

			foreach (var item in remainingUsers)
			{
				item.SqlAdd(true);
			}
		}
	}

	public static async Task<List<SteamUser>> GetUsersAsync(string idString)
	{
		if (string.IsNullOrWhiteSpace(idString))
		{
			return new();
		}

		var ids = idString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(ulong.Parse).ToList();

		if (ids.Count == 0)
		{
			return new();
		}

		var results = DynamicSql.SqlGet<SteamUser>($"[SteamId] {(ids.Count == 1 ? $"= {ids[0]}" : $"in ({string.Join(',', ids)})")}");

		for (var i = 0; i < ids.Count;)
		{
			if (results.Any(x => x.SteamId == ids[0]))
			{
				ids.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}

		if (ids.Count > 0)
		{
			var remainingUsers = await GetSteamUsersAsync(ids);

			foreach (var item in remainingUsers)
			{
				item.SqlAdd(true);

				results.Add(item);
			}
		}

		_idsToUpdate.AddRange(results.Where(x => x.Timestamp < DateTime.Now.AddDays(-7)).Select(x => x.SteamId));

		return results;
	}

	public static async Task<List<SteamUser>> GetSteamUsersAsync(List<ulong> steamId64s)
	{
		steamId64s.RemoveAll(x => x == 0);

		if (steamId64s.Count == 0)
		{
			return new();
		}

		try
		{
			var idString = string.Join(",", steamId64s.Distinct());

			var result = await Get<SteamUserRootResponse>($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/",
				("key", KEYS.STEAM_API_KEY),
				("steamids", idString));

			return result?.response.players.Select(x => new SteamUser(x)).ToList() ?? new();
		}
		catch { }

		return new();
	}

	private static async Task<T?> Get<T>(string baseUrl, params (string, object)[] queryParams)
	{
		var url = baseUrl;

		if (queryParams.Length > 0)
		{
			var query = queryParams.Select(x => $"{Uri.EscapeDataString(x.Item1)}={Uri.EscapeDataString(x.Item2.ToString()!)}");

			url += "?" + string.Join("&", query);
		}

		using var httpClient = new HttpClient();

		var httpResponse = await httpClient.SendAsync(new HttpRequestMessage(new HttpMethod("GET"), new Uri(url)));

		if (httpResponse.IsSuccessStatusCode)
		{
			var response = await httpResponse.Content.ReadAsStringAsync();

			return JsonSerializer.Deserialize<T>(response);
		}

		return typeof(T) == typeof(ApiResponse)
			? (T)(object)new ApiResponse
			{
				Message = httpResponse.ReasonPhrase
			}
			: default;
	}
}