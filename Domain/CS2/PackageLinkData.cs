using Extensions.Sql;

namespace SkyveApi.Domain.CS2;

[DynamicSqlClass("CS2_PackageLinks")]
public class PackageLinkData : IDynamicSql
{
#if API
	[DynamicSqlProperty(Indexer = true), System.Text.Json.Serialization.JsonIgnore]
	public ulong PackageId { get; set; }
#endif

	[DynamicSqlProperty]
	public int Type { get; set; }

	[DynamicSqlProperty]
	public string? Url { get; set; }

	[DynamicSqlProperty]
	public string? Title { get; set; }
}
