using System;
using DG.Tweening;
using FMODUnity;
using Framework.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.UI.Others.Buttons;

public class MenuButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IMoveHandler, IEventSystemHandler
{
	public enum BUTTON_MOVEMENT_SOUND
	{
		CLASSIC,
		NONE,
		HORIZONTAL,
		VERTICAL,
		BOTH
	}

	public Text buttonText;

	protected Button myButton;

	protected EventsButton myEventsButton;

	public Color textColorDefault;

	public Color textColorHighlighted;

	public Color textColorDisabled;

	public bool ChangeText = true;

	public bool ChangeAllTexts;

	public GameObject selectedChild;

	public GameObject selectedChild2;

	public BUTTON_MOVEMENT_SOUND whenToSoundOnMove;

	[EventRef]
	public string OnMoveAudio;

	[EventRef]
	public string OnClickAudio;

	public UnityEvent OnSelectAction;

	private bool aloneButton;

	public bool useDisplacement;

	public RectTransform displacementRect;

	public Vector2 displacement;

	private Vector2 initPos;

	public event Action<MenuButton> OnMenuButtonSelected;

	public void Awake()
	{
		myButton = GetComponent<Button>();
		myEventsButton = GetComponent<EventsButton>();
		aloneButton = false;
		if (ChangeAllTexts)
		{
			ChangeAllTextInternal(highlighted: false);
		}
		if (useDisplacement)
		{
			initPos = displacementRect.anchoredPosition;
		}
		if ((bool)myEventsButton)
		{
			myEventsButton.onClick.AddListener(TaskOnClick);
			aloneButton = IsAloneButton(myEventsButton.navigation);
			if (ChangeText && buttonText == null)
			{
				buttonText = myEventsButton.GetComponentInChildren<Text>(includeInactive: true);
			}
		}
		else
		{
			myButton.onClick.AddListener(TaskOnClick);
			aloneButton = IsAloneButton(myButton.navigation);
			if (ChangeText && buttonText == null)
			{
				buttonText = myButton.GetComponentInChildren<Text>(includeInactive: true);
			}
		}
		InheritedAwake();
	}

	protected virtual void InheritedAwake()
	{
	}

	protected virtual void InheritedStart()
	{
	}

	protected virtual void OnSelectInherited(BaseEventData eventData)
	{
	}

	protected virtual void OnDeselectedInherited(BaseEventData eventData)
	{
	}

	protected void Start()
	{
		InheritedStart();
	}

	protected void TaskOnClick()
	{
		if (OnClickAudio != string.Empty)
		{
			Core.Audio.PlayOneShot(OnClickAudio);
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if ((bool)buttonText)
		{
			buttonText.color = textColorHighlighted;
		}
		if (ChangeAllTexts)
		{
			ChangeAllTextInternal(highlighted: true);
		}
		if ((bool)selectedChild)
		{
			selectedChild.SetActive(value: true);
		}
		if ((bool)selectedChild2)
		{
			selectedChild2.SetActive(value: true);
		}
		OnSelectInherited(eventData);
		if (useDisplacement)
		{
			displacementRect.DOAnchorPos(initPos + displacement, 0.5f).SetEase(Ease.InOutQuad);
		}
		if (this.OnMenuButtonSelected != null)
		{
			this.OnMenuButtonSelected(this);
		}
		if (OnSelectAction != null)
		{
			OnSelectAction.Invoke();
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if ((bool)buttonText)
		{
			buttonText.color = textColorDefault;
		}
		if (ChangeAllTexts)
		{
			ChangeAllTextInternal(highlighted: false);
		}
		if ((bool)selectedChild)
		{
			selectedChild.SetActive(value: false);
		}
		if ((bool)selectedChild2)
		{
			selectedChild2.SetActive(value: false);
		}
		OnDeselectedInherited(eventData);
		if (useDisplacement)
		{
			displacementRect.DOAnchorPos(initPos, 0.2f).SetEase(Ease.InOutQuad);
		}
	}

	private void ChangeAllTextInternal(bool highlighted)
	{
		bool flag = true;
		if ((bool)myButton)
		{
			flag = myButton.interactable;
		}
		else if ((bool)myEventsButton)
		{
			flag = myEventsButton.interactable;
		}
		Text[] componentsInChildren = GetComponentsInChildren<Text>(includeInactive: true);
		foreach (Text text in componentsInChildren)
		{
			Color color = ((!highlighted) ? textColorDefault : textColorHighlighted);
			if (!flag)
			{
				color = textColorDisabled;
			}
			ButtonColor component = text.GetComponent<ButtonColor>();
			if ((bool)component)
			{
				color = component.GetColor(highlighted, flag);
			}
			text.color = color;
		}
	}

	public void OnMove(AxisEventData eventData)
	{
		if (!string.IsNullOrEmpty(OnMoveAudio) && whenToSoundOnMove != BUTTON_MOVEMENT_SOUND.NONE)
		{
			bool flag = false;
			if (whenToSoundOnMove == BUTTON_MOVEMENT_SOUND.CLASSIC)
			{
				flag = OnMoveAudio != string.Empty && !aloneButton;
			}
			else
			{
				bool flag2 = eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Right;
				bool flag3 = eventData.moveDir == MoveDirection.Up || eventData.moveDir == MoveDirection.Down;
				flag = whenToSoundOnMove == BUTTON_MOVEMENT_SOUND.BOTH || (whenToSoundOnMove == BUTTON_MOVEMENT_SOUND.HORIZONTAL && flag2) || (whenToSoundOnMove == BUTTON_MOVEMENT_SOUND.VERTICAL && flag3);
			}
			if (flag)
			{
				Core.Audio.PlayOneShot(OnMoveAudio);
			}
		}
	}

	private bool IsAloneButton(Navigation nav)
	{
		bool result = false;
		if (nav.mode == Navigation.Mode.Explicit)
		{
			result = nav.selectOnDown == null && nav.selectOnLeft == null && nav.selectOnRight == null && nav.selectOnUp == null;
		}
		return result;
	}
}
