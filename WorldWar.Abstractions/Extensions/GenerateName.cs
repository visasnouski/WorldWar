using System.Globalization;
using System.Security.Cryptography;

namespace WorldWar.Abstractions.Extensions;

public static class GenerateName
{
	public static string Generate(int len)
	{
		string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
		string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
		var name = "";
		name += consonants[RandomNumberGenerator.GetInt32(0, consonants.Length)].ToUpper(CultureInfo.CurrentCulture);
		name += vowels[RandomNumberGenerator.GetInt32(0, vowels.Length)];
		var b = 2;
		while (b < len)
		{
			name += consonants[RandomNumberGenerator.GetInt32(0, consonants.Length)];
			b++;
			name += vowels[RandomNumberGenerator.GetInt32(0, vowels.Length)];
			b++;
		}

		return name;
	}
}