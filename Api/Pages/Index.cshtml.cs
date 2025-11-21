using Extensions.Sql;

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SkyveApi.Pages;

public class IndexModel : PageModel
{
	public string? LatestPatch { get; private set; }
	public bool ShowPatch { get; private set; }

	public void OnGet()
	{
		LatestPatch = GetLatestPatch();
		ShowPatch = (DateTime)SqlHelper.ExecuteScalar(SqlHandler.ConnectionString, System.Data.CommandType.Text, 
			$"SELECT MIN([ReviewDate]) FROM [CS2_Packages] WHERE [ReviewedGameVersion] = '{LatestPatch}'") >= DateTime.UtcNow.AddDays(-15);
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
}
