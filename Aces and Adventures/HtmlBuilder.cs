using UnityEngine;

public class HtmlBuilder
{
	public enum PadType
	{
		left,
		right,
		bottom,
		top
	}

	public enum UnitType
	{
		em,
		px,
		IN,
		cm,
		mm,
		pt,
		pc
	}

	private TextBuilder _builder;

	public TextBuilder builder => _builder ?? (_builder = new TextBuilder());

	public HtmlBuilder Append(string s)
	{
		builder.Append(s);
		return this;
	}

	public HtmlBuilder Bold()
	{
		builder.Bold();
		return this;
	}

	public HtmlBuilder EndBold()
	{
		builder.EndBold();
		return this;
	}

	public HtmlBuilder Italic()
	{
		builder.Italic();
		return this;
	}

	public HtmlBuilder EndItalic()
	{
		builder.EndItalic();
		return this;
	}

	public HtmlBuilder Mark()
	{
		builder.Append("<mark>");
		return this;
	}

	public HtmlBuilder EndMark()
	{
		builder.Append("</mark>");
		return this;
	}

	public HtmlBuilder Underline()
	{
		builder.Append("<ins>");
		return this;
	}

	public HtmlBuilder EndUnderline()
	{
		builder.Append("</ins>");
		return this;
	}

	public HtmlBuilder Small()
	{
		builder.Append("<small>");
		return this;
	}

	public HtmlBuilder EndSmall()
	{
		builder.Append("</small>");
		return this;
	}

	public HtmlBuilder Quote()
	{
		builder.Append("<q>");
		return this;
	}

	public HtmlBuilder EndQuote()
	{
		builder.Append("</q>");
		return this;
	}

	public HtmlBuilder Color(Color32 color, bool background = false)
	{
		builder.Append("<span style=\"").Append(background.ToText("background-")).Append("color:#")
			.Append(TextMeshHelper.ColorHex(color))
			.Append(";\">");
		return this;
	}

	public HtmlBuilder EndColor()
	{
		builder.Append("</span>");
		return this;
	}

	public HtmlBuilder BeginUnorderedList()
	{
		builder.Append("<ul>");
		return this;
	}

	public HtmlBuilder EndUnorderedList()
	{
		builder.Append("</ul>");
		return this;
	}

	public HtmlBuilder ListItem()
	{
		builder.Append("<li>");
		return this;
	}

	public HtmlBuilder EndListItem()
	{
		builder.Append("</li>");
		return this;
	}

	public HtmlBuilder BeginOrderedList()
	{
		builder.Append("<ol>");
		return this;
	}

	public HtmlBuilder EndOrderedList()
	{
		builder.Append("</ol>");
		return this;
	}

	public HtmlBuilder Header()
	{
		builder.Append("<h1>");
		return this;
	}

	public HtmlBuilder EndHeader()
	{
		builder.Append("</h1>");
		return this;
	}

	public HtmlBuilder Title()
	{
		builder.Append("<title>");
		return this;
	}

	public HtmlBuilder EndTitle()
	{
		builder.Append("</title>");
		return this;
	}

	public HtmlBuilder Tab()
	{
		builder.Append("&emsp;");
		return this;
	}

	public HtmlBuilder Paragraph()
	{
		builder.Append("<p>");
		return this;
	}

	public HtmlBuilder EndParagraph()
	{
		builder.Append("</p>");
		return this;
	}

	public HtmlBuilder BlockQuote()
	{
		builder.Append("<blockquote>");
		return this;
	}

	public HtmlBuilder EndBlockQuote()
	{
		builder.Append("</blockquote>");
		return this;
	}

	public HtmlBuilder Pad(int amount = 1, PadType edge = PadType.left, UnitType unit = UnitType.em, bool usePercent = false)
	{
		builder.Append("<div style=\"padding-").Append(EnumUtil.FriendlyName(edge, uppercase: false)).Append(": ")
			.Append(amount)
			.Append(usePercent ? "%" : EnumUtil.FriendlyName(unit, uppercase: false))
			.Append(";\">");
		return this;
	}

	public HtmlBuilder EndPad()
	{
		builder.Append("</div>");
		return this;
	}

	public HtmlBuilder NewLine()
	{
		builder.Append("<br>");
		return this;
	}

	public HtmlBuilder RemoveNewLine()
	{
		builder.RemoveFromEnd(4);
		return this;
	}

	public HtmlBuilder Preformatted()
	{
		builder.Append("<pre>");
		return this;
	}

	public HtmlBuilder EndPreformatted()
	{
		builder.Append("</pre>");
		return this;
	}

	public void Clear(bool clearCapacity = false)
	{
		builder.Clear(clearCapacity);
	}

	public static implicit operator TextBuilder(HtmlBuilder htmlBuilder)
	{
		return htmlBuilder?.builder;
	}

	public static implicit operator string(HtmlBuilder htmlBuilder)
	{
		if (htmlBuilder == null)
		{
			return "";
		}
		return htmlBuilder.builder.ToString();
	}

	public override string ToString()
	{
		return this;
	}
}
