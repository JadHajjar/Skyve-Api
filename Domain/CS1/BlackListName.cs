using Extensions.Sql;

namespace SkyveApi.Domain.CS1;

[DynamicSqlClass("BlackListNames")]
public class BlackListName : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public string? Name { get; set; }
}
