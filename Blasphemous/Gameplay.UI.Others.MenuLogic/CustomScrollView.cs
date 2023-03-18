using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class CustomScrollView : MonoBehaviour
{
	public enum AxisEnum
	{
		InventoryScroll,
		Movement,
		Both
	}

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	public ScrollRect scrollRect;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	public FixedScrollBar scrollBar;

	[SerializeField]
	[BoxGroup("Params", true, false, 0)]
	private float scrollBarSpeed = 200f;

	[SerializeField]
	[BoxGroup("Params", true, false, 0)]
	private float inputThreshold = 0.1f;

	[SerializeField]
	[BoxGroup("Params", true, false, 0)]
	private AxisEnum axis;

	private const int SIZE_EPSILON = 6;

	public bool InputEnabled { get; set; }

	public bool ScrollBarNeeded { get; private set; }

	public void NewContentSetted()
	{
		if (scrollRect.content != null)
		{
			VerticalLayoutGroup component = scrollRect.content.GetComponent<VerticalLayoutGroup>();
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)component.transform);
		}
		RectTransform rectTransform = (RectTransform)scrollRect.transform;
		RectTransform rectTransform2 = (RectTransform)scrollRect.content.transform;
		ScrollBarNeeded = rectTransform2.rect.height + 6f >= rectTransform.rect.height;
		float num = 1f;
		scrollRect.verticalNormalizedPosition = num;
		if (scrollBar != null)
		{
			scrollBar.SetScrollbar(1f - num);
			scrollBar.SetEnabled(ScrollBarNeeded);
		}
	}

	private void Awake()
	{
		InputEnabled = true;
	}

	private void Update()
	{
		if (!(scrollRect == null) && InputEnabled && ScrollBarNeeded)
		{
			float num = 0f;
			Player player = ReInput.players.GetPlayer(0);
			num = (player.GetButton(43) ? 1f : ((!player.GetButton(45)) ? 0f : (-1f)));
			float num2 = player.GetAxis(49);
			if (axis == AxisEnum.Movement || (axis == AxisEnum.Both && Mathf.Abs(num2) > inputThreshold))
			{
				num = num2;
			}
			if (num > inputThreshold || num < 0f - inputThreshold)
			{
				RectTransform rectTransform = (RectTransform)scrollRect.content.transform;
				float num3 = 1f / rectTransform.rect.height;
				float value = scrollRect.verticalNormalizedPosition + num * scrollBarSpeed * Time.unscaledDeltaTime * num3;
				value = Mathf.Clamp01(value);
				scrollRect.verticalNormalizedPosition = value;
			}
			if (scrollBar != null)
			{
				scrollBar.SetScrollbar(1f - scrollRect.verticalNormalizedPosition);
			}
		}
	}
}
