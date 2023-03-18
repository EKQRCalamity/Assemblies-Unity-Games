using System;
using DG.Tweening;
using Framework.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.UI.Others.Buttons;

public class ControlRemapButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IMoveHandler, IEventSystemHandler
{
	protected Text buttonText;

	protected Button myButton;

	protected EventsButton myEventsButton;

	public Color textColorDefault;

	public Color textColorHighlighted;

	public Color textColorConflictDefault;

	public Color textColorConflictHighlighted;

	public GameObject selectedChild;

	public string OnMoveAudio;

	public string OnClickAudio;

	private bool aloneButton;

	public bool useDisplacement;

	public RectTransform displacementRect;

	public Vector2 displacement;

	private Vector2 initPos;

	private bool conflict;

	public event Action<ControlRemapButton> OnControlRemapButtonSelected;

	public void Awake()
	{
		myButton = GetComponent<Button>();
		myEventsButton = GetComponent<EventsButton>();
		buttonText = null;
		aloneButton = false;
		if (useDisplacement)
		{
			initPos = displacementRect.anchoredPosition;
		}
		if ((bool)myEventsButton)
		{
			myEventsButton.onClick.AddListener(TaskOnClick);
			aloneButton = IsAloneButton(myEventsButton.navigation);
			buttonText = myEventsButton.GetComponent<Text>();
		}
		else
		{
			myButton.onClick.AddListener(TaskOnClick);
			aloneButton = IsAloneButton(myButton.navigation);
			buttonText = myButton.GetComponent<Text>();
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
			buttonText.color = ((!conflict) ? textColorHighlighted : textColorConflictHighlighted);
		}
		if ((bool)selectedChild)
		{
			selectedChild.SetActive(value: true);
		}
		OnSelectInherited(eventData);
		if (useDisplacement)
		{
			displacementRect.DOAnchorPos(initPos + displacement, 0.5f).SetEase(Ease.InOutQuad);
		}
		if (this.OnControlRemapButtonSelected != null)
		{
			this.OnControlRemapButtonSelected(this);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if ((bool)buttonText)
		{
			buttonText.color = ((!conflict) ? textColorDefault : textColorConflictDefault);
		}
		if ((bool)selectedChild)
		{
			selectedChild.SetActive(value: false);
		}
		OnDeselectedInherited(eventData);
		if (useDisplacement)
		{
			displacementRect.DOAnchorPos(initPos, 0.2f).SetEase(Ease.InOutQuad);
		}
	}

	public void OnMove(AxisEventData eventData)
	{
		if (OnMoveAudio != string.Empty && !aloneButton)
		{
			Core.Audio.PlayOneShot(OnMoveAudio);
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

	public void SetConflict(bool b)
	{
		if (conflict != b)
		{
			buttonText.color = ((!b) ? textColorDefault : textColorConflictDefault);
		}
		conflict = b;
	}

	public bool GetConflict()
	{
		return conflict;
	}
}
