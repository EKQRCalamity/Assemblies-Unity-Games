using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class TextMeshHelper
{
	private static StringBuilder _ColorBuilder = new StringBuilder();

	private static StringBuilder _InnerBuilder = new StringBuilder();

	private static StringBuilder _Builder = new StringBuilder();

	private static Dictionary<Color32, string> _ColorTags = new Dictionary<Color32, string>();

	private static Dictionary<Color, string> _ColorHexes = new Dictionary<Color, string>();

	public const string FONT_RIGHTEOUS = "Righteous SDF";

	public const string MAT_RIGHTEOUS_COMBAT_TEXT = "Righteous SDF CombatText";

	public static string ColorHex(Color32 color32)
	{
		if (!_ColorHexes.ContainsKey(color32))
		{
			_ColorBuilder.Clear();
			_ColorBuilder.Append(_InnerBuilder.FromValueMap(color32.r, IOUtil.HexCharMap, null, 2));
			_ColorBuilder.Append(_InnerBuilder.FromValueMap(color32.g, IOUtil.HexCharMap, null, 2));
			_ColorBuilder.Append(_InnerBuilder.FromValueMap(color32.b, IOUtil.HexCharMap, null, 2));
			if (color32.a != byte.MaxValue)
			{
				_ColorBuilder.Append(_InnerBuilder.FromValueMap(color32.a, IOUtil.HexCharMap));
			}
			_ColorHexes.Add(color32, _ColorBuilder.ToString());
		}
		return _ColorHexes[color32];
	}

	public static string Color(Color32 color32)
	{
		if (!_ColorTags.ContainsKey(color32))
		{
			_Builder.Clear();
			_Builder.Append("<#");
			_Builder.Append(ColorHex(color32));
			_Builder.Append(">");
			_ColorTags.Add(color32, _Builder.ToString());
		}
		return _ColorTags[color32];
	}

	public static string ToHex(byte b)
	{
		return _ColorBuilder.FromValueMap(b, IOUtil.HexCharMap, null, 2);
	}

	public static string EndColor()
	{
		return "</color>";
	}

	public static string Link(int id)
	{
		_Builder.Clear();
		_Builder.Append("<link=");
		_Builder.Append(id);
		_Builder.Append(">");
		return _Builder.ToString();
	}

	public static string EndLink()
	{
		return "</link>";
	}

	public static string Bold()
	{
		return "<b>";
	}

	public static string EndBold()
	{
		return "</b>";
	}

	public static string Italic()
	{
		return "<i>";
	}

	public static string EndItalic()
	{
		return "</i>";
	}

	public static string Font(string font, string material = null)
	{
		_Builder.Clear();
		_Builder.Append("<font=\"").Append(font).Append("\"");
		if (!material.IsNullOrEmpty())
		{
			_Builder.Append(" material=\"").Append(material).Append("\"");
		}
		_Builder.Append(">");
		return _Builder.ToString();
	}

	public static string EndFont()
	{
		return "</font>";
	}
}
