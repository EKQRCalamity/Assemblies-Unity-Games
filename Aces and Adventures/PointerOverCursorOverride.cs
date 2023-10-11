using UnityEngine;
using UnityEngine.EventSystems;

public class PointerOverCursorOverride : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public SpecialCursorImage cursorOverride;

	public void OnPointerEnter(PointerEventData eventData)
	{
		InputManager.I.RequestCursorOverride(this, cursorOverride);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		InputManager.I.ReleaseCursorOverride(this);
	}
}
