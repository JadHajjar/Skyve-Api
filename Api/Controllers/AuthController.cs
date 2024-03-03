using Extensions.Sql;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

using System.Text.RegularExpressions;
using SkyveApi.Domain.Generic;
using Skyve.Compatibility.Domain.Enums;

namespace SkyveApi.Controllers;

[Route("[controller]")]
public class AuthController : ControllerBase
{
	private static readonly Regex _steamIdRegex = new(@"\b(7[0-9]{15,25})\b", RegexOptions.Compiled);
	private readonly Dictionary<Guid, string> _steamResults = [];

	[Route("[action]")]
	public Guid SteamHandshake()
	{
		var guid = Guid.NewGuid();

		while (new AuthEntry { Guid = guid }.SqlGetById() is not null)
		{
			guid = Guid.NewGuid();
		}

		_steamResults[guid] = string.Empty;

		return guid;
	}

	[Route("[action]")]
	public IActionResult Steam(Guid guid)
	{
		var redirect = $"{Request.Scheme}://{Request.Host}/Auth/SteamSuccess?guid={guid}";

		return Challenge(new AuthenticationProperties
		{
			RedirectUri = Uri.EscapeDataString(redirect)
		}, "Steam");
	}

	[Route("[action]")]
	public async Task<IActionResult> SteamSuccess(Guid guid)
	{
		var idString = (await HttpContext.AuthenticateAsync("Steam"))?
			.Principal?
			.Claims?.FirstOrDefault(claim => claim.Type.Contains("nameidentifier"))?
			.Value;

		if (!string.IsNullOrWhiteSpace(idString) && _steamIdRegex.IsMatch(idString))
		{
			new AuthEntry
			{
				Guid = guid,
				Type = AuthType.Steam,
				Value = _steamIdRegex.Match(idString!).Groups[1].Value
			}.SqlAdd();
		}

		return Ok();
	}
}
