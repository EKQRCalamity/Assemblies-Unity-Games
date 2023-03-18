using UnityEngine;

namespace Gameplay.UI.Others.Buttons;

public class ButtonColor : MonoBehaviour
{
	public Color textColorDefault;

	public Color textColorHighlighted;

	public Color textColorDisabled;

	public Color GetColor(bool highlighted, bool interactable)
	{
		Color result = ((!highlighted) ? textColorDefault : textColorHighlighted);
		if (!interactable)
		{
			result = textColorDisabled;
		}
		return result;
	}
}
