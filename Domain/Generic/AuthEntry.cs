using Extensions.Sql;

using System;

namespace SkyveApi.Domain.Generic;
public class AuthEntry : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public Guid Guid { get; set; }
	[DynamicSqlProperty]
	public int Type { get; set; }
	[DynamicSqlProperty]
	public string? Value { get; set; }
}
