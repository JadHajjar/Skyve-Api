using Extensions.Sql;

namespace SkyveApi.Domain.CS2;

[DynamicSqlClass("CS2_PackageTags")]
public class PackageTagData : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true, Indexer = true)]
	public ulong PackageId { get; set; }

	[DynamicSqlProperty(PrimaryKey = true)]
	public string Tag { get; set; }

	public PackageTagData()
	{
		Tag = string.Empty;
	}
}
