using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class UIFieldCollectionItemAttribute : UIFieldAttribute
{
	public UIFieldCollectionItemAttribute()
	{
	}

	public UIFieldCollectionItemAttribute(string label, uint order = 0u, object min = null, object max = null, object stepSize = null, object defaultValue = null, string category = null, string view = null, bool validateOnChange = false, string[] dependentOn = null, int maxCount = 5, bool showAddData = false, object filter = null)
		: base(label, order, min, max, stepSize, defaultValue, category, view, validateOnChange, dependentOn, maxCount, showAddData, filter)
	{
	}
}
