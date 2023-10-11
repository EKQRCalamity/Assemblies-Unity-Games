using UnityEngine;
using UnityEngine.EventSystems;

public class PointerOverTrigger : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
	[Header("Event Bubbling")]
	public bool bubbleDragEvents = true;

	public bool pointerExitOnBeginDrag;

	[Header("Events")]
	public PointerEvent OnPointerEnterEvent;

	public PointerEvent OnPointerExitEvent;

	private bool _pointerEntered;

	private bool _dragging;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!_pointerEntered && (!bubbleDragEvents || !eventData.pointerDrag))
		{
			_pointerEntered = true;
			OnPointerEnterEvent.Invoke(eventData);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (_pointerEntered)
		{
			_pointerEntered = false;
			OnPointerExitEvent.Invoke(eventData);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (bubbleDragEvents)
		{
			ExecuteEvents.ExecuteHierarchy(base.transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (bubbleDragEvents)
		{
			_dragging = true;
			ExecuteEvents.ExecuteHierarchy(base.transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
			if (pointerExitOnBeginDrag)
			{
				OnPointerExit(eventData);
			}
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (bubbleDragEvents)
		{
			_dragging = false;
			if ((bool)base.transform && (bool)base.transform.parent)
			{
				ExecuteEvents.ExecuteHierarchy(base.transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
			}
			eventData.ExecutePointerEnterOnEndDrag();
		}
	}

	private void OnDisable()
	{
		if ((bool)this)
		{
			if (_dragging)
			{
				OnEndDrag(EventSystem.current.GetPointerData());
			}
			if (_pointerEntered)
			{
				OnPointerExit(EventSystem.current.GetPointerData());
			}
		}
	}

	public void SignalPointerEnter()
	{
		OnPointerEnterEvent.Signal(base.gameObject);
	}

	public void SignalPointerExit()
	{
		OnPointerExitEvent.Signal(base.gameObject);
	}

	public void SignalPointerIsOver(bool isOver)
	{
		if (isOver)
		{
			SignalPointerEnter();
		}
		else
		{
			SignalPointerExit();
		}
	}
}
