using System;

[AttributeUsage(AttributeTargets.Enum)]
public class UISortEnumAttribute : Attribute
{
	public UISortEnumType type { get; set; }

	public UISortEnumAttribute()
	{
		type = UISortEnumType.Alphabetical;
	}

	public UISortEnumAttribute(UISortEnumType type)
	{
		this.type = type;
	}
}
