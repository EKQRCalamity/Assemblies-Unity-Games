using System.Globalization;

public static class Parser
{
	private static NumberFormatInfo InvariantInfo = CultureInfo.InvariantCulture.NumberFormat;

	public static string ToStringInvariant(this int value)
	{
		return value.ToString(InvariantInfo);
	}

	public static string ToStringInvariant(this float value)
	{
		return value.ToString(InvariantInfo);
	}

	public static int IntParse(string s)
	{
		return int.Parse(s, InvariantInfo);
	}

	public static bool IntTryParse(string s, out int result)
	{
		return int.TryParse(s, NumberStyles.Integer, InvariantInfo, out result);
	}

	public static float FloatParse(string s)
	{
		return float.Parse(s, InvariantInfo);
	}

	public static bool FloatTryParse(string s, out float result)
	{
		return float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, InvariantInfo, out result);
	}

	public static byte ByteParse(string s)
	{
		return byte.Parse(s, InvariantInfo);
	}

	public static byte ByteParse(string s, NumberStyles style)
	{
		return byte.Parse(s, style, InvariantInfo);
	}

	public static bool ByteTryParse(string s, out byte result)
	{
		return byte.TryParse(s, NumberStyles.Integer, InvariantInfo, out result);
	}
}
