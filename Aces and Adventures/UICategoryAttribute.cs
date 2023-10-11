using System;

[AttributeUsage(AttributeTargets.All)]
public class UICategoryAttribute : Attribute
{
	public string category { get; set; }

	public UICategoryAttribute(string category)
	{
		this.category = category;
	}
}
