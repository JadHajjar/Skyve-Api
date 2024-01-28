using System.Collections.Generic;

namespace SkyveApi.Domain.CS1;
public class CompatibilityData
{
	public List<CompatibilityPackageData>? Packages { get; set; }
	public List<Author>? Authors { get; set; }
	public List<ulong>? BlackListedIds { get; set; }
	public List<string>? BlackListedNames { get; set; }
}
