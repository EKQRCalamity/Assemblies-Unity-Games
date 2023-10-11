using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class UIFieldKeyAttribute : UIFieldAttribute
{
	public UIElementType inCollection { get; set; }

	public float flexibleWidth { get; set; }

	public UIFieldKeyAttribute()
	{
		flexibleWidth = 1f;
	}

	public UIFieldKeyAttribute(string label, uint order = 0u, object min = null, object max = null, object stepSize = null, object defaultValue = null, string category = null, string view = null, bool validateOnChange = false, string[] dependentOn = null, int maxCount = 5, bool showAddData = false, object filter = null)
		: base(label, order, min, max, stepSize, defaultValue, category, view, validateOnChange, dependentOn, maxCount, showAddData, filter)
	{
		flexibleWidth = 1f;
	}
}
