using Extensions.Sql;

namespace SkyveApi.Domain.CS1;

[DynamicSqlClass("CS1_Packages")]
public class PostPackage : CompatibilityPackageData
{
	public Author? Author { get; set; }
	public bool BlackListId { get; set; }
	public bool BlackListName { get; set; }
}
