using System;
using System.Collections.Generic;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

[Serializable]
public class ArgumentSource : ISource
{
	public const string SELECTOR = "x";

	public static int GetArgumentIndex(IList<object> arguments, object currentValue = null)
	{
		if (arguments.Count == 1)
		{
			return 0;
		}
		if (!(arguments[0] is IVariable))
		{
			return 0;
		}
		return 1;
	}

	public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
	{
		if (selectorInfo.SelectorText == "x")
		{
			IList<object> originalArgs = selectorInfo.FormatDetails.OriginalArgs;
			if (originalArgs != null && originalArgs.Count > 0)
			{
				selectorInfo.Result = originalArgs[GetArgumentIndex(originalArgs, selectorInfo.CurrentValue)];
				return true;
			}
		}
		return false;
	}
}
