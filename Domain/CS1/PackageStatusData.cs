using Extensions.Sql;

using System;
using System.Linq;

namespace SkyveApi.Domain.CS1;

[DynamicSqlClass("CS1_PackageStatuses")]
public class PackageStatusData : IDynamicSql
{
#if API
	[DynamicSqlProperty(PrimaryKey = true, Indexer = true), System.Text.Json.Serialization.JsonIgnore]
#endif
	public ulong PackageId { get; set; }

	[DynamicSqlProperty(PrimaryKey = true)]
	public int Type { get; set; }

	[DynamicSqlProperty]
	public int Action { get; set; }

	public ulong[]? Packages { get; set; }

	[DynamicSqlProperty]
	public string? Note { get; set; }

#if API
	[DynamicSqlProperty(ColumnName = nameof(Packages)), System.Text.Json.Serialization.JsonIgnore]
#endif
	public string? PackageList { get => Packages is null ? null : string.Join(",", Packages); set => Packages = value?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(ulong.Parse).ToArray(); }

	public PackageStatusData()
	{

	}
}
