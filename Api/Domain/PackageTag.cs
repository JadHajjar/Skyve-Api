using Extensions.Sql;

namespace SkyveApi.Domain;

[DynamicSqlClass("PackageTags")]
public class PackageTag : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true, Indexer = true)]
	public ulong PackageId { get; set; }

	[DynamicSqlProperty(PrimaryKey = true)]
	public string? Tag { get; set; }
}
