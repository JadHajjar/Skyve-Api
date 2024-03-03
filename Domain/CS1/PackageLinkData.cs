using Extensions.Sql;

using Skyve.Compatibility.Domain;
using Skyve.Domain.Enums;

namespace SkyveApi.Domain.CS1;

[DynamicSqlClass("PackageLinks")]
public class PackageLinkData : IDynamicSql
{
#if API
	[DynamicSqlProperty(Indexer = true), System.Text.Json.Serialization.JsonIgnore]
#endif
	public ulong PackageId { get; set; }

	[DynamicSqlProperty]
	public LinkType Type { get; set; }

	[DynamicSqlProperty]
	public string? Url { get; set; }

	[DynamicSqlProperty]
	public string? Title { get; set; }
}
