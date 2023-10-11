using UnityEngine;
using UnityEngine.EventSystems;

public class DebugPointerClick3D : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerClickHandler
{
	public void OnPointerDown(PointerEventData eventData)
	{
		Debug.Log($"OnPointerDown({eventData.button}): [{base.gameObject.name}] \n\n{eventData}");
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		Debug.Log($"OnPointerUp({eventData.button}): [{base.gameObject.name}] \n\n{eventData}");
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log($"OnPointerClick({eventData.button}): [{base.gameObject.name}] \n\n{eventData}");
	}
}
