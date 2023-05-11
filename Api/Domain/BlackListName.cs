using Extensions.Sql;

namespace ApiApplication.Domain;

[DynamicSqlClass("BlackListNames")]
public class BlackListName : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public string? Name { get; set; }
}
