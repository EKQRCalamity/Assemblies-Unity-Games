using UnityEngine.EventSystems;

public interface IDeepPointerPropagation : IDeepPointerEventHandler, IEventSystemHandler
{
	bool ShouldDeepPointerContinuePropagation(PointerEventData eventData);
}
