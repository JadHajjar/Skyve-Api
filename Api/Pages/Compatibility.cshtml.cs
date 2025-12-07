using Extensions.Sql;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Skyve.Compatibility.Domain.Enums;

using SkyveApi.Domain.CS2;

using System.Text.RegularExpressions;

namespace SkyveApi.Pages;

public class CompatibilityModel : PageModel
{
	[BindProperty(SupportsGet = true)]
	public string? Patch { get; set; }
	public bool IsMobile { get; private set; }
	public bool ShowPatchProgress { get; private set; }
	public int TotalMods { get; private set; }
	public int FixedMods { get; private set; }
	public DateTime StartTime { get; private set; }
	public List<KeyValuePair<NotificationType, List<CompatibilityPackageData>>>? Data { get; private set; }
	public DateTime LastUpdateTime { get; private set; }

	public void OnGet()
	{
		IsMobile = Regex.IsMatch(Request.Headers["User-Agent"].ToString(), @"\b(mobile)|(tablet)\b", RegexOptions.IgnoreCase);

		if (Patch?.Equals("latest", StringComparison.InvariantCultureIgnoreCase) ?? false)
		{
			Patch = GetLatestPatch();
		}

		var packages = DynamicSql.SqlGet<CompatibilityPackageData>(string.IsNullOrEmpty(Patch) ? null : $"[{nameof(CompatibilityPackageData.ReviewedGameVersion)}] = '{Patch.Replace("'", "''")}'");
		//var packageLinks = GroupBy(DynamicSql.SqlGet<PackageLinkData>(), x => x.PackageId);
		//var packageStatuses = GroupBy(DynamicSql.SqlGet<PackageStatusData>(), x => x.PackageId);
		//var packageInteractions = GroupBy(DynamicSql.SqlGet<PackageInteractionData>(), x => x.PackageId);
		//var packageTags = GroupBy(DynamicSql.SqlGet<PackageTagData>(), x => x.PackageId);
		//var packageReviews = GroupBy(DynamicSql.SqlGet<ReviewRequestNoLogData>(), x => x.PackageId);

		//for (var i = 0; i < packages.Count; i++)
		//{
		//	var id = packages[i].Id;

		//	if (packageLinks.ContainsKey(id))
		//	{
		//		packages[i].Links = [.. packageLinks[id]];
		//	}

		//	if (packageStatuses.ContainsKey(id))
		//	{
		//		packages[i].Statuses = [.. packageStatuses[id]];
		//	}

		//	if (packageInteractions.ContainsKey(id))
		//	{
		//		packages[i].Interactions = [.. packageInteractions[id]];
		//	}

		//	if (packageTags.ContainsKey(id))
		//	{
		//		packages[i].Tags = [.. packageTags[id].Select(x => x.Tag!)];
		//	}

		//	if (packageReviews.ContainsKey(id))
		//	{
		//		packages[i].ActiveReports = packageReviews[id].Count;
		//	}
		//}

		ShowPatchProgress = !string.IsNullOrEmpty(Patch) && packages.Count > 0 && !packages.Any(x => x.ReviewDate < DateTime.UtcNow.AddDays(-10));

		Data = packages
			.Where(x => !string.IsNullOrEmpty(x.Name))
			.GroupBy(x => Merge(CRNAttribute.GetNotification((PackageStability)x.Stability)))
			.Select(x => new KeyValuePair<NotificationType, List<CompatibilityPackageData>>(x.Key, OrderBy(x).ToList()))
			.OrderByDescending(x => x.Key == NotificationType.Obsolete ? 11 : (int)x.Key)
			.ToList();

		if (ShowPatchProgress && Data.Count > 0)
		{
			TotalMods = Data.Sum(x => x.Value.Count);
			FixedMods = Data.Sum(x => x.Key < NotificationType.Caution ? x.Value.Count : 0);
			StartTime = Enumerable.Min(Data, x => Enumerable.Min(x.Value, y => y.ReviewDate));
			LastUpdateTime = Enumerable.Max(Data, x => Enumerable.Max(x.Value, y => y.ReviewDate));
		}
	}

	private static string? GetLatestPatch()
	{
		using var reader = SqlHelper.ExecuteReader(SqlHandler.ConnectionString, System.Data.CommandType.Text,
			$"SELECT DISTINCT [ReviewedGameVersion] FROM [CS2_Packages] WHERE [ReviewedGameVersion] IS NOT NULL");

		var list = new List<string>();

		while (reader.Read())
		{
			list.Add(reader.GetString(0));
		}

		list.RemoveAll(string.IsNullOrWhiteSpace);
		list.Sort((x, y) => Extensions.ExtensionClass.IsVersionEqualOrHigher(x, y) ? 1 : -1);

		return list.FirstOrDefault();
	}

	private IEnumerable<CompatibilityPackageData> OrderBy(IEnumerable<CompatibilityPackageData> list)
	{
		return list.OrderBy(x => string.Concat(x.Name?.Where(char.IsLetter) ?? []));
	}

	private NotificationType Merge(NotificationType notificationType)
	{
		return notificationType == NotificationType.None ? NotificationType.Info : notificationType;
	}

	private static Dictionary<T2, List<T>> GroupBy<T, T2>(List<T> packageLinks, Func<T, T2> value) where T2 : notnull
	{
		var dictionary = new Dictionary<T2, List<T>>();

		foreach (var packageLink in packageLinks)
		{
			var val = value(packageLink);

			if (dictionary.ContainsKey(val))
			{
				dictionary[val].Add(packageLink);
			}
			else
			{
				dictionary[val] = [packageLink];
			}
		}

		return dictionary;
	}
}
