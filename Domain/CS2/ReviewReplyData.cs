using Extensions.Sql;

using System;

namespace SkyveApi.Domain.CS2;

[DynamicSqlClass("CS2_ReviewReplies")]
public class ReviewReplyData : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true, Indexer = true)]
	public string? Username { get; set; }
	[DynamicSqlProperty(PrimaryKey = true)]
	public ulong PackageId { get; set; }
	[DynamicSqlProperty]
	public string? Message { get; set; }
	[DynamicSqlProperty]
	public string? Link { get; set; }
	[DynamicSqlProperty]
	public bool RequestUpdate { get; set; }
	[DynamicSqlProperty]
	public DateTime Timestamp { get; set; }
}
