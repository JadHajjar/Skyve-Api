using Extensions.Sql;

using System;

namespace SkyveApi.Domain.CS2;
[DynamicSqlClass("CS2_ReviewRequests")]
public class ReviewRequestData : ReviewRequestNoLogData
{
	[DynamicSqlProperty]
	public byte[]? LogFile { get; set; }
}

[DynamicSqlClass("CS2_ReviewRequests")]
public class ReviewRequestNoLogData : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true, Indexer = true)]
	public ulong PackageId { get; set; }
	[DynamicSqlProperty(PrimaryKey = true)]
	public string? UserId { get; set; }
	[DynamicSqlProperty]
	public bool IsMissingInfo { get; set; }
	[DynamicSqlProperty]
	public int PackageStability { get; set; }
	[DynamicSqlProperty]
	public int PackageUsage { get; set; }
	[DynamicSqlProperty]
	public int PackageType { get; set; }
	[DynamicSqlProperty]
	public int SavegameEffect { get; set; }
	[DynamicSqlProperty]
	public string? RequiredDLCs { get; set; }
	[DynamicSqlProperty]
	public string? PackageNote { get; set; }
	[DynamicSqlProperty]
	public string? SaveUrl { get; set; }
	[DynamicSqlProperty]
	public DateTime Timestamp { get; set; }
}
