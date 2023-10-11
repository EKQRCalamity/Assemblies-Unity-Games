using UnityEngine;

public class UnityInspectorButtonAttribute : PropertyAttribute
{
	public string methodName { get; set; }

	public UnityInspectorButtonAttribute(string methodName)
	{
		this.methodName = methodName;
	}
}
