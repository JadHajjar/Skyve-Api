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

	[Route("/guide")]
	public IActionResult Guide()
	{
		return new RedirectResult("https://www.youtube.com/watch?v=mQvWrQ4rk_U");
	}

	[Route("/donate")]
	public IActionResult Donate()
	{
		return new RedirectResult("https://ko-fi.com/chameleontbn");
	}

	[Route("/translate")]
	public IActionResult Translate()
	{
		return new RedirectResult("https://crowdin.com/project/load-order-mod-2");
	}

	[Route("/app/{*url}")]
	public IActionResult App(string url)
	{
		return new RedirectResult($"skyve://{url}");
	}
}
