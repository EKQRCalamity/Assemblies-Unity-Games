using UnityEngine.EventSystems;

public interface IDragThresholdSetter : IEventSystemHandler
{
	void OnSetDragThreshold(PointerEventData eventData);
}
