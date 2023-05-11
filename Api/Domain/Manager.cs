using Extensions.Sql;

namespace ApiApplication.Domain;

[DynamicSqlClass("Managers")]
public class Manager : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public ulong SteamId { get; set; }
}
