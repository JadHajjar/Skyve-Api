using Extensions.Sql;

namespace ApiApplication.Domain;

[DynamicSqlClass("BlackListIds")]
public class BlackListId : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public ulong SteamId { get; set; }
}
