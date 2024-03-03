using Extensions.Sql;

using System;

namespace SkyveApi.Domain.CS1;

[DynamicSqlClass("CS1_SteamUsers")]
public class SteamUser : IDynamicSql
{
	public SteamUser(SteamUserEntry entry)
	{
		SteamId = ulong.Parse(entry.steamid);
		Name = entry.personaname;
		ProfileUrl = entry.profileurl;
		AvatarUrl = entry.avatarfull;
		Timestamp = DateTime.Now;
	}

	public SteamUser()
	{
		Name = string.Empty;
		ProfileUrl = string.Empty;
		AvatarUrl = string.Empty;
	}

	[DynamicSqlProperty(PrimaryKey = true)]
	public ulong SteamId { get; set; }
	[DynamicSqlProperty]
	public string Name { get; set; }
	[DynamicSqlProperty]
	public string ProfileUrl { get; set; }
	[DynamicSqlProperty]
	public string AvatarUrl { get; set; }
	[DynamicSqlProperty]
	public DateTime Timestamp { get; set; }
}
