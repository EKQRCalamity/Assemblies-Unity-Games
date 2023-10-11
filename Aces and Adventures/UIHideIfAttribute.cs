using System;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
public class UIHideIfAttribute : Attribute
{
	public string methodName { get; set; }

	public UIHideIfAttribute(string methodName)
	{
		this.methodName = methodName;
	}
}
