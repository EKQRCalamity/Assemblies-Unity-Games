using UnityEngine;
using UnityEngine.EventSystems;

public class DebugDeepPointerClick3D : MonoBehaviour, IDeepPointerDownHandler, IDeepPointerEventHandler, IEventSystemHandler, IDeepPointerUpHandler, IDeepPointerClickHandler
{
	public void OnDeepPointerDown(PointerEventData eventData)
	{
		Debug.Log($"OnDeepPointerDown({eventData.button}): {base.gameObject.name}\n\n{eventData}");
	}

	public void OnDeepPointerUp(PointerEventData eventData)
	{
		Debug.Log($"OnDeepPointerUp({eventData.button}): {base.gameObject.name}\n\n{eventData}");
	}

	public void OnDeepPointerClick(PointerEventData eventData)
	{
		Debug.Log($"OnDeepPointerClick({eventData.button}): {base.gameObject.name}\n\n{eventData}");
	}
}
