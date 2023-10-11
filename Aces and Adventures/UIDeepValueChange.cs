using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class UIDeepValueChange : Attribute
{
	public bool updateLabel { get; set; }

	public UIDeepValueChange()
	{
		updateLabel = true;
	}

	public UIDeepValueChange(bool updateLabel)
	{
		this.updateLabel = updateLabel;
	}
}
