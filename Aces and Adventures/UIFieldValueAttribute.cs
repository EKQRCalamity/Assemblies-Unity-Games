using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class UIFieldValueAttribute : UIFieldAttribute
{
	private bool _hideInCollectionAdd = true;

	public bool hideInCollectionAdd
	{
		get
		{
			return _hideInCollectionAdd;
		}
		set
		{
			_hideInCollectionAdd = value;
		}
	}

	public float flexibleWidth { get; set; }

	public UIFieldValueAttribute()
	{
		flexibleWidth = 1f;
	}

	public UIFieldValueAttribute(string label, uint order = 0u, object min = null, object max = null, object stepSize = null, object defaultValue = null, string category = null, string view = null, bool validateOnChange = false, string[] dependentOn = null, int maxCount = 5, bool showAddData = false, object filter = null)
		: base(label, order, min, max, stepSize, defaultValue, category, view, validateOnChange, dependentOn, maxCount, showAddData, filter)
	{
		flexibleWidth = 1f;
	}
}
