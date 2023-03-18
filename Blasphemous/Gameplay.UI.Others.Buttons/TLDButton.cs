using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.UI.Others.Buttons;

public class TLDButton : MenuButton, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	protected Image buttonImage;

	protected Color buttonImageColor;

	protected bool enabledButton;

	protected override void InheritedAwake()
	{
		base.InheritedAwake();
		buttonImage = GetComponent<Image>();
		if (buttonImage != null)
		{
			buttonImageColor = buttonImage.color;
		}
	}

	protected override void InheritedStart()
	{
		base.InheritedStart();
		float num = Color.white.a / 2f;
		setButtonColorAlpha(num);
		if (buttonText != null)
		{
			setButtonTextColorAlpha(num);
		}
		enabledButton = false;
	}

	protected override void OnSelectInherited(BaseEventData eventData)
	{
		base.OnSelectInherited(eventData);
		if (!enabledButton)
		{
			enabledButton = true;
			showButtonFocusEffect();
		}
	}

	protected override void OnDeselectedInherited(BaseEventData eventData)
	{
		base.OnDeselectedInherited(eventData);
		if (enabledButton)
		{
			enabledButton = !enabledButton;
			showButtonFocusEffect(show: false);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!enabledButton)
		{
			enabledButton = true;
			showButtonFocusEffect();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (enabledButton)
		{
			enabledButton = !enabledButton;
			showButtonFocusEffect(show: false);
		}
	}

	private void showButtonFocusEffect(bool show = true)
	{
		if (show)
		{
			float a = Color.white.a;
			setButtonColorAlpha(a);
			setButtonTextColorAlpha(a);
		}
		else
		{
			float num = Color.white.a / 2f;
			setButtonColorAlpha(num);
			setButtonTextColorAlpha(num);
		}
	}

	private void setButtonColorAlpha(float colorAlpha)
	{
		buttonImageColor.a = colorAlpha;
		buttonImage.color = buttonImageColor;
	}

	private void setButtonTextColorAlpha(float textAlpha)
	{
		Color color = buttonText.color;
		color.a = textAlpha;
		buttonText.color = color;
	}
}
