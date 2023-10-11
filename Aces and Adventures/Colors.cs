using System;
using UnityEngine;

public static class Colors
{
	public static Color32 STAT_HIGHLIGHT_GREEN = new Color32(180, byte.MaxValue, 180, byte.MaxValue);

	public static Color32 STAT_HIGHLIGHT_RED = new Color32(byte.MaxValue, 180, 180, byte.MaxValue);

	public static readonly Color TRANSPARENT = new Color(0f, 0f, 0f, 0f);

	public const float BYTE_TO_FLOAT = 0.003921569f;

	public static Color ACTIVATE => GameColors.Default.value.activate;

	public static Color TARGET => GameColors.Default.value.target;

	public static Color TARGET_SELECTED => GameColors.Default.value.targetSelected;

	public static Color ATTACK => GameColors.Default.value.attack;

	public static Color ATTACK_CONFIRM => GameColors.Default.value.attackConfirm;

	public static Color USED => GameColors.Default.value.used;

	public static Color CAN_BE_USED => GameColors.Default.value.canBeUsed;

	public static Color FAILURE => GameColors.Default.value.failure;

	public static Color TIE => GameColors.Default.value.tie;

	public static Color SUCCESS => GameColors.Default.value.success;

	public static Color END_TURN_CAUTION => GameColors.Default.value.endTurnCaution;

	public static Color END_TURN => GameColors.Default.value.endTurn;

	public static Color SELECTED => GameColors.Default.value.selected;

	public static GameColors gameColors => GameColors.Default.value;

	public static Color HSVToRGB(float H, float S, float V)
	{
		if (S == 0f)
		{
			return new Color(V, V, V);
		}
		if (V == 0f)
		{
			return Color.black;
		}
		Color black = Color.black;
		float num = H * 6f;
		int num2 = Mathf.FloorToInt(num);
		float num3 = num - (float)num2;
		float num4 = V * (1f - S);
		float num5 = V * (1f - S * num3);
		float num6 = V * (1f - S * (1f - num3));
		switch (num2)
		{
		case -1:
			black.r = V;
			black.g = num4;
			black.b = num5;
			break;
		case 0:
			black.r = V;
			black.g = num6;
			black.b = num4;
			break;
		case 1:
			black.r = num5;
			black.g = V;
			black.b = num4;
			break;
		case 2:
			black.r = num4;
			black.g = V;
			black.b = num6;
			break;
		case 3:
			black.r = num4;
			black.g = num5;
			black.b = V;
			break;
		case 4:
			black.r = num6;
			black.g = num4;
			black.b = V;
			break;
		case 5:
			black.r = V;
			black.g = num4;
			black.b = num5;
			break;
		case 6:
			black.r = V;
			black.g = num6;
			black.b = num4;
			break;
		}
		black.r = Mathf.Clamp(black.r, 0f, 1f);
		black.g = Mathf.Clamp(black.g, 0f, 1f);
		black.b = Mathf.Clamp(black.b, 0f, 1f);
		return black;
	}

	public static void RGBToHSV(Color rgbColor, out float H, out float S, out float V)
	{
		if (rgbColor.b > rgbColor.g && rgbColor.b > rgbColor.r)
		{
			_RGBToHSVHelper(4f, rgbColor.b, rgbColor.r, rgbColor.g, out H, out S, out V);
		}
		else if (rgbColor.g > rgbColor.r)
		{
			_RGBToHSVHelper(2f, rgbColor.g, rgbColor.b, rgbColor.r, out H, out S, out V);
		}
		else
		{
			_RGBToHSVHelper(0f, rgbColor.r, rgbColor.g, rgbColor.b, out H, out S, out V);
		}
	}

	private static void _RGBToHSVHelper(float offset, float dominantcolor, float colorone, float colortwo, out float H, out float S, out float V)
	{
		V = dominantcolor;
		if (V != 0f)
		{
			float num = 0f;
			num = ((!(colorone > colortwo)) ? colorone : colortwo);
			float num2 = V - num;
			if (num2 != 0f)
			{
				S = num2 / V;
				H = offset + (colorone - colortwo) / num2;
			}
			else
			{
				S = 0f;
				H = offset + (colorone - colortwo);
			}
			H /= 6f;
			if (H < 0f)
			{
				H += 1f;
			}
		}
		else
		{
			S = 0f;
			H = 0f;
		}
	}

