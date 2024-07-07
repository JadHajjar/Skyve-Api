using Extensions.Sql;

using System;

namespace SkyveApi.Domain.CS2;

[DynamicSqlClass("CS2_PackageEdits")]
public class PackageEditData : IDynamicSql
{
#if API
	[DynamicSqlProperty(Indexer = true), System.Text.Json.Serialization.JsonIgnore]
	public ulong PackageId { get; set; }
#endif

	[DynamicSqlProperty]
	public string? Username { get; set; }

	[DynamicSqlProperty]
	public DateTime EditDate { get; set; }

	[DynamicSqlProperty]
	public string? Note { get; set; }
}
