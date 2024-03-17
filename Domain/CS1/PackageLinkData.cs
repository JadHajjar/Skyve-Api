using Extensions.Sql;

namespace SkyveApi.Domain.CS1;

[DynamicSqlClass("CS1_PackageLinks")]
public class PackageLinkData : IDynamicSql
{
#if API
	[DynamicSqlProperty(Indexer = true), System.Text.Json.Serialization.JsonIgnore]
#endif
	public ulong PackageId { get; set; }

	[DynamicSqlProperty]
	public int Type { get; set; }

	[DynamicSqlProperty]
	public string? Url { get; set; }

	[DynamicSqlProperty]
	public string? Title { get; set; }
}