	public static Color FromString(string s)
	{
		string[] array = s.Split(',');
		Color result = new Color(0f, 0f, 0f, 1f);
		for (int i = 0; i < array.Length; i++)
		{
			result[i] = StringUtil.ParseInvariantF(array[i]);
		}
		return result;
	}

	public static Color32 FromString32(string s)
	{
		string[] array = s.Split(',');
		Color32 result = new Color32(0, 0, 0, byte.MaxValue);
		for (int i = 0; i < array.Length; i++)
		{
			byte b = StringUtil.ParseInvariantB(array[i]);
			switch (i)
			{
			case 0:
				result.r = b;
				break;
			case 1:
				result.g = b;
				break;
			case 2:
				result.b = b;
				break;
			case 3:
				result.a = b;
				break;
			}
		}
		return result;
	}

	public static Color LerpHSV(Color startRGB, Color endRGB, float t)
	{
		return Color.Lerp(startRGB.ToHSV(), endRGB.ToHSV(), t).ToRGB();
	}

	public static Color SetAlpha(this Color c, float alpha)
	{
		return new Color(c.r, c.g, c.b, alpha);
	}

	public static Color MultiplyAlpha(this Color c, float alphaMultiplier)
	{
		return new Color(c.r, c.g, c.b, c.a * alphaMultiplier);
	}

	public static Color SetValue(this Color c, float value)
	{
		Color hsv = c.ToHSV();
		hsv.b = value;
		return hsv.ToRGB();
	}

	public static Color32 SetAlpha(this Color32 c, float alpha)
	{
		return new Color32(c.r, c.g, c.b, (byte)Mathf.RoundToInt(alpha * 255f));
	}

	public static Color32 SetAlpha32(this Color32 c, byte alpha)
	{
		return new Color32(c.r, c.g, c.b, alpha);
	}

	public static Color32 SetRGB32(this Color32 c, Color32 rgb)
	{
		return new Color32(rgb.r, rgb.g, rgb.b, c.a);
	}

	public static float Alpha(this Color32 c)
	{
		return (float)(int)c.a * MathUtil.OneTwoFiftyFifth;
	}

	public static void SetComponentValues(this Color32 c, ref byte r, ref byte g, ref byte b)
	{
		r = c.r;
		g = c.g;
		b = c.b;
	}

	public static bool EqualTo(this Color32 color, Color32 other)
	{
		if (color.r == other.r && color.g == other.g && color.b == other.b)
		{
			return color.a == other.a;
		}
		return false;
	}

	public static bool EqualTo(this Color32? color, Color32? other)
	{
		if (color.HasValue == other.HasValue)
		{
			if (color.HasValue)
			{
				return color.Value.EqualTo(other.Value);
			}
			return true;
		}
		return false;
	}

	public static bool EqualTo(this Gradient gradient, Color color)
	{
		GradientColorKey[] colorKeys = gradient.colorKeys;
		for (int i = 0; i < colorKeys.Length; i++)
		{
			if (colorKeys[i].color != color)
			{
				return false;
			}
		}
		GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
		for (int i = 0; i < alphaKeys.Length; i++)
		{
			if (alphaKeys[i].alpha != color.a)
			{
				return false;
			}
		}
		return true;
	}

	public static Color Add(this Color x, Color y, bool includeAlpha = true)
	{
		x.r += y.r;
		x.g += y.g;
		x.b += y.b;
		if (includeAlpha)
		{
			x.a += y.a;
		}
		return x;
	}

	public static Color Multiply(this Color x, Color y, bool includeAlpha = true)
	{
		x.r *= y.r;
		x.g *= y.g;
		x.b *= y.b;
		if (includeAlpha)
		{
			x.a *= y.a;
		}
		return x;
	}

	public static Color? Multiply(this Color? a, Color? b, bool includeAlpha = true)
	{
		if (!a.HasValue)
		{
			return b;
		}
		if (!b.HasValue)
		{
			return a;
		}
		Color value = a.Value;
		Color value2 = b.Value;
		value.r *= value2.r;
		value.g *= value2.g;
		value.b *= value2.b;
		if (includeAlpha)
		{
			value.a *= value2.a;
		}
		return value;
	}

