using Extensions.Sql;

namespace SkyveApi.Domain.CS1;

[DynamicSqlClass("Managers")]
public class Manager : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public ulong SteamId { get; set; }
}
