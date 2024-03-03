using Extensions.Sql;

namespace SkyveApi.Domain.CS1;

[DynamicSqlClass("CS1_UserProfileContents")]
public class UserProfileContent : IDynamicSql
{
	[DynamicSqlProperty(Indexer = true)]
	public int ProfileId { get; set; }
	[DynamicSqlProperty]
	public string? RelativePath { get; set; }
	[DynamicSqlProperty]
	public ulong SteamId { get; set; }
	[DynamicSqlProperty]
	public bool IsMod { get; set; }
	[DynamicSqlProperty]
	public bool Enabled { get; set; }
}