using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Card Layout/CardLayoutRandomSettings")]
public class CardLayoutRandomSettings : ScriptableObject
{
	public float rotation = 5f;

	public float rotationFrequency = 1f;

	public float position = 0.01f;

	public float positionFrequency = 1f;

	public bool applyToAllAxes;

	public bool onlyApplyToCardsWithActiveOffset;

	public bool sameForAllInLayout;

	public bool disableWhenInputActive;

	[SerializeField]
	protected float _layoutRestTime = -1f;

	public float? layoutRestTime
	{
		get
		{
			if (!(_layoutRestTime >= 0f))
			{
				return null;
			}
			return _layoutRestTime;
		}
	}
}
