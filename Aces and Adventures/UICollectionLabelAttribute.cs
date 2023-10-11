using System;
using System.Reflection;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class UICollectionLabelAttribute : Attribute
{
	public string label { get; set; }

	public string dynamicLabelMethod { get; set; }

	public UICollectionLabelAttribute(string label, string dynamicLabelMethod = null)
	{
		this.label = label;
		this.dynamicLabelMethod = dynamicLabelMethod;
	}

	public string GetLabel(object obj)
	{
		string text = null;
		if (obj != null && !dynamicLabelMethod.IsNullOrEmpty())
		{
			text = obj.InvokeMethod<string>(dynamicLabelMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, checkProperties: true, Array.Empty<object>());
		}
		if (!text.IsNullOrEmpty())
		{
			return text;
		}
		return label;
	}
}
