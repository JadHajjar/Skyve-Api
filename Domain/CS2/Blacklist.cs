using System.Collections.Generic;

namespace SkyveApi.Domain.CS2;
public class Blacklist
{
	public List<ulong> BlackListedIds { get; set; }
	public List<string> BlackListedNames { get; set; }

	public Blacklist()
	{
		BlackListedIds = [];
		BlackListedNames = [];
	}
}
