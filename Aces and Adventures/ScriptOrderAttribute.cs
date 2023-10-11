using System;

public class ScriptOrderAttribute : Attribute
{
	public int order;

	public ScriptOrderAttribute(int order)
	{
		this.order = order;
	}
}
