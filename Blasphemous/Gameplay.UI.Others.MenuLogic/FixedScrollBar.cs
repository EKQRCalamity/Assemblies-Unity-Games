using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.UI.Others.MenuLogic;

public class FixedScrollBar : MonoBehaviour
{
	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private RectTransform barElement;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private RectTransform handleElement;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private float alphaWhenDisabled = 0.7f;

	public void SetEnabled(bool enabled)
	{
		CanvasGroup component = GetComponent<CanvasGroup>();
		if ((bool)component)
		{
			component.alpha = ((!enabled) ? alphaWhenDisabled : 1f);
		}
	}

	public void SetScrollbar(float percent)
	{
		float num = percent * (barElement.rect.height - handleElement.rect.height);
		num -= barElement.rect.height / 2f;
		handleElement.localPosition = new Vector3(handleElement.localPosition.x, 0f - num, handleElement.localPosition.z);
	}
}
