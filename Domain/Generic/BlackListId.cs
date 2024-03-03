using Extensions.Sql;

namespace SkyveApi.Domain.Generic;

[DynamicSqlClass("CS2_BlackListIds")]
public class BlackListId : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public ulong Id { get; set; }
}
