using Extensions.Sql;

using System;

namespace SkyveApi.Domain.CS2;

[DynamicSqlClass("CS2_Announcements")]
public class AnnouncementData : IDynamicSql
{
	[DynamicSqlProperty]
	public DateTime Date { get; set; }
	[DynamicSqlProperty]
	public string? Title { get; set; }
	[DynamicSqlProperty]
	public string? Description { get; set; }
	[DynamicSqlProperty]
	public DateTime? EndDate { get; set; }
}
