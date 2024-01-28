using Extensions.Sql;

using Skyve.Compatibility.Domain;

using System;
using System.Linq;

namespace SkyveApi.Domain.CS1;

[DynamicSqlClass("PackageInteractions")]
public class PackageInteraction : IDynamicSql
{
#if API
	[DynamicSqlProperty(PrimaryKey = true, Indexer = true), System.Text.Json.Serialization.JsonIgnore]
#endif
	public ulong PackageId { get; set; }

	[DynamicSqlProperty(PrimaryKey = true)]
	public InteractionType Type { get; set; }

	[DynamicSqlProperty]
	public StatusAction Action { get; set; }

	public ulong[]? Packages { get; set; }

	[DynamicSqlProperty]
	public string? Note { get; set; }

#if API
	[DynamicSqlProperty(ColumnName = nameof(Packages)), System.Text.Json.Serialization.JsonIgnore]
#endif
	public string? PackageList { get => Packages is null ? null : string.Join(",", Packages); set => Packages = value?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(ulong.Parse).ToArray(); }

	public PackageInteraction()
	{

	}

	public PackageInteraction(InteractionType type, StatusAction action = StatusAction.NoAction)
	{
		Type = type;
		Action = action;
	}
}
