using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Card Layout/CardLayoutSpringThresholds")]
public class CardLayoutSpringThresholds : ScriptableObject
{
	public TransformSpringThresholds settings;

	public float distance => settings.distance;

	public float degrees => settings.degrees;

	public float scale => settings.scale;

	public bool useScaleThreshold => settings.useScaleThreshold;

	public Vector3? perAxisDistance
	{
		get
		{
			if (!settings.usePerAxisDistance)
			{
				return null;
			}
			return settings.perAxisDistance;
		}
	}

	public static implicit operator TransformSpringThresholds(CardLayoutSpringThresholds settings)
	{
		return settings.settings;
	}
}
