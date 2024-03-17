using Extensions.Sql;

namespace SkyveApi.Domain.CS1;

[DynamicSqlClass("CS1_PackageTags")]
public class PackageTagData : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true, Indexer = true)]
	public ulong PackageId { get; set; }

	[DynamicSqlProperty(PrimaryKey = true)]
	public string? Tag { get; set; }
}
