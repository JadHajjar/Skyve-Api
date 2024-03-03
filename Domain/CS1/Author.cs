using Extensions.Sql;

namespace SkyveApi.Domain.CS1;

[DynamicSqlClass("CS1_Authors")]
public class Author : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public ulong SteamId { get; set; }
	[DynamicSqlProperty]
	public string? Name { get; set; }
	[DynamicSqlProperty]
	public bool Retired { get; set; }
	[DynamicSqlProperty]
	public bool Verified { get; set; }
	[DynamicSqlProperty]
	public bool Malicious { get; set; }
}
