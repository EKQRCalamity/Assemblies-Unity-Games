using System;

[AttributeUsage(AttributeTargets.All)]
public class DefaultEnumValueAttribute : Attribute
{
	public string category;

	public DefaultEnumValueAttribute(string category = "")
	{
		this.category = category;
	}
}
