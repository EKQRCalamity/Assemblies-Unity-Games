using UnityEngine;
using UnityEngine.EventSystems;

public class DebugPointerDrag3D : MonoBehaviour, IInitializePotentialDragHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
		Debug.Log($"OnInitializePotentialDrag: [{base.gameObject.name}]\n\n{eventData}");
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		Debug.Log($"OnBeginDrag: [{base.gameObject.name}]\n\n{eventData}");
	}

	public void OnDrag(PointerEventData eventData)
	{
		Debug.Log($"OnDrag: [{base.gameObject.name}]\n\n{eventData}");
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		Debug.Log($"OnEndDrag: [{base.gameObject.name}]\n\n{eventData}");
	}
}
