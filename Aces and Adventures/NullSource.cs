using System;
using UnityEngine.Localization.SmartFormat.Core.Extensions;

[Serializable]
public class NullSource : ISource
{
	public const string NULL = " ";

	public string selector = "null";

	public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
	{
		if (selectorInfo.SelectorText != selector)
		{
			return false;
		}
		selectorInfo.Result = " ";
		return true;
	}
}
