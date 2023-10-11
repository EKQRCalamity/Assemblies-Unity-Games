using UnityEngine;

[DisallowMultipleComponent]
public class RegisteredEnum : MonoBehaviour
{
	public string valueNameOverride;

	[ShowOnly]
	public int value;

	public string category;

	[SerializeField]
	protected bool _isDefaultValue;

	[SerializeField]
	protected string _defaultValueCategory;

	public string enumValueName => (valueNameOverride.IsNullOrEmpty() ? base.gameObject.name : valueNameOverride).PascalCaseFromFriendly();

	public string categoryName => (category.IsNullOrEmpty() ? null : category).PascalCaseFromFriendly();

	public string defaultCategory
	{
		get
		{
			if (!_isDefaultValue)
			{
				return null;
			}
			return _defaultValueCategory ?? "";
		}
	}
}
