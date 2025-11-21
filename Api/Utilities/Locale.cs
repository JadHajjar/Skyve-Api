using Extensions;

namespace SkyveApi.Utilities;

public class Locale : LocaleHelper
{
	private static readonly Locale _instance = new();

	public static void Load() { _ = _instance; }

	protected Locale() : base($"SkyveApi.Properties.SlickUI.json") { }
}