using Extensions.Sql;

namespace SkyveApi.Domain.Generic;
[DynamicSqlClass("CS2_GoFileInfo", AlwaysReturn = true, SingleRecord = true)]
public class GoFileInfoData : IDynamicSql
{
	[DynamicSqlProperty]
	public string? Token { get; set; }
	[DynamicSqlProperty]
	public string? RootFolder { get; set; }
}