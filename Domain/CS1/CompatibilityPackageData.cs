using Extensions.Sql;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SkyveApi.Domain.CS1;
[DynamicSqlClass("CS1_Packages")]
public class CompatibilityPackageData : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public ulong SteamId { get; set; }
	[DynamicSqlProperty]
	public string? Name { get; set; }
	[DynamicSqlProperty]
	public string? FileName { get; set; }
	[DynamicSqlProperty]
	public ulong AuthorId { get; set; }
	[DynamicSqlProperty]
	public string? Note { get; set; }
	[DynamicSqlProperty]
	public DateTime ReviewDate { get; set; }
	[DynamicSqlProperty]
	public int Stability { get; set; }
	[DynamicSqlProperty]
	public int Usage { get; set; } = -1;
	[DynamicSqlProperty]
	public int Type { get; set; }
#if API
	[DynamicSqlProperty(ColumnName = nameof(RequiredDLCs)), System.Text.Json.Serialization.JsonIgnore]
#endif
	public string? RequiredDLCsList { get => RequiredDLCs is null ? null : string.Join(",", RequiredDLCs); set => RequiredDLCs = value?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(uint.Parse).ToArray(); }
	public uint[]? RequiredDLCs { get; set; }
	public List<string>? Tags { get; set; }
	public List<PackageLinkData>? Links { get; set; }
	public List<PackageStatusData>? Statuses { get; set; }
	public List<PackageInteractionData>? Interactions { get; set; }
}
