using UnityEngine;
using UnityEngine.EventSystems;

public class DragTrigger : MonoBehaviour, IDragHandler, IEventSystemHandler, IBeginDragHandler, IEndDragHandler
{
	[Header("Events")]
	public PointerEvent OnBeginDragEvent;

	public PointerEvent OnDragEvent;

	public PointerEvent OnEndDragEvent;

	public void OnBeginDrag(PointerEventData eventData)
	{
		OnBeginDragEvent.Invoke(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		OnDragEvent.Invoke(eventData);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		OnEndDragEvent.Invoke(eventData);
	}
}
