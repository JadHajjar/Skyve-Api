using Extensions.Sql;

namespace SkyveApi.Domain;

[DynamicSqlClass("BlackListIds")]
public class BlackListId : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public ulong SteamId { get; set; }
}
