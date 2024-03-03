using Extensions.Sql;

namespace SkyveApi.Domain.Generic;

[DynamicSqlClass("CS2_BlackListNames")]
public class BlackListName : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public string Name { get; set; } = string.Empty;
}
