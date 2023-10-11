using UnityEngine;

public class HideInInspectorIf : PropertyAttribute
{
	public string hideIfMethodName;

	public bool showOnly;

	public HideInInspectorIf(string hideIfMethodName, bool showOnly = false)
	{
		this.hideIfMethodName = hideIfMethodName;
		this.showOnly = showOnly;
	}
}
