using System.Collections.Generic;

namespace SkyveApi.Domain.CS2;

public class BulkCompatibilityPackageUpdateData
{
	public List<ulong> Packages { get; set; } = [];
	public int Stability { get; set; }
	public string? ReviewedGameVersion { get; set; }
}

