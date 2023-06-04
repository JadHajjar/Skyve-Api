using System.Security.Cryptography;
using System.Text;

namespace SkyveApi.Utilities;

public static class IdHasher
{
	public static string HashToShortString(int id)
	{
		using var md5 = MD5.Create();
		var inputBytes = BitConverter.GetBytes(id);
		var hashBytes = md5.ComputeHash(inputBytes);

		var stringBuilder = new StringBuilder();
		for (var i = 0; i < 4; i++)
		{
			stringBuilder.Append(hashBytes[i].ToString("X2"));
		}

		return stringBuilder.ToString().Substring(0, 10);
	}

	public static int ShortStringToHash(string hashedValue)
	{
		using var md5 = MD5.Create();
		var hashBytes = new byte[16];
		for (var i = 0; i < 10; i += 2)
		{
			hashBytes[i / 2] = Convert.ToByte(hashedValue.Substring(i, 2), 16);
		}

		return BitConverter.ToInt32(hashBytes, 0);
	}
}
