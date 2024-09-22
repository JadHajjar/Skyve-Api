using Extensions.Sql;

using System;

namespace SkyveApi.Domain.RoadBuilder;

[DynamicSqlClass("RB_Roads")]
public class RoadBuilderEntry : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public string? ID { get; set; }
	[DynamicSqlProperty]
	public string? Name { get; set; }
	[DynamicSqlProperty]
	public string? Icon { get; set; }
	[DynamicSqlProperty]
	public string? Author { get; set; }
	[DynamicSqlProperty]
	public string? Tags { get; set; }
	[DynamicSqlProperty]
	public int Category { get; set; }
	[DynamicSqlProperty]
	public int Downloads { get; set; }
	[DynamicSqlProperty]
	public DateTime UploadTime { get; set; }
}