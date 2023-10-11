using System;
using System.Text;
using UnityEngine;

public class TextBuilder
{
	public const string FLOAT_TINY_FORMAT = "#.##;-#.##;0";

	public const string FLOAT_SMALL_FORMAT = "#.#;-#.#;0";

	public const string FLOAT_LARGE_FORMAT = "#.;-#.;0";

	public const string FLOAT_PERCENT_FORMAT = "#;-#;0";

	public const string FLOAT_SMALL_PERCENT_FORMAT = "#.#;-#.#;0";

	public const string FLOAT_TINY_PERCENT_FORMAT = "#.##;-#.##;0";

	public const string RANGE_DELIM = "~";

	public static int SMALL_SIZE_PERCENT = 66;

	public static string SMALL_SIZE_TAG = "<size=" + SMALL_SIZE_PERCENT + "%>";

	public static int MED_SIZE_PERCENT = 75;

	public static string MED_SIZE_TAG = "<size=" + MED_SIZE_PERCENT + "%>";

	protected StringBuilder _builder;

	protected bool _clearOnToString;

	public int length => _builder.Length;

	public TextBuilder(bool clearOnToString = false)
	{
		_builder = new StringBuilder();
		_clearOnToString = clearOnToString;
	}

	public void Clear(bool clearCapacity = false)
	{
		_builder.Clear(clearCapacity);
	}

	public TextBuilder Append(string s)
	{
		_builder.Append(s);
		return this;
	}

	public TextBuilder Prepend(string s)
	{
		_builder.Insert(0, s);
		return this;
	}

	public void AppendWithSpace(string s, bool prependSpace = false, bool appendSpace = true)
	{
		Space(prependSpace);
		_builder.Append(s);
		Space(appendSpace);
	}

	public TextBuilder AppendSpaced(string s, bool prependSpace = true, bool appendSpace = true)
	{
		if (s.IsNullOrEmpty())
		{
			return this;
		}
		Space(prependSpace);
		_builder.Append(s);
		Space(appendSpace);
		return this;
	}

	public TextBuilder AppendSpacedSmart(string s, bool prependSpace = false, bool appendSpace = true, bool appendPeriod = false, bool newLine = false)
	{
		if (s.IsNullOrEmpty())
		{
			return this;
		}
		if (newLine)
		{
			NewLine();
		}
		if (prependSpace)
		{
			Space();
		}
		_builder.Append(s);
		if (appendPeriod)
		{
			Period();
		}
		if (appendSpace)
		{
			Space();
		}
		return this;
	}

	public void Append(char c)
	{
		_builder.Append(c);
	}

	public TextBuilder Append(int i)
	{
		_builder.Append(i);
		return this;
	}

	public TextBuilder Append(long l, string format = "N0")
	{
		_builder.Append((format != null) ? l.ToString(format) : l.ToString());
		return this;
	}

	public TextBuilder Append(float f)
	{
		_builder.Append(f);
		return this;
	}

	public TextBuilder AppendFloat(float f, bool isPercent)
	{
		Append(_FloatToString(f, isPercent));
		if (isPercent)
		{
			Append("%");
		}
		return this;
	}

	public TextBuilder AppendFloat(float f, string format)
	{
		return Append(f.ToString(format));
	}

	public TextBuilder AppendPercent(float f, string format = "#;-#;0")
	{
		_builder.Append((f * 100f).ToString(format));
		_builder.Append("%");
		return this;
	}

	public TextBuilder Append(object o)
	{
		_builder.Append(o);
		return this;
	}

	public void Append(char[] c)
	{
		_builder.Append(c);
	}

	public TextBuilder Pluralize(string s, int count)
	{
		_builder.Append(s);
		if (count > 1)
		{
			_builder.Append("s");
		}
		return this;
	}

	public TextBuilder Range(int min, int max, string delimiter = "~")
	{
		if (min != max)
		{
			_builder.Append(min);
			_builder.Append(delimiter);
			_builder.Append(max);
		}
		else
		{
			_builder.Append(min);
		}
		return this;
	}

	public TextBuilder RangeF(float min, float max, bool isPercent, string delimiter = "~", string formatOverride = null)
	{
		if (Math.Abs(min - max) > MathUtil.BigEpsilon)
		{
			_builder.Append(_FloatToString(min, isPercent, formatOverride));
			_builder.Append(delimiter);
			_builder.Append(_FloatToString(max, isPercent, formatOverride));
		}
		else
		{
			_builder.Append(_FloatToString(min, isPercent, formatOverride));
		}
		if (isPercent)
		{
			_builder.Append("%");
		}
		return this;
	}

	public void RangeF(RangeF range, bool isPercent, string delimiter = "~")
	{
		bool flag = isPercent;
		if (range.min == range.minRange)
		{
			_builder.Append("below ");
			_builder.Append(_FloatToString(range.max, isPercent));
		}
		else if (range.max == range.maxRange)
		{
			_builder.Append("above ");
			_builder.Append(_FloatToString(range.min, isPercent));
		}
		else if (range.min == range.max)
		{
			_builder.Append(_FloatToString(range.max, isPercent));
		}
		else
		{
			_builder.Append("between ");
			RangeF(range.min, range.max, isPercent, delimiter);
			flag = false;
		}
		if (flag)
		{
			_builder.Append("%");
		}
	}

	public TextBuilder Space(bool add = true)
	{
		if (add)
		{
			_builder.Append(" ");
		}
		return this;
	}

	public TextBuilder Period()
	{
		_builder.Append(".");
		return this;
	}

	public TextBuilder PlusSign(bool add = true)
	{
		if (add)
		{
			_builder.Append("+");
		}
		return this;
	}

	public TextBuilder MinusSign(bool add = true)
	{
		if (add)
		{
			_builder.Append("-");
		}
		return this;
	}

	public TextBuilder Sign(float value)
	{
		Append((value >= 0f) ? "+" : "-");
		return this;
	}

	public TextBuilder TrimEnd()
	{
		if (_builder.Length == 0)
		{
			return this;
		}
		int num = _builder.Length - 1;
		while (num >= 0 && char.IsWhiteSpace(_builder[num]))
		{
			num--;
		}
		if (num < _builder.Length - 1)
		{
			_builder.Length = num + 1;
		}
		return this;
	}

	public override string ToString()
	{
		string result = _builder.ToString();
		if (_clearOnToString)
		{
			Clear();
		}
		return result;
	}

	protected string _FloatToString(float f, bool isPercent, string formatOverride = null)
	{
		float num = Mathf.Abs(f);
		string format = formatOverride ?? ((!isPercent) ? ((!(num < 10f)) ? "#.;-#.;0" : ((num < 1f) ? "#.##;-#.##;0" : "#.#;-#.#;0")) : ((!(num < 0.1f)) ? "#;-#;0" : ((num < 0.01f) ? "#.##;-#.##;0" : "#.#;-#.#;0")));
		return (isPercent ? (f * 100f) : f).ToStringRoundZero(format);
	}

	public TextBuilder NewLine()
	{
		_builder.Append("\n");
		return this;
	}

	public TextBuilder RemoveNewLine()
	{
		if (_builder.Length > 0 && _builder[_builder.Length - 1] == '\n')
		{
			RemoveFromEnd(1);
		}
		return this;
	}

	public TextBuilder Color(Color32 color32)
	{
		_builder.Append(TextMeshHelper.Color(color32));
		return this;
	}

	public TextBuilder EndColor()
	{
		_builder.Append("</color>");
		return this;
	}

	public TextBuilder Alpha(byte alpha)
	{
		_builder.Append("<alpha=#");
		_builder.Append(TextMeshHelper.ToHex(alpha));
		_builder.Append(">");
		return this;
	}

	public TextBuilder EndAlpha()
	{
		_builder.Append("</color>");
		return this;
	}

	public void Link(int id)
	{
		_builder.Append(TextMeshHelper.Link(id));
	}

	public void EndLink()
	{
		_builder.Append("</link>");
	}

	public TextBuilder Bold()
	{
		_builder.Append("<b>");
		return this;
	}

	public TextBuilder EndBold()
	{
		_builder.Append("</b>");
		return this;
	}

	public TextBuilder Italic()
	{
		_builder.Append("<i>");
		return this;
	}

	public TextBuilder EndItalic()
	{
		_builder.Append("</i>");
		return this;
	}

	public TextBuilder Align(TextBuilderAlign align)
	{
		_builder.Append("<align=");
		_builder.Append(align.Text());
		_builder.Append(">");
		return this;
	}

	public void EndAlign()
	{
		_builder.Append("</align>");
	}

	public TextBuilder Underline()
	{
		_builder.Append("<u>");
		return this;
	}

	public TextBuilder EndUnderline()
	{
		_builder.Append("</u>");
		return this;
	}

	public TextBuilder StrikeThrough()
	{
		_builder.Append("<s>");
		return this;
	}

	public TextBuilder EndStrikeThrough()
	{
		_builder.Append("</s>");
		return this;
	}

	public void Lowercase()
	{
		_builder.Append("<lowercase>");
	}

	public void EndLowercase()
	{
		_builder.Append("</lowercase>");
	}

	public void Uppercase()
	{
		_builder.Append("<uppercase>");
	}

	public void EndUppercase()
	{
		_builder.Append("</uppercase>");
	}

	public void SmallCaps()
	{
		_builder.Append("<smallcaps>");
	}

	public void EndSmallCaps()
	{
		_builder.Append("</smallcaps>");
	}

	public TextBuilder UnderlineSmallCaps()
	{
		_builder.Append("<u><smallcaps>");
		return this;
	}

	public TextBuilder EndUnderlineSmallCaps()
	{
		_builder.Append("</u></smallcaps>");
		return this;
	}

	public TextBuilder TutorialStyle()
	{
		return UnderlineSmallCaps();
	}

	public TextBuilder EndTutorialStyle()
	{
		return EndUnderlineSmallCaps();
	}

	public TextBuilder TutorialSize()
	{
		return MedSize();
	}

	public TextBuilder NodeDescriptionSize()
	{
		return SmallSize();
	}

	public TextBuilder RecipeSize()
	{
		return SmallSize();
	}

	public TextBuilder Size(int size)
	{
		_builder.Append("<size=");
		_builder.Append(size);
		_builder.Append(">");
		return this;
	}

	public void SizeDelta(int size)
	{
		_builder.Append("<size=");
		if (size > 0)
		{
			_builder.Append("+");
		}
		_builder.Append(size);
		_builder.Append(">");
	}

	public TextBuilder SizePercent(int percent)
	{
		_builder.Append("<size=");
		_builder.Append(percent);
		_builder.Append("%>");
		return this;
	}

	public TextBuilder SmallSize()
	{
		_builder.Append(SMALL_SIZE_TAG);
		return this;
	}

	public TextBuilder MedSize()
	{
		_builder.Append(MED_SIZE_TAG);
		return this;
	}

	public TextBuilder EndSize()
	{
		_builder.Append("</size>");
		return this;
	}

	public void Subscript()
	{
		_builder.Append("<sub>");
	}

	public void EndSubscript()
	{
		_builder.Append("</sub>");
	}

	public void Superscript()
	{
		_builder.Append("<sup>");
	}

	public void EndSuperscript()
	{
		_builder.Append("</sup>");
	}

	public void Indent(int pixels)
	{
		_builder.Append("<indent=");
		_builder.Append(pixels);
		_builder.Append(">");
	}

	public void IndentPercent(int percent)
	{
		_builder.Append("<indent=");
		_builder.Append(percent);
		_builder.Append("%>");
	}

	public void EndIndent()
	{
		_builder.Append("</indent>");
	}

	public void Mark(Color32 color32)
	{
		_builder.Append("<mark=#");
		_builder.Append(TextMeshHelper.ColorHex(color32));
		_builder.Append(">");
	}

	public void EndMark()
	{
		_builder.Append("</mark>");
	}

	public TextBuilder Sprite(string spriteName, string assetName = null, Color32? tint = null, bool inheritTint = false, int? sizePercent = null)
	{
		if (sizePercent.HasValue)
		{
			SizePercent(sizePercent.Value);
		}
		_builder.Append("<sprite");
		if (!assetName.IsNullOrEmpty())
		{
			_builder.Append("=\"");
			_builder.Append(assetName);
			_builder.Append("\"");
		}
		_builder.Append(" name=\"");
		_builder.Append(spriteName);
		_builder.Append("\"");
		if (tint.HasValue)
		{
			_builder.Append(" color=#");
			_builder.Append(TextMeshHelper.ColorHex(tint.Value));
		}
		else if (inheritTint)
		{
			_builder.Append(" tint=1");
		}
		_builder.Append(">");
		if (sizePercent.HasValue)
		{
			EndSize();
		}
		return this;
	}

	public void Font(string fontName, string materialName = null)
	{
		_builder.Append("<font=\"");
		_builder.Append(fontName);
		_builder.Append("\"");
		if (!materialName.IsNullOrEmpty())
		{
			_builder.Append(" material=\"");
			_builder.Append(materialName);
			_builder.Append("\"");
		}
		_builder.Append(">");
	}

	public void EndFont()
	{
		_builder.Append("</font>");
	}

	public void Style(string styleName)
	{
		_builder.Append("<style=");
		_builder.Append(styleName);
		_builder.Append(">");
	}

	public void EndStyle()
	{
		_builder.Append("</style>");
	}

	public TextBuilder NoBreak()
	{
		_builder.Append("<nobr>");
		return this;
	}

	public TextBuilder EndNoBreak()
	{
		_builder.Append("</nobr>");
		return this;
	}

	public TextBuilder NoBreakSmart(string textToNoBreak, bool newLine)
	{
		if (textToNoBreak.IsNullOrEmpty())
		{
			return this;
		}
		if (newLine && _builder.Length > 0)
		{
			NewLine();
		}
		NoBreak();
		Append(textToNoBreak);
		EndNoBreak();
		return this;
	}

	public TextBuilder VOffset(float amount, bool isPixelOffset = false)
	{
		_builder.Append("<voffset=");
		_builder.Append(amount);
		if (!isPixelOffset)
		{
			_builder.Append("em");
		}
		_builder.Append(">");
		return this;
	}

	public TextBuilder EndVOffset()
	{
		_builder.Append("</voffset>");
		return this;
	}

	public void InsertAt(int insertIndex, string insertedText)
	{
		_builder.Insert(insertIndex, insertedText);
	}

	public void RemoveAt(int index)
	{
		Remove(index, 1);
	}

	public void Remove(int startIndex, int numCharactersToRemove)
	{
		_builder.Remove(startIndex, numCharactersToRemove);
	}

	public TextBuilder RemoveFromEnd(int numCharactersToRemove)
	{
		Remove(_builder.Length - numCharactersToRemove, numCharactersToRemove);
		return this;
	}

	public void RemoveFromEndTill(char c)
	{
		for (int num = _builder.Length - 2; num >= 0; num--)
		{
			if (_builder[num] == c)
			{
				_builder.Remove(num + 1, _builder.Length - num - 1);
				break;
			}
		}
	}

	public void RemoveFromEndTillPeriod(int stopAt = 0)
	{
		for (int num = _builder.Length - 2; num >= stopAt; num--)
		{
			if (_builder[num] == '.' && !char.IsDigit(_builder[num + 1]))
			{
				_builder.Remove(num + 1, _builder.Length - num - 1);
				break;
			}
		}
	}

	public static implicit operator string(TextBuilder builder)
	{
		return builder?.ToString();
	}
}
