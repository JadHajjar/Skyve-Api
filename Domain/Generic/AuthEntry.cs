using Extensions.Sql;

using Skyve.Compatibility.Domain.Enums;

using System;

namespace SkyveApi.Domain.Generic;
public class AuthEntry : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public Guid Guid { get; set; }
	[DynamicSqlProperty]
	public AuthType Type { get; set; }
	[DynamicSqlProperty]
	public string? Value { get; set; }
}
