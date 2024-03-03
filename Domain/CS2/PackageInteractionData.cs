using Extensions.Sql;

using Skyve.Compatibility.Domain.Enums;

using System;
using System.Linq;

namespace SkyveApi.Domain.CS2;

[DynamicSqlClass("CS2_PackageInteractions")]
public class PackageInteractionData : IDynamicSql
{
#if API
	[DynamicSqlProperty(PrimaryKey = true, Indexer = true), System.Text.Json.Serialization.JsonIgnore]
	public ulong PackageId { get; set; }
#endif

	[DynamicSqlProperty(PrimaryKey = true)]
	public int Type { get; set; }

	[DynamicSqlProperty]
	public int Action { get; set; }

	public ulong[]? Packages { get; set; }

	[DynamicSqlProperty]
	public string? Note { get; set; }

#if API
	[DynamicSqlProperty(ColumnName = nameof(Packages)), System.Text.Json.Serialization.JsonIgnore]
	public string? PackageList { get => Packages is null ? null : string.Join(",", Packages); set => Packages = value?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(ulong.Parse).ToArray(); }
#endif

	public PackageInteractionData()
	{

	}

	public PackageInteractionData(InteractionType type, StatusAction action = StatusAction.NoAction)
	{
		Type = (int)type;
		Action = (int)action;
	}
}
