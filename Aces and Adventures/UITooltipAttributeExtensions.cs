public static class UITooltipAttributeExtensions
{
	public static string GetTooltip(this UITooltipAttribute tooltip)
	{
		return tooltip?.tooltip;
	}
}
