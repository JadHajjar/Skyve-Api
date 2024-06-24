using Microsoft.AspNetCore.Mvc;

namespace SkyveApi.Controllers;
public class RedirectController : ControllerBase
{
	[Route("/")]
	public IActionResult Home()
	{
		return new RedirectResult("https://mods.paradoxplaza.com/mods/75804/Windows");
	}

	[Route("/discord")]
	public IActionResult Discord()
	{
		return new RedirectResult("https://discord.gg/E4k8ZEtRxd");
	}

	[Route("/app/{*url}")]
	public IActionResult App(string url)
	{
		return new RedirectResult($"skyve://{url}");
	}
}
