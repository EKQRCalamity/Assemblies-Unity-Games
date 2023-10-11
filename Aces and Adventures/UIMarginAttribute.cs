using System;

public class UIMarginAttribute : Attribute
{
	public float amount { get; set; }

	public bool shouldApplyIfFirstElement { get; set; }

	public UIMarginAttribute(float amount = 24f, bool shouldApplyIfFirstElement = false)
	{
		this.amount = amount;
		this.shouldApplyIfFirstElement = shouldApplyIfFirstElement;
	}
}