	public static Color Clamp(this Color c, Color min, Color max)
	{
		return new Color(Mathf.Clamp(c.r, min.r, max.r), Mathf.Clamp(c.g, min.g, max.g), Mathf.Clamp(c.b, min.b, max.b), Mathf.Clamp(c.a, min.a, max.a));
	}

	public static Color Clamp01(this Color c)
	{
		return new Color(Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b), Mathf.Clamp01(c.a));
	}

	public static byte rByte(this Color c)
	{
		return (byte)(c.r * 255f + 0.5f);
	}

	public static byte gByte(this Color c)
	{
		return (byte)(c.g * 255f + 0.5f);
	}

	public static byte bByte(this Color c)
	{
		return (byte)(c.b * 255f + 0.5f);
	}

	public static Color ToHSV(this Color rgb)
	{
		RGBToHSV(rgb, out var H, out var S, out var V);
		return new Color(H, S, V, rgb.a);
	}

	public static Color ToHSV(this Color rgb, float? hueOverride, float? saturationOverride, float? valueOverride)
	{
		Color result = rgb.ToHSV();
		if (hueOverride.HasValue)
		{
			result.r = hueOverride.Value;
		}
		if (saturationOverride.HasValue)
		{
			result.g = saturationOverride.Value;
		}
		if (valueOverride.HasValue)
		{
			result.b = valueOverride.Value;
		}
		return result;
	}

	public static Color ToRGB(this Color hsv)
	{
		Color result = HSVToRGB(hsv.r, hsv.g, hsv.b);
		result.a = hsv.a;
		return result;
	}

	public static float HueInShortestDirection(float hue, float targetHue)
	{
		float num = targetHue - hue;
		int num2 = Math.Sign(num);
		if (!(num * (float)num2 <= 0.5f))
		{
			if (num2 != 1)
			{
				return 1f + targetHue;
			}
			return targetHue - 1f;
		}
		return targetHue;
	}

	public static float WrapHue(float hue)
	{
		float num = MathUtil.FloatModulus(hue, 1f);
		if (!(num >= 0f))
		{
			return 1f + num;
		}
		return num;
	}

	public static Color LerpInHSVOutputRGB(this Color startRGB, Color endRGB, float lerp, bool hueInShortestDirection = true)
	{
		Color color = startRGB.ToHSV();
		Color color2 = endRGB.ToHSV();
		return new Color(WrapHue(Mathf.Lerp(color.r, hueInShortestDirection ? HueInShortestDirection(color.r, color2.r) : color2.r, lerp)), Mathf.Lerp(color.g, color2.g, lerp), Mathf.Lerp(color.b, color2.b, lerp), 1f).ToRGB();
	}

	public static Color32 LerpInHSVOutputRGB(this Color32 startRGB, Color32 endRGB, float lerp, bool hueInShortestDirection = true)
	{
		return ((Color)startRGB).LerpInHSVOutputRGB((Color)endRGB, lerp, hueInShortestDirection);
	}

	public static Color AdjustSaturation(this Color rgb, float saturationMultiplier)
	{
		Color hsv = rgb.ToHSV();
		hsv.g *= saturationMultiplier;
		hsv.g = Mathf.Clamp01(hsv.g);
		return hsv.ToRGB();
	}

	public static Color32 AdjustSaturation(this Color32 rgb, float saturationMultiplier)
	{
		return ((Color)rgb).AdjustSaturation(saturationMultiplier);
	}

	public static Color AdjustBrightness(this Color rgb, float valueMultiplier)
	{
		Color hsv = rgb.ToHSV();
		hsv.b *= valueMultiplier;
		hsv.b = Mathf.Clamp01(hsv.b);
		return hsv.ToRGB();
	}

	public static Color32 AdjustBrightness(this Color32 rgb, float valueMultiplier)
	{
		return ((Color)rgb).AdjustBrightness(valueMultiplier);
	}

	public static string ToStringValue(this Color color)
	{
		return color.r + "," + color.g + "," + color.b + "," + color.a;
	}

	public static string ToStringValue(this Color32 color)
	{
		return color.r + "," + color.g + "," + color.b + "," + color.a;
	}

	public static Vector3 ToVector3(this Color color)
	{
		return new Vector3(color.r, color.g, color.b);
	}

	public static Vector4 ToVector4(this Color color)
	{
		return color;
	}

	public static Color ToColor(this Vector3 v, float a = 1f)
	{
		return new Color(v.x, v.y, v.z, a);
	}
}
