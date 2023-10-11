using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Card Layout/CardLayoutSpringSettings")]
public class CardLayoutSpringSettings : ScriptableObject
{
	public TransformSpringSettings settings;

	public static implicit operator TransformSpringSettings(CardLayoutSpringSettings settings)
	{
		return settings.settings;
	}
}
