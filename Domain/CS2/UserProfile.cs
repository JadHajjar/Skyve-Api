using Extensions.Sql;

using System;

namespace SkyveApi.Domain.CS2;

[DynamicSqlClass("UserProfiles")]
public class UserProfile : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true, Identity = true)]
	public int ProfileId { get; set; }
	[DynamicSqlProperty(Indexer = true, ColumnName = "AuthorId")]
	public ulong Author { get; set; }
	[DynamicSqlProperty]
	public string? Name { get; set; }
	[DynamicSqlProperty]
	public int ModCount { get; set; }
	[DynamicSqlProperty]
	public int AssetCount { get; set; }
	[DynamicSqlProperty]
	public DateTime DateCreated { get; set; }
	[DynamicSqlProperty]
	public DateTime DateUpdated { get; set; }
	[DynamicSqlProperty]
	public bool Public { get; set; }
	[DynamicSqlProperty]
	public byte[]? Banner { get; set; }
	[DynamicSqlProperty]
	public int? Color { get; set; }
	[DynamicSqlProperty]
	public int Downloads { get; set; }
	[DynamicSqlProperty(ColumnName = "Usage")]
	public int? ProfileUsage { get; set; }

	public UserProfileContent[]? Contents { get; set; }
}
