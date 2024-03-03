using Extensions.Sql;

namespace SkyveApi.Domain.CS2;

[DynamicSqlClass("CS2_Packages")]
public class PostPackage : CompatibilityPackageData
{
	public bool BlackListId { get; set; }
	public bool BlackListName { get; set; }
}
