using Extensions.Sql;

using Microsoft.AspNetCore.Mvc;

using SkyveApi.Domain;
using SkyveApi.Utilities;

using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Compatibility.Api;

using System.Data;
using System.Data.SqlClient;

namespace SkyveApi.Controllers;

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
		var packages = DynamicSql.SqlGet<CrPackage>();
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

	[HttpGet("Package")]
	public CompatibilityData GetPackage(ulong steamId)
	{
		var data = new CompatibilityData();

		var blackListIds = DynamicSql.SqlGet<BlackListId>();
		var blackListNames = DynamicSql.SqlGet<BlackListName>();

		data.BlackListedIds = new(blackListIds.Select(x => x.SteamId));
		data.BlackListedNames = new(blackListNames.Select(x => x.Name!));

		var package = new CrPackage { SteamId = steamId }.SqlGetById();

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

			new ReviewRequest { PackageId = package.SteamId }.SqlDeleteByIndex(tr: transaction);

			transaction.Commit();

			return new() { Success = true, Message = "Success" };
		}
		catch (Exception ex)
		{
			((SqlTransaction)transaction).Rollback();

			return new() { Success = false, Message = ex.Message };
		}
	}

	[HttpGet(nameof(Translations))]
	public Dictionary<string, string?> Translations()
	{
		var notes = DynamicSql.SqlGet<CrPackage>($"[{nameof(CrPackage.Note)}] IS NOT NULL AND [{nameof(CrPackage.Note)}] <> ''");
		var interactions = DynamicSql.SqlGet<PackageInteraction>($"[{nameof(PackageInteraction.Note)}] IS NOT NULL AND [{nameof(PackageInteraction.Note)}] <> ''");
		var statuses = DynamicSql.SqlGet<PackageStatus>($"[{nameof(PackageStatus.Note)}] IS NOT NULL AND [{nameof(PackageStatus.Note)}] <> ''");

		var dictionary = new Dictionary<string, string?>();

		foreach (var item in notes)
		{
			dictionary[item.Note!] = item.Note;
		}

		foreach (var item in interactions)
		{
			dictionary[item.Note!] = item.Note;
		}

		foreach (var item in statuses)
		{
			dictionary[item.Note!] = item.Note;
		}

		return dictionary;
	}

	[HttpPost(nameof(RequestReview))]
	public ApiResponse RequestReview([FromBody] ReviewRequest request)
	{
		try
		{
			if (!Request.Headers.TryGetValue("USER_ID", out var userId))
			{
				return new() { Success = false, Message = "Unauthorized" };
			}

			request.UserId = ulong.Parse(Encryption.Decrypt(userId.ToString(), KEYS.SALT));
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
	public List<ReviewRequestNoLog> GetReviewRequests()
	{
		if (!Request.Headers.TryGetValue("USER_ID", out var userId))
		{
			return new();
		}

		var manager = DynamicSql.SqlGetById(new Manager { SteamId = ulong.Parse(Encryption.Decrypt(userId.ToString(), KEYS.SALT)) });

		if (manager is null)
		{
			return new();
		}

		return DynamicSql.SqlGet<ReviewRequestNoLog>();
	}

	[HttpGet(nameof(GetReviewRequest))]
	public ReviewRequest GetReviewRequest(ulong userId, ulong packageId)
	{
		if (!Request.Headers.TryGetValue("USER_ID", out var senderId))
		{
			return new();
		}

		var manager = DynamicSql.SqlGetById(new Manager { SteamId = ulong.Parse(Encryption.Decrypt(senderId.ToString(), KEYS.SALT)) });

		if (manager is null)
		{
			return new();
		}

		return new ReviewRequest { UserId = userId, PackageId = packageId }.SqlGetById();
	}

	[HttpPost(nameof(ProcessReviewRequest))]
	public ApiResponse ProcessReviewRequest([FromBody] ReviewRequest request)
	{
		try
		{
			if (!Request.Headers.TryGetValue("USER_ID", out var userId))
			{
				return new() { Success = false, Message = "Unauthorized" };
			}

			var manager = DynamicSql.SqlGetById(new Manager { SteamId = ulong.Parse(Encryption.Decrypt(userId.ToString(), KEYS.SALT)) });

			if (manager is null)
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

	[HttpGet(nameof(GetUserProfiles))]
	public List<UserProfile> GetUserProfiles(ulong userId)
	{
		if (!Request.Headers.TryGetValue("USER_ID", out var senderId))
		{
			return new();
		}

		var onlyPublicProfiles = userId != ulong.Parse(Encryption.Decrypt(senderId.ToString(), KEYS.SALT));

		var profiles = new UserProfile { Author = userId }.SqlGetByIndex(onlyPublicProfiles ? $"[{nameof(UserProfile.Public)}] = 1" : null);

		return profiles;
	}

	[HttpGet(nameof(GetPublicProfiles))]
	public List<UserProfile> GetPublicProfiles()
	{
		if (!Request.Headers.TryGetValue("USER_ID", out var senderId))
		{
			return new();
		}

		var profiles = DynamicSql.SqlGet<UserProfile>($"[{nameof(UserProfile.Public)}] = 1");

		return profiles;
	}

	[HttpDelete(nameof(DeleteUserProfile))]
	public ApiResponse DeleteUserProfile(int profileId)
	{
		if (!Request.Headers.TryGetValue("USER_ID", out var senderId))
		{
			return new() { Success = false, Message = "Unauthorized" };
		}

		try
		{
			var userIdVal = ulong.Parse(Encryption.Decrypt(senderId.ToString(), KEYS.SALT));
			var currentProfile = new UserProfile { ProfileId = profileId }.SqlGetById();

			if (currentProfile != null && currentProfile.Author != userIdVal)
			{
				return new() { Success = false, Message = "Unauthorized" };
			}

			new UserProfile { ProfileId = profileId }.SqlDeleteOne();
		}
		catch (Exception ex)
		{
			return new() { Success = false, Message = ex.Message };
		}

		return new() { Success = true };
	}

	[HttpGet(nameof(GetUserProfileContents))]
	public UserProfile? GetUserProfileContents(int profileId)
	{
		if (!Request.Headers.TryGetValue("USER_ID", out var senderId))
		{
			return null;
		}

		var profile = new UserProfile { ProfileId = profileId }.SqlGetById();

		if (profile == null || (!profile.Public && profile.Author != ulong.Parse(Encryption.Decrypt(senderId.ToString(), KEYS.SALT))))
		{
			return null;
		}

		profile.Downloads++;

		profile.SqlUpdate();

		profile.Contents = new UserProfileContent { ProfileId = profileId }.SqlGetByIndex().ToArray();

		return profile;
	}

	[HttpGet(nameof(GetUserProfileByLink))]
	public UserProfile? GetUserProfileByLink(string link)
	{
		if (!Request.Headers.TryGetValue("USER_ID", out var senderId))
		{
			return null;
		}

		int profileId;

		try
		{
			profileId = IdHasher.ShortStringToHash(link);
		}
		catch { return null; }

		var profile = new UserProfile { ProfileId = profileId }.SqlGetById();

		if (profile == null)
		{
			return null;
		}

		profile.Downloads++;

		profile.SqlUpdate();

		profile.Contents = new UserProfileContent { ProfileId = profileId }.SqlGetByIndex().ToArray();

		return profile;
	}

	[HttpPost(nameof(SaveUserProfile))]
	public ApiResponse SaveUserProfile([FromBody] UserProfile profile)
	{
		try
		{
			if (!Request.Headers.TryGetValue("USER_ID", out var userId))
			{
				return new() { Success = false, Message = "Unauthorized" };
			}

			var userIdVal = ulong.Parse(Encryption.Decrypt(userId.ToString(), KEYS.SALT));

			if (userIdVal == 0)
			{
				return new() { Success = false, Message = "Unauthorized" };
			}

			var currentProfile = new UserProfile { ProfileId = profile.ProfileId }.SqlGetById();

			if (currentProfile != null && currentProfile.Author != userIdVal)
			{
				return new() { Success = false, Message = "Unauthorized" };
			}

			if (!(profile.Contents?.Any() ?? false))
			{
				return new() { Success = false, Message = "Nothing to save" };
			}

			using var transaction = SqlHandler.CreateTransaction();

			profile.Author = userIdVal;
			profile.DateUpdated = DateTime.UtcNow;
			profile.ModCount = profile.Contents.Count(x => x.IsMod);
			profile.AssetCount = profile.Contents.Length - profile.ModCount;
			profile.Banner ??= Array.Empty<byte>();

			if (currentProfile is null)
			{
				profile.ProfileId = 0;
				profile.DateCreated = DateTime.UtcNow;
			}
			else
			{
				profile.Downloads = currentProfile.Downloads;
				profile.DateCreated = currentProfile.DateCreated;
				profile.Public = currentProfile.Public;
			}

			if (profile.ProfileId == 0)
			{
				var currentProfiles = (int)SqlHelper.ExecuteScalar((SqlTransaction)transaction, CommandType.Text, $"SELECT COUNT(*) FROM [UserProfiles] WHERE [AuthorId] = {profile.Author}");

				if (currentProfiles >= 5)
				{
					return new() { Success = false, Message = "Limit Exceeded" };
				}

				profile.ProfileId = (int)(decimal)profile.SqlAdd(false, tr: transaction);
			}
			else
			{
				profile.SqlUpdate(tr: transaction);

				new UserProfileContent { ProfileId = profile.ProfileId }.SqlDeleteByIndex(tr: transaction);
			}

			foreach (var item in profile.Contents)
			{
				item.ProfileId = profile.ProfileId;

				item.SqlAdd(false, tr: transaction);
			}

			transaction.Commit();

			return new() { Success = true, Data = profile.ProfileId };
		}
		catch (Exception ex)
		{
			return new() { Success = false, Message = ex.Message };
		}
	}

	[HttpPost(nameof(SetProfileVisibility))]
	public ApiResponse SetProfileVisibility(int profileId, [FromBody] bool visible)
	{
		try
		{
			if (!Request.Headers.TryGetValue("USER_ID", out var userId))
			{
				return new() { Success = false, Message = "Unauthorized" };
			}

			var userIdVal = ulong.Parse(Encryption.Decrypt(userId.ToString(), KEYS.SALT));
			var currentProfile = new UserProfile { ProfileId = profileId }.SqlGetById();

			if (currentProfile == null || currentProfile.Author != userIdVal)
			{
				return new() { Success = false, Message = "Unauthorized" };
			}

			currentProfile.Public = visible;
			currentProfile.SqlUpdate();

			return new() { Success = true };
		}
		catch (Exception ex)
		{
			return new() { Success = false, Message = ex.Message };
		}
	}
}
