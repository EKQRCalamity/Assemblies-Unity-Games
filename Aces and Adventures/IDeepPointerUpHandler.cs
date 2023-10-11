using UnityEngine.EventSystems;

public interface IDeepPointerUpHandler : IDeepPointerEventHandler, IEventSystemHandler
{
	void OnDeepPointerUp(PointerEventData eventData);
}
