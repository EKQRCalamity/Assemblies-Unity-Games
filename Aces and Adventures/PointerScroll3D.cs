using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerScroll3D : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	public bool ignoreScrollWhileDragging = true;

	[SerializeField]
	private PointerEvent _onScroll;

	[SerializeField]
	private Vector2Event _onScrollDelta;

	[SerializeField]
	private UnityEvent _onScrollUp;

	[SerializeField]
	private UnityEvent _onScrollDown;

	public PointerEvent onScroll => _onScroll ?? (_onScroll = new PointerEvent());

	public Vector2Event onScrollDelta => _onScrollDelta ?? (_onScrollDelta = new Vector2Event());

	public UnityEvent onScrollUp => _onScrollUp ?? (_onScrollUp = new UnityEvent());

	public UnityEvent onScrollDown => _onScrollDown ?? (_onScrollDown = new UnityEvent());

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (!ignoreScrollWhileDragging || !InputManager.IsDraggingUI)
		{
			onScroll.Invoke(eventData);
			onScrollDelta.Invoke(eventData.scrollDelta);
			if (eventData.scrollDelta.y > 0f)
			{
				onScrollUp.Invoke();
			}
			else if (eventData.scrollDelta.y < 0f)
			{
				onScrollDown.Invoke();
			}
		}
	}
}
