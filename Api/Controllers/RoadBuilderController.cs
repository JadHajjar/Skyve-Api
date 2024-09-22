using Extensions.Sql;

using Microsoft.AspNetCore.Mvc;

using Skyve.Compatibility.Domain;

using SkyveApi.Domain.Generic;
using SkyveApi.Domain.RoadBuilder;

using System.Data.SqlClient;

namespace SkyveApi.Controllers;

[ApiController]
[Route("rbapi")]
public class RoadBuilderController : ControllerBase
{
	[HttpPost(nameof(SaveRoad))]
	public ApiResponse SaveRoad([FromBody] RoadBuilderEntryPost payload)
	{
		if (payload?.ID is null)
		{
			return new() { Message = "Payload was empty" };
		}

		if (!Request.Headers.TryGetValue("USER_ID", out var userid) || string.IsNullOrWhiteSpace(userid.ToString()))
		{
			return new() { Message = "Unauthorized" };
		}

		if (!Request.Headers.TryGetValue("IDENTIFIER", out var identifier) || string.IsNullOrWhiteSpace(identifier.ToString()))
		{
			return new() { Message = "Unauthorized" };
		}

		if (!payload.ID.EndsWith("-" + identifier))
		{
			return new() { Message = "Unauthorized" };
		}

		try
		{
			using var transaction = SqlHandler.CreateTransaction();

			var entry = new RoadBuilderEntry { ID = userid }.SqlGetById() ?? new() { UploadTime = DateTime.UtcNow };

			entry.ID = payload.ID;
			entry.Name = payload.Name;
			entry.Tags = payload.Tags ?? string.Empty;
			entry.Category = payload.Category;
			entry.Icon = payload.Icon;
			entry.Author = userid;

			entry.SqlAdd(true, transaction);

			new RoadBuilderConfig
			{
				ID = payload.ID,
				Payload = payload.Config
			}.SqlAdd(true, transaction);

			transaction.Commit();
		}
		catch (Exception ex)
		{
			return new() { Message = ex.Message };
		}

		return new() { Success = true };
	}

	[HttpGet(nameof(Roads))]
	public PagedContent<RoadBuilderEntry> Roads(string? query = null, int? category = null, int order = 0, int page = 1)
	{
		var conditions = new List<string>();

		if (query != null)
		{
			conditions.AddRange(query.Split([' '], StringSplitOptions.RemoveEmptyEntries).Select(x => $"([Name] LIKE '%{x}%' OR [Author] LIKE '%{x}%')"));
			//conditions.AddRange(query.Split([' '], StringSplitOptions.RemoveEmptyEntries).Select(x => $"([Name] LIKE '%{x}%' OR [Author] LIKE '%{x}%' OR [Tags] LIKE '%{x}%')"));
		}

		if (category != null)
		{
			conditions.Add($"[Category] = {category.Value}");
		}

		var condition = conditions.Count == 0 ? null : string.Join(" AND ", conditions);

		return new()
		{
			Page = page,
			PageSize = 14,
			TotalPages = (int)Math.Ceiling(DynamicSql.SqlCount<RoadBuilderEntry>(condition) / 14d),
			Items = DynamicSql.SqlGet<RoadBuilderEntry>(
			condition: condition,
			pagination: new Pagination
			{
				PageNumber = page,
				PageSize = 14,
				OrderBy = order switch
				{
					5 => "[UploadTime] ASC",
					4 => "[UploadTime] DESC",
					3 => "[Downloads] ASC, [UploadTime] ASC",
					2 => "[Downloads] DESC, [UploadTime] DESC",
					1 => "(200 * [Downloads]) / (DATEDIFF(day, [UploadTime], GETDATE()) + 10) ASC, [UploadTime] ASC",
					_ => "(200 * [Downloads]) / (DATEDIFF(day, [UploadTime], GETDATE()) + 10) DESC, [UploadTime] DESC",
				}
			})
		};
	}

	[HttpGet(nameof(RoadIcon) + "/{id}.svg")]
	public IActionResult RoadIcon(string id)
	{
		var icon = new RoadBuilderEntry { ID = id }.SqlGetById();

		if (icon is null)
		{
			return NotFound();
		}

		return Ok(icon.Icon);
	}

	[HttpGet(nameof(RoadConfig) + "/{id}.json")]
	public IActionResult RoadConfig(string id)
	{
		var config = new RoadBuilderConfig { ID = id }.SqlGetById();

		if (config is null)
		{
			return NotFound();
		}

		SqlHelper.ExecuteNonQuery(SqlHandler.ConnectionString, System.Data.CommandType.Text, "UPDATE [RB_Roads] SET [Downloads] = [Downloads] + 1 WHERE [ID] = @id", new SqlParameter("@id", id));

		return Ok(config.Payload);
	}
}
