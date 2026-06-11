using Extensions.Sql;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SkyveApi.Pages;

public class ObsoleteModel : PageModel
{
	[BindProperty(SupportsGet = true)]
	public string? v { get; set; }

	public void OnGet()
	{
	}
}
