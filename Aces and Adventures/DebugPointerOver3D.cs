using UnityEngine;
using UnityEngine.EventSystems;

public class DebugPointerOver3D : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public void OnPointerEnter(PointerEventData eventData)
	{
		Debug.Log($"OnPointerEnter: [{base.gameObject.name}]\n\n{eventData}");
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Debug.Log($"OnPointerExit: [{base.gameObject.name}]\n\n{eventData}");
	}
}
