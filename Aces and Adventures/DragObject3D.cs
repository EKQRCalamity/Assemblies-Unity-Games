using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class DragObject3D : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	private Vector3? _mouseDownPosition;

	private Vector3 _offset;

	private float _mouseDownDistance;

	public PointerEvent OnBeginDragEvent;

	public PointerEvent OnDragEvent;

	public PointerEvent OnEndDragEvent;

	public void OnBeginDrag(PointerEventData eventData)
	{
		_mouseDownPosition = eventData.pointerPressRaycast.worldPosition;
		_mouseDownDistance = eventData.pointerPressRaycast.distance;
		_offset = base.transform.position - _mouseDownPosition.Value;
		UnityEventExtensions.SafeInvoke(ref OnBeginDragEvent, eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		base.transform.position = CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition).GetPoint(_mouseDownDistance) + _offset;
		UnityEventExtensions.SafeInvoke(ref OnDragEvent, eventData);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		_mouseDownPosition = null;
		UnityEventExtensions.SafeInvoke(ref OnEndDragEvent, eventData);
	}
}
