using UnityEngine.EventSystems;

public interface IDeepPointerClickHandler : IDeepPointerEventHandler, IEventSystemHandler
{
	void OnDeepPointerClick(PointerEventData eventData);
}
