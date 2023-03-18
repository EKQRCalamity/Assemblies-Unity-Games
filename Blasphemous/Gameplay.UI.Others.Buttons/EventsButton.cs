using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Gameplay.UI.Others.Buttons;

public class EventsButton : Selectable, IPointerClickHandler, ISubmitHandler, IEventSystemHandler
{
	[Serializable]
	public class ButtonSelectedEvent : UnityEvent
	{
	}

	[Serializable]
	public class ButtonClickedEvent : UnityEvent
	{
	}

	[FormerlySerializedAs("onSelected")]
	[SerializeField]
	private ButtonSelectedEvent m_onSelected = new ButtonSelectedEvent();

	[FormerlySerializedAs("onClick")]
	[SerializeField]
	private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();

	public ButtonSelectedEvent onSelected
	{
		get
		{
			return m_onSelected;
		}
		set
		{
			m_onSelected = value;
		}
	}

	public ButtonClickedEvent onClick
	{
		get
		{
			return m_OnClick;
		}
		set
		{
			m_OnClick = value;
		}
	}

	protected EventsButton()
	{
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (IsActive() && IsInteractable())
		{
			m_onSelected.Invoke();
		}
	}

	private void Press()
	{
		if (IsActive() && IsInteractable())
		{
			m_OnClick.Invoke();
		}
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			Press();
		}
	}

	public virtual void OnSubmit(BaseEventData eventData)
	{
		Press();
		if (IsActive() && IsInteractable())
		{
			DoStateTransition(SelectionState.Pressed, instant: false);
			StartCoroutine(OnFinishSubmit());
		}
	}

	private IEnumerator OnFinishSubmit()
	{
		float fadeTime = base.colors.fadeDuration;
		float elapsedTime = 0f;
		while (elapsedTime < fadeTime)
		{
			elapsedTime += Time.unscaledDeltaTime;
			yield return null;
		}
		DoStateTransition(base.currentSelectionState, instant: false);
	}
}
