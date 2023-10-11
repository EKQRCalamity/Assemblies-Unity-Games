using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerClickTrigger : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerClickHandler
{
	public UnityEvent OnLeftClick;

	public UnityEvent OnRightClick;

	public UnityEvent OnMiddleClick;

	public UnityEvent OnDoubleClick;

	private void OnEnable()
	{
	}

	public void OnPointerDown(PointerEventData eventData)
	{
	}

	public void OnPointerUp(PointerEventData eventData)
	{
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		switch (eventData.button)
		{
		case PointerEventData.InputButton.Left:
			OnLeftClick.Invoke();
			if (eventData.clickCount % 2 == 0)
			{
				OnDoubleClick.Invoke();
			}
			break;
		case PointerEventData.InputButton.Right:
			OnRightClick.Invoke();
			break;
		case PointerEventData.InputButton.Middle:
			OnMiddleClick.Invoke();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
