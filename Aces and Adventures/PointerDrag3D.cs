using UnityEngine;
using UnityEngine.EventSystems;

public class PointerDrag3D : MonoBehaviour, IInitializePotentialDragHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[SerializeField]
	protected PointerEvent _OnInitialize;

	[SerializeField]
	protected PointerEvent _OnBegin;

	[SerializeField]
	protected PointerEvent _OnDrag;

	[SerializeField]
	protected PointerEvent _OnEnd;

	public PointerEvent OnInitialize => _OnInitialize ?? (_OnInitialize = new PointerEvent());

	public PointerEvent OnBegin => _OnBegin ?? (_OnBegin = new PointerEvent());

	public PointerEvent OnDragged => _OnDrag ?? (_OnDrag = new PointerEvent());

	public PointerEvent OnEnd => _OnEnd ?? (_OnEnd = new PointerEvent());

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
		OnInitialize.Invoke(eventData);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		OnBegin.Invoke(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		OnDragged.Invoke(eventData);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		OnEnd.Invoke(eventData);
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}
}
