using System;

[AttributeUsage(AttributeTargets.All)]
public class UITooltipAttribute : Attribute
{
	public string tooltip;

	public UITooltipAttribute(string tooltip)
	{
		this.tooltip = tooltip;
	}

	public static implicit operator string(UITooltipAttribute tooltip)
	{
		return tooltip?.tooltip;
	}
}
