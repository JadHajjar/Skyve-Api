using Extensions.Sql;

using Skyve.Compatibility.Domain;

using SkyveApi.Domain.CS1;

using System.Text.Json;

namespace SkyveApi.Utilities;

public static class SteamUtil
{
    private static readonly List<ulong> _idsToUpdate = [];

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
            var remainingUsers = new List<SteamUser>();

            foreach (var chunk in idsToUpdate.Chunk(50))
            {
                remainingUsers.AddRange(await GetSteamUsersAsync(chunk));
            }

            foreach (var item in remainingUsers)
            {
                item.SqlAdd(true);
            }
        }
    }

    public static async Task<List<SteamUser>> GetUsersAsync(List<ulong> ids)
    {
        ids?.RemoveAll(x => x == 0);

        if (!(ids?.Any() ?? false))
        {
            return [];
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
            var remainingUsers = new List<SteamUser>();

            foreach (var chunk in ids.Chunk(50))
            {
                remainingUsers.AddRange(await GetSteamUsersAsync(chunk));
            }

            foreach (var item in remainingUsers)
            {
                item.SqlAdd(true);

                results.Add(item);
            }
        }

        _idsToUpdate.AddRange(results.Where(x => x.Timestamp < DateTime.Now.AddDays(-6)).Select(x => x.SteamId));

        return results;
    }

    public static async Task<List<SteamUser>> GetSteamUsersAsync(ulong[] steamId64s)
    {
        if (steamId64s.Length == 0)
        {
            return [];
        }

        try
        {
            var idString = string.Join(",", steamId64s.Distinct());

            var result = await Get<SteamUserRootResponse>($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/",
                ("key", KEYS.STEAM_API_KEY),
                ("steamids", idString));

            return result?.response.players.Select(x => new SteamUser(x)).ToList() ?? [];
        }
        catch { }

        return [];
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