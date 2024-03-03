using Extensions;
using Extensions.Sql;

using Microsoft.AspNetCore.Mvc;

using Skyve.Compatibility.Domain;

using SkyveApi.Domain.CS2;
using SkyveApi.Domain.Generic;

using System.Data;
using System.Data.SqlClient;

namespace SkyveApi.Controllers;

[ApiController]
[Route("v2/api")]
public class CS2ApiController : ControllerBase
{
	[HttpGet(nameof(CompatibilityData))]
	public List<CompatibilityPackageData> CompatibilityData()
	{
		var packages = DynamicSql.SqlGet<CompatibilityPackageData>();
		var packageLinks = GroupBy(DynamicSql.SqlGet<PackageLinkData>(), x => x.PackageId);
		var packageStatuses = GroupBy(DynamicSql.SqlGet<PackageStatusData>(), x => x.PackageId);
		var packageInteractions = GroupBy(DynamicSql.SqlGet<PackageInteractionData>(), x => x.PackageId);
		var packageTags = GroupBy(DynamicSql.SqlGet<PackageTagData>(), x => x.PackageId);

		for (var i = 0; i < packages.Count; i++)
		{
			var id = packages[i].Id;

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

		return packages;
	}

	[HttpGet($"{nameof(CompatibilityData)}/{{{nameof(packageId)}}}")]
	public CompatibilityPackageData? CompatibilityData(ulong packageId)
	{
		var package = new CompatibilityPackageData { Id = packageId }.SqlGetById();

		if (package != null)
		{
			package.Links = new PackageLinkData { PackageId = packageId }.SqlGetByIndex();
			package.Statuses = new PackageStatusData { PackageId = packageId }.SqlGetByIndex();
			package.Interactions = new PackageInteractionData { PackageId = packageId }.SqlGetByIndex();
			package.Tags = new PackageTagData { PackageId = packageId }.SqlGetByIndex().ToList(x => x.Tag);
		}

		return package;
	}

	[HttpGet(nameof(Blacklist))]
	public Blacklist Blacklist()
	{
		return new Blacklist
		{
			BlackListedIds = DynamicSql.SqlGet<BlackListId>().ToList(x => x.Id),
			BlackListedNames = DynamicSql.SqlGet<BlackListName>().ToList(x => x.Name),
		};
	}

	[HttpGet(nameof(Users))]
	public List<UserData> Users()
	{
		return DynamicSql.SqlGet<UserData>();
	}

	[HttpPost(nameof(UpdatePackageData))]
	public ApiResponse UpdatePackageData([FromBody] PostPackage package)
	{
		if (package is null)
		{
			return new() { Success = false, Message = "Package was empty" };
		}

		if (!TryGetUserId(out var userId))
		{
			return new() { Success = false, Message = "Unauthorized" };
		}

		var user = DynamicSql.SqlGetById(new UserData { Id = userId });

		if ((package.AuthorId != userId && !(user?.Manager ?? false)) || (user?.Malicious ?? false))
		{
			return new() { Success = false, Message = "Unauthorized" };
		}

		using var transaction = SqlHandler.CreateTransaction();

		try
		{
			package.SqlAdd(true, transaction);

			if (package.BlackListId)
			{
				new BlackListId { Id = package.Id }.SqlAdd(tr: transaction);
			}
			else
			{
				new BlackListId { Id = package.Id }.SqlDeleteOne(tr: transaction);
			}

			if (package.BlackListName)
			{
				new BlackListName { Name = package.Name ?? string.Empty }.SqlAdd(tr: transaction);
			}
			else
			{
				new BlackListName { Name = package.Name ?? string.Empty }.SqlDeleteOne(tr: transaction);
			}

			new PackageStatusData { PackageId = package.Id }.SqlDeleteByIndex(tr: transaction);
			new PackageInteractionData { PackageId = package.Id }.SqlDeleteByIndex(tr: transaction);
			new PackageTagData { PackageId = package.Id }.SqlDeleteByIndex(tr: transaction);
			new PackageLinkData { PackageId = package.Id }.SqlDeleteByIndex(tr: transaction);

			if (package.Statuses is not null)
			{
				foreach (var item in package.Statuses)
				{
					item.PackageId = package.Id;
					item.SqlAdd(true, transaction);
				}
			}

			if (package.Interactions is not null)
			{
				foreach (var item in package.Interactions)
				{
					item.PackageId = package.Id;
					item.SqlAdd(true, transaction);
				}
			}

			if (package.Links is not null)
			{
				foreach (var item in package.Links)
				{
					item.PackageId = package.Id;
					item.SqlAdd(true, transaction);
				}
			}

			if (package.Tags is not null)
			{
				foreach (var item in package.Tags)
				{
					new PackageTagData { PackageId = package.Id, Tag = item }.SqlAdd(true, transaction);
				}
			}

			new ReviewRequestData { PackageId = package.Id }.SqlDeleteByIndex(tr: transaction);

			transaction.Commit();

			return new() { Success = true, Message = "Success" };
		}
		catch (Exception ex)
		{
			((SqlTransaction)transaction).Rollback();

			return new() { Success = false, Message = ex.Message };
		}
	}

	[HttpPost(nameof(RequestReview))]
	public ApiResponse RequestReview([FromBody] ReviewRequestData request)
	{
		try
		{
			if (!TryGetUserId(out var userId))
			{
				return new() { Success = false, Message = "Unauthorized" };
			}

			request.UserId = userId;
			request.Timestamp = DateTime.UtcNow;
			request.SqlAdd(true);

			return new() { Success = true };
		}
		catch (Exception ex)
		{
			return new() { Success = false, Message = ex.Message };
		}
	}

	[HttpGet(nameof(GetReviewRequests))]
	public List<ReviewRequestNoLogData> GetReviewRequests()
	{
		if (!TryGetUserId(out var userId))
		{
			return [];
		}

		var user = new UserData { Id = userId }.SqlGetById();

		if (user is null || !user.Manager)
		{
			return [];
		}

		return DynamicSql.SqlGet<ReviewRequestNoLogData>();
	}

	[HttpGet(nameof(GetReviewRequest))]
	public ReviewRequestData GetReviewRequest(string userId, ulong packageId)
	{
		if (!TryGetUserId(out var senderId))
		{
			return new();
		}

		var user = new UserData { Id = senderId }.SqlGetById();

		if (user is null || !user.Manager)
		{
			return new();
		}

		return new ReviewRequestData { UserId = userId, PackageId = packageId }.SqlGetById();
	}

	[HttpPost(nameof(ProcessReviewRequest))]
	public ApiResponse ProcessReviewRequest([FromBody] ReviewRequestData request)
	{
		try
		{
			if (!TryGetUserId(out var userId))
			{
				return new() { Success = false, Message = "Unauthorized" };
			}

			var user = new UserData { Id = userId }.SqlGetById();

			if (user is null || !user.Manager)
			{
				return new() { Success = false, Message = "Unauthorized" };
			}

			request.SqlDeleteOne();

			return new() { Success = true };
		}
		catch (Exception ex)
		{
			return new() { Success = false, Message = ex.Message };
		}
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

	private bool TryGetUserId(out string userId)
	{
		if (Request.Headers.TryGetValue("USER_ID", out var userid) && !string.IsNullOrEmpty(userid.ToString()))
		{
			try
			{
				userId = Encryption.Decrypt(userid.ToString(), KEYS.SALT);

				return true;
			}
			catch { }
		}

		userId = string.Empty;
		return false;
	}
}
