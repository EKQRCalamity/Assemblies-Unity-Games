using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.Dialog;

public class BaseObject : ScriptableObject
{
	[OnValueChanged("OnIdChanged", false)]
	public string id = string.Empty;

	public string sortDescription = string.Empty;

	[TextArea(3, 10)]
	public string description = string.Empty;

	[HideInInspector]
	public string translationCategory = string.Empty;

	public void OnIdChanged(string value)
	{
		id = value.Replace(' ', '_').ToUpper();
		string prefix = GetPrefix();
		if (prefix.Length > 0 && !id.StartsWith(prefix))
		{
			id = prefix + id;
		}
	}

	public virtual string GetPrefix()
	{
		return string.Empty;
	}

	public string GetBaseTranslationID()
	{
		if (translationCategory.Length > 0)
		{
			return translationCategory + "/" + id;
		}
		return id;
	}
}
