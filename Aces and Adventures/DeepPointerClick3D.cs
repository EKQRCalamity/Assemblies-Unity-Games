using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeepPointerClick3D : MonoBehaviour, IDeepPointerClickHandler, IDeepPointerEventHandler, IEventSystemHandler, IDeepPointerPropagation
{
	[EnumFlags]
	public PointerInputButtonFlags stopPropagationButtonFlags;

	[SerializeField]
	protected PointerEvent _onLeftClick;

	[SerializeField]
	protected PointerEvent _onRightClick;

	[SerializeField]
	protected PointerEvent _onMiddleClick;

	public PointerEvent onLeftClick => _onLeftClick ?? (_onLeftClick = new PointerEvent());

	public PointerEvent onRightClick => _onRightClick ?? (_onRightClick = new PointerEvent());

	public PointerEvent onMiddleClick => _onMiddleClick ?? (_onMiddleClick = new PointerEvent());

	private void OnEnable()
	{
		EventSystem.current.SetDeepPointerEventsEnabled(enabled: true);
	}

	public void OnDeepPointerClick(PointerEventData eventData)
	{
		switch (eventData.button)
		{
		case PointerEventData.InputButton.Left:
			onLeftClick.Invoke(eventData);
			break;
		case PointerEventData.InputButton.Right:
			onRightClick.Invoke(eventData);
			break;
		case PointerEventData.InputButton.Middle:
			onMiddleClick.Invoke(eventData);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public bool ShouldDeepPointerContinuePropagation(PointerEventData eventData)
	{
		return !EnumUtil.HasFlagConvert(stopPropagationButtonFlags, eventData.button);
	}
}
