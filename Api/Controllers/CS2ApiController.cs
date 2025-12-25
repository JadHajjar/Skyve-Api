using Extensions.Sql;

using Microsoft.AspNetCore.Mvc;

using Skyve.Compatibility.Domain;

using SkyveApi.Domain.CS2;
using SkyveApi.Domain.Generic;
using SkyveApi.Utilities;

using System.Data;
using System.Data.SqlClient;

namespace SkyveApi.Controllers;

[ApiController]
[Route("v2/api")]
public class CS2ApiController : ControllerBase
{
	[HttpGet(nameof(Ping))]
	public IActionResult Ping()
	{
		return Ok();
	}

	[HttpGet(nameof(CompatibilityData))]
	public List<CompatibilityPackageData> CompatibilityData()
	{
		var packages = DynamicSql.SqlGet<CompatibilityPackageData>();
		var packageLinks = GroupBy(DynamicSql.SqlGet<PackageLinkData>(), x => x.PackageId);
		var packageStatuses = GroupBy(DynamicSql.SqlGet<PackageStatusData>(), x => x.PackageId);
		var packageInteractions = GroupBy(DynamicSql.SqlGet<PackageInteractionData>(), x => x.PackageId);
		var packageTags = GroupBy(DynamicSql.SqlGet<PackageTagData>(), x => x.PackageId);
		var packageReviews = GroupBy(DynamicSql.SqlGet<ReviewRequestNoLogData>(), x => x.PackageId);

		for (var i = 0; i < packages.Count; i++)
		{
			var id = packages[i].Id;

			if (packageLinks.ContainsKey(id))
			{
				packages[i].Links = [.. packageLinks[id]];
			}

			if (packageStatuses.ContainsKey(id))
			{
				packages[i].Statuses = [.. packageStatuses[id]];
			}

			if (packageInteractions.ContainsKey(id))
			{
				packages[i].Interactions = [.. packageInteractions[id]];
			}

			if (packageTags.ContainsKey(id))
			{
				packages[i].Tags = [.. packageTags[id].Select(x => x.Tag!)];
			}

			if (packageReviews.ContainsKey(id))
			{
				packages[i].ActiveReports = packageReviews[id].Count;
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
			package.Tags = new PackageTagData { PackageId = packageId }.SqlGetByIndex().Select(x => x.Tag).ToList();
			package.ActiveReports = new ReviewRequestNoLogData { PackageId = packageId }.SqlGetByIndex().Count;
		}

		return package;
	}

	[HttpGet(nameof(Blacklist))]
	public Blacklist Blacklist()
	{
		return new Blacklist
		{
			BlackListedIds = DynamicSql.SqlGet<BlackListId>().Select(x => x.Id).ToList(),
			BlackListedNames = DynamicSql.SqlGet<BlackListName>().Select(x => x.Name).ToList(),
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
			return new() { Message = "Payload was empty" };
		}

		if (!TryGetUserId(out var userId))
		{
			return NoAuth();
		}

		var user = DynamicSql.SqlGetById(new UserData { Id = userId });

		if ((package.AuthorId != userId && !(user?.Manager ?? false)) || (user?.Malicious ?? false))
		{
			return NoAuth();
		}

		try
		{
			using var transaction = SqlHandler.CreateTransaction();

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

			if (user?.Manager ?? false)
			{
				var requests = new ReviewRequestData { PackageId = package.Id }.SqlGetByIndex(tr: transaction);

				foreach (var item in requests)
				{
					new ReviewReplyData
					{
						PackageId = package.Id,
						Message = "ReviewIsUpdated",
						Timestamp = DateTime.UtcNow,
						Username = item.UserId
					}.SqlAdd(true, tr: transaction);
				}

				new ReviewRequestData { PackageId = package.Id }.SqlDeleteByIndex(tr: transaction);
			}

			new PackageEditData
			{
				PackageId = package.Id,
				Username = userId,
				EditDate = DateTime.UtcNow,
				Note = package.EditNote
			}.SqlAdd(tr: transaction);

			transaction.Commit();

			return new() { Success = true };
		}
		catch (Exception ex)
		{
			return new() { Message = ex.Message };
		}
	}

	[HttpPost(nameof(BulkUpdatePackageData))]
	public ApiResponse BulkUpdatePackageData([FromBody] BulkCompatibilityPackageUpdateData bulkData)
	{
		if (bulkData is null)
		{
			return new() { Message = "Payload was empty" };
		}

		if (!TryGetUserId(out var userId))
		{
			return NoAuth();
		}

		var user = DynamicSql.SqlGetById(new UserData { Id = userId });

		if (!(user?.Manager ?? false))
		{
			return NoAuth();
		}

		try
		{
			using var transaction = SqlHandler.CreateTransaction();

			SqlHelper.ExecuteNonQuery((SqlTransaction)transaction, CommandType.Text,
				$"UPDATE [CS2_Packages] SET " +
					$"[ReviewDate] = DATEADD(hour, DATEDIFF(hour, 0, DATEADD(minute, 30, SYSUTCDATETIME())), 0), " +
					$"[Stability] = {bulkData.Stability}, " +
					$"[ReviewedGameVersion] = '{bulkData.ReviewedGameVersion?.Replace("'", "''")}' " +
				$"WHERE [Id] IN ({string.Join(',', bulkData.Packages)})");

			foreach (var id in bulkData.Packages)
			{
				new PackageEditData
				{
					PackageId = id,
					Username = userId,
					EditDate = DateTime.UtcNow,
					Note = "Bulk Edit"
				}.SqlAdd(tr: transaction);
			}

			transaction.Commit();

			return new() { Success = true };
		}
		catch (Exception ex)
		{
			return new() { Message = ex.Message };
		}
	}

	[HttpPost(nameof(RequestReview))]
	public ApiResponse RequestReview([FromBody] ReviewRequestData request)
	{
		try
		{
			if (!TryGetUserId(out var userId)
				|| string.IsNullOrEmpty(userId)
				|| DynamicSql.SqlGetById(new UserData { Id = userId })?.LockedAccess == true)
			{
				return NoAuth();
			}

			request.UserId = userId;
			request.Timestamp = DateTime.UtcNow;
			request.SqlAdd(true);

			new ReviewReplyData { Username = userId, PackageId = request.PackageId }.SqlDeleteOne();

			return new() { Success = true };
		}
		catch (Exception ex)
		{
			return new() { Message = ex.Message };
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

		DynamicSql.SqlDelete<ReviewRequestNoLogData>("[Timestamp] < DATEADD(DAY, -14, GETUTCDATE())");

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

	[HttpGet(nameof(GetPackageEdits))]
	public List<PackageEditData> GetPackageEdits(ulong packageId)
	{
		if (!TryGetUserId(out var senderId))
		{
			return [];
		}

		var user = new UserData { Id = senderId }.SqlGetById();

		if (user is null || !user.Manager)
		{
			return [];
		}

		return new PackageEditData { PackageId = packageId }.SqlGetByIndex();
	}

	[HttpPost(nameof(ProcessReviewRequest))]
	public ApiResponse ProcessReviewRequest([FromBody] ReviewRequestData request)
	{
		try
		{
			if (!TryGetUserId(out var userId))
			{
				return NoAuth();
			}

			var user = new UserData { Id = userId }.SqlGetById();

			if (user is null || !user.Manager)
			{
				return NoAuth();
			}

			request.SqlDeleteOne();

			return new() { Success = true };
		}
		catch (Exception ex)
		{
			return new() { Message = ex.Message };
		}
	}

	[HttpGet(nameof(Announcements))]
	public List<AnnouncementData> Announcements()
	{
		if (!TryGetUserId(out var userId))
		{
			return [];
		}

		return DynamicSql.SqlGet<AnnouncementData>($"[{nameof(AnnouncementData.EndDate)}] IS NULL OR [{nameof(AnnouncementData.EndDate)}] > GETDATE()");
	}

	[HttpPost(nameof(CreateAnnouncement))]
	public ApiResponse CreateAnnouncement([FromBody] AnnouncementData announcement)
	{
		if (!TryGetUserId(out var senderId) || string.IsNullOrEmpty(senderId))
		{
			return NoAuth();
		}

		if (announcement is null)
		{
			return new() { Message = "Payload was empty" };
		}

		var user = new UserData { Id = senderId }.SqlGetById();

		if (user is null || !user.Manager)
		{
			return NoAuth();
		}

		try
		{
			announcement.SqlAdd();

			return new ApiResponse { Success = true };
		}
		catch (Exception ex)
		{
			return new() { Message = ex.Message };
		}
	}

	[HttpGet(nameof(GetReviewMessages))]
	public List<ReviewReplyData> GetReviewMessages()
	{
		if (!TryGetUserId(out var senderId))
		{
			return [];
		}

		return new ReviewReplyData { Username = senderId }.SqlGetByIndex();
	}

	[HttpGet(nameof(GetReviewStatus))]
	public ReviewReplyData? GetReviewStatus(ulong packageId)
	{
		if (!TryGetUserId(out var senderId))
		{
			return null;
		}

		var reply = new ReviewReplyData { Username = senderId, PackageId = packageId }.SqlGetById();

		if (reply is not null)
		{
			return reply;
		}

		var request = new ReviewRequestData { PackageId = packageId, UserId = senderId }.SqlGetById();

		if (request is not null)
		{
			return new ReviewReplyData
			{
				Message = "ReviewPending",
				PackageId = packageId,
				Timestamp = request.Timestamp,
			};
		}

		return null;
	}

	[HttpPost(nameof(SendReviewMessage))]
	public ApiResponse SendReviewMessage([FromBody] ReviewReplyData reply)
	{
		if (!TryGetUserId(out var senderId) || string.IsNullOrEmpty(senderId))
		{
			return NoAuth();
		}

		if (reply is null)
		{
			return new() { Message = "Payload was empty" };
		}

		var user = new UserData { Id = senderId }.SqlGetById();

		if (user is null || !user.Manager)
		{
			return NoAuth();
		}

		reply.Timestamp = DateTime.UtcNow;

		try
		{
			reply.SqlAdd(true);

			return new ApiResponse { Success = true };
		}
		catch (Exception ex)
		{
			return new() { Message = ex.Message };
		}
	}

	[HttpDelete(nameof(DeleteReviewMessage))]
	public ApiResponse DeleteReviewMessage(ulong packageId)
	{
		if (!TryGetUserId(out var senderId) || string.IsNullOrEmpty(senderId))
		{
			return NoAuth();
		}

		try
		{
			new ReviewReplyData { Username = senderId, PackageId = packageId }.SqlDeleteOne();

			return new ApiResponse { Success = true };
		}
		catch (Exception ex)
		{
			return new() { Message = ex.Message };
		}
	}

	[HttpGet(nameof(GetGoFileUploadInfo))]
	public async Task<GoFileUploadData> GetGoFileUploadInfo(string folderName)
	{
		if (!TryGetUserId(out var userId))
		{
			return new();
		}

		var apiInfo = DynamicSql.SqlGetOne<GoFileInfoData>();
		var folder = await GoFileHelper.CreateFolder(apiInfo.Token!, apiInfo.RootFolder!, folderName);
		var server = await GoFileHelper.GetServer();

		return new GoFileUploadData
		{
			Token = apiInfo.Token,
			ServerId = server,
			FolderId = folder
		};
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

	private ApiResponse NoAuth()
	{
		return new() { Message = "Unauthorized" };
	}
}
