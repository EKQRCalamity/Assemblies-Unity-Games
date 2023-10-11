using UnityEngine;
using UnityEngine.EventSystems;

public class PointerDrop3D : MonoBehaviour, IDropHandler, IEventSystemHandler
{
	[SerializeField]
	protected PointerEvent _OnDrop;

	public PointerEvent OnDropped => _OnDrop ?? (_OnDrop = new PointerEvent());

	public void OnDrop(PointerEventData eventData)
	{
		OnDropped.Invoke(eventData);
	}
}
