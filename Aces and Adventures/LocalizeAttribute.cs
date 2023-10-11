using System;

[AttributeUsage(AttributeTargets.All)]
public class LocalizeAttribute : Attribute
{
	public bool reflectedUI { get; set; }
}
