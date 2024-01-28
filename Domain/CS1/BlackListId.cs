using Extensions.Sql;

namespace SkyveApi.Domain.CS1;

[DynamicSqlClass("BlackListIds")]
public class BlackListId : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public ulong SteamId { get; set; }
}
