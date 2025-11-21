using Extensions.Sql;

namespace SkyveApi.Domain.Generic;
[DynamicSqlClass("Keys")]
public class ApiKeys : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public string? ApiKey { get; set; }
	[DynamicSqlProperty]
	public string? AllowedDirectories { get; set; }
}
