using ApiApplication.Domain;

using Extensions.Sql;

using LoadOrderToolTwo.Domain.Compatibility;

using Microsoft.AspNetCore.Mvc;

using System.Data.SqlClient;

namespace ApiApplication.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase
{
	[HttpGet(nameof(IsCommunityManager))]
	public bool IsCommunityManager()
	{
		if (!Request.Headers.TryGetValue("USER_ID", out var userId))
		{
			return false;
		}

		return DynamicSql.SqlGetById(new Manager { SteamId = ulong.Parse(Encryption.Decrypt(userId.ToString(), KEYS.SALT)) }) is not null;
	}

	[HttpGet(nameof(Catalogue))]
	public CompatibilityData Catalogue()
	{
		var data = new CompatibilityData();

		var blackListIds = DynamicSql.SqlGet<BlackListId>();
		var blackListNames = DynamicSql.SqlGet<BlackListName>();
		var packages = DynamicSql.SqlGet<Package>();
		var authors = DynamicSql.SqlGet<Author>();
		var packageLinks = DynamicSql.SqlGet<PackageLink>().GroupBy(x => x.PackageId).ToDictionary(x => x.Key);
		var packageStatuses = DynamicSql.SqlGet<PackageStatus>().GroupBy(x => x.PackageId).ToDictionary(x => x.Key);
		var packageInteractions = DynamicSql.SqlGet<PackageInteraction>().GroupBy(x => x.PackageId).ToDictionary(x => x.Key);
		var packageTags = DynamicSql.SqlGet<PackageTag>().GroupBy(x => x.PackageId).ToDictionary(x => x.Key);

		for (var i = 0; i < packages.Count; i++)
		{
			var id = packages[i].SteamId;

			if (packageLinks.ContainsKey(id))
			{
				packages[i].Links = new(packageLinks[id]);
			}

			if (packageStatuses.ContainsKey(id))
			{
				packages[i].Statuses = new(packageStatuses[id]);
			}

			if (packageInteractions.ContainsKey(id))
			{
				packages[i].Interactions = new(packageInteractions[id]);
			}

			if (packageTags.ContainsKey(id))
			{
				packages[i].Tags = new(packageTags[id].Select(x => x.Tag!));
			}
		}

		data.BlackListedIds = new(blackListIds.Select(x => x.SteamId));
		data.BlackListedNames = new(blackListNames.Select(x => x.Name!));
		data.Packages = packages;
		data.Authors = authors;

		return data;
	}

	[HttpGet(nameof(Package))]
	public CompatibilityData Package(ulong steamId)
	{
		var data = new CompatibilityData();

		var blackListIds = DynamicSql.SqlGet<BlackListId>();
		var blackListNames = DynamicSql.SqlGet<BlackListName>();

		data.BlackListedIds = new(blackListIds.Select(x => x.SteamId));
		data.BlackListedNames = new(blackListNames.Select(x => x.Name!));

		var package = new Package { SteamId = steamId }.SqlGetById();

		if (package is null)
		{
			data.Packages = new();
			data.Authors = new();

			return data;
		}

		var author = new Author { SteamId = package.SteamId }.SqlGetById();
		var packageLinks = new PackageLink { PackageId = steamId }.SqlGetByIndex();
		var packageStatuses = new PackageStatus { PackageId = steamId }.SqlGetByIndex();
		var packageInteractions = new PackageInteraction { PackageId = steamId }.SqlGetByIndex();
		var packageTags = new PackageTag { PackageId = steamId }.SqlGetByIndex();

		package.Links = packageLinks;
		package.Statuses = packageStatuses;
		package.Interactions = packageInteractions;
		package.Tags = packageTags.Select(x => x.Tag!).ToList();

		data.Packages = new() { package };
		data.Authors = new() { author };

		return data;
	}

	[HttpPost(nameof(SaveEntry))]
	public ApiResponse SaveEntry([FromBody] PostPackage package)
	{
		if (package is null)
		{
			return new() { Success = false, Message = "Package was empty" };
		}

		if (!Request.Headers.TryGetValue("USER_ID", out var userId))
		{
			return new() { Success = false, Message = "Unauthorized" };
		}

		var manager = DynamicSql.SqlGetById(new Manager { SteamId = ulong.Parse(Encryption.Decrypt(userId.ToString(), KEYS.SALT)) });
		var user = DynamicSql.SqlGetById(new Author { SteamId = ulong.Parse(Encryption.Decrypt(userId.ToString(), KEYS.SALT)) });

		if ((package.AuthorId.ToString() != userId.ToString() && manager is null) || (user?.Malicious ?? false))
		{
			return new() { Success = false, Message = "Unauthorized" };
		}

		using var transaction = SqlHandler.CreateTransaction();

		try
		{
			var currentAuthor = new Author { SteamId = package.AuthorId }.SqlGetById(tr: transaction);

			if (currentAuthor is null && package.Author is null)
			{
				return new() { Success = false, Message = "Author was not provided" };
			}

			if (currentAuthor is null)
			{
				package.Author!.SqlAdd(true, transaction);
			}

			package.SqlAdd(true, transaction);

			if (package.BlackListId)
			{
				new BlackListId { SteamId = package.SteamId }.SqlAdd(tr: transaction);
			}
			else
			{
				new BlackListId { SteamId = package.SteamId }.SqlDeleteOne(tr: transaction);
			}

			if (package.BlackListName)
			{
				new BlackListName { Name = package.Name }.SqlAdd(tr: transaction);
			}
			else
			{
				new BlackListName { Name = package.Name }.SqlDeleteOne(tr: transaction);
			}

			new PackageStatus { PackageId = package.SteamId }.SqlDeleteByIndex(tr: transaction);
			new PackageInteraction { PackageId = package.SteamId }.SqlDeleteByIndex(tr: transaction);
			new PackageTag { PackageId = package.SteamId }.SqlDeleteByIndex(tr: transaction);
			new PackageLink { PackageId = package.SteamId }.SqlDeleteByIndex(tr: transaction);

			if (package.Statuses is not null)
			{
				foreach (var item in package.Statuses)
				{
					item.PackageId = package.SteamId;
					item.SqlAdd(true, transaction);
				}
			}

			if (package.Interactions is not null)
			{
				foreach (var item in package.Interactions)
				{
					item.PackageId = package.SteamId;
					item.SqlAdd(true, transaction);
				}
			}

			if (package.Links is not null)
			{
				foreach (var item in package.Links)
				{
					item.PackageId = package.SteamId;
					item.SqlAdd(true, transaction);
				}
			}

			if (package.Tags is not null)
			{
				foreach (var item in package.Tags)
				{
					new PackageTag { PackageId = package.SteamId, Tag = item }.SqlAdd(true, transaction);
				}
			}

			((SqlTransaction)transaction).Commit();

			return new() { Success = true, Message = "Success" };
		}
		catch (Exception ex)
		{
			((SqlTransaction)transaction).Rollback();

			return new() { Success = false, Message = ex.Message };
		}
	}
}
