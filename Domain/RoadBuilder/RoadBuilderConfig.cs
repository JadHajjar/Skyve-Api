using Extensions.Sql;

namespace SkyveApi.Domain.RoadBuilder;

[DynamicSqlClass("RB_RoadConfigs")]
public class RoadBuilderConfig : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public string? ID { get; set; }
	[DynamicSqlProperty]
	public string? Payload { get; set; }
}
