using Extensions.Sql;

using Microsoft.AspNetCore.Mvc;

using SkyveApi.Domain.Generic;

namespace SkyveApi.Controllers;
public class RedirectController : ControllerBase
{
	//[Route("/")]
	//public IActionResult Home()
	//{
	//	return 
	//	return RedirectEndpoint("Home");
	//}

	[Route("/{key}")]
	public IActionResult RedirectEndpoint(string key)
	{
		var url = new RedirectLink(key).SqlGetById()?.Link;

		if (url == null)
		{
			return NotFound();
		}

		return new RedirectResult(url);
	}

	[Route("/app/{*url}")]
	public IActionResult App(string url)
	{
		return new RedirectResult($"skyve://{url}");
	}
}
