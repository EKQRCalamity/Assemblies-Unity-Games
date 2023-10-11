using UnityEngine;
using UnityEngine.Localization;

public class LocalizedStringRef : MonoBehaviour
{
	private LocalizedString _localizedString;

	public LocalizedString localizedString => _localizedString;

	public LocalizedStringRef SetData(LocalizedString localizedStringToSet, (LocalizedVariableName, object)[] variables = null)
	{
		_localizedString = ((variables != null) ? GameUtil.JsonClone(localizedStringToSet) : localizedStringToSet);
		if (variables != null)
		{
			localizedString.SetVariables(variables);
		}
		return this;
	}
}
