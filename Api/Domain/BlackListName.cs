using Extensions.Sql;

namespace SkyveApi.Domain;

[DynamicSqlClass("BlackListNames")]
public class BlackListName : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public string? Name { get; set; }
}
