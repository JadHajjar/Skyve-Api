using Extensions.Sql;

namespace SkyveApi.Domain.Generic;
[DynamicSqlClass("RedirectLinks")]
public class RedirectLink : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public string? Key { get; set; }
	[DynamicSqlProperty]
	public string? Link { get; set; }

	public RedirectLink(string key)
	{
		Key = key;
	}

	public RedirectLink()
	{

	}
}
