﻿using Extensions.Sql;

namespace SkyveApi.Domain.CS2;

[DynamicSqlClass("CS2_Users")]
public class UserData : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public string? Id { get; set; }
	[DynamicSqlProperty]
	public string? Name { get; set; }
	[DynamicSqlProperty]
	public bool Retired { get; set; }
	[DynamicSqlProperty]
	public bool Verified { get; set; }
	[DynamicSqlProperty]
	public bool Malicious { get; set; }
	[DynamicSqlProperty]
	public bool Manager { get; set; }
}
