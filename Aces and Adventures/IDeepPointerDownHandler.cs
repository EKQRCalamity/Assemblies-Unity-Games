using UnityEngine.EventSystems;

public interface IDeepPointerDownHandler : IDeepPointerEventHandler, IEventSystemHandler
{
	void OnDeepPointerDown(PointerEventData eventData);
}
