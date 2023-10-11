using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class BubblePointerClick3D : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerClickHandler
{
	public PointerEventBubbleType left;

	public PointerEventBubbleType right;

	public PointerEventBubbleType middle;

	[HideInInspectorIf("_hideOtherGameObject", false)]
	public GameObject otherGameObject;

	private GameObject _pressed;

	private PointerEventBubbleType this[PointerEventData.InputButton button] => button switch
	{
		PointerEventData.InputButton.Left => left, 
		PointerEventData.InputButton.Right => right, 
		PointerEventData.InputButton.Middle => middle, 
		_ => throw new ArgumentOutOfRangeException("button", button, null), 
	};

	private bool _hideOtherGameObject
	{
		get
		{
			if (left != PointerEventBubbleType.OtherGameObject && right != PointerEventBubbleType.OtherGameObject)
			{
				return middle != PointerEventBubbleType.OtherGameObject;
			}
			return false;
		}
	}

	private void OnDisable()
	{
		_pressed = null;
	}

	public BubblePointerClick3D SetData(PointerEventBubbleType left = PointerEventBubbleType.CachedRaycast, PointerEventBubbleType right = PointerEventBubbleType.CachedRaycast, PointerEventBubbleType middle = PointerEventBubbleType.CachedRaycast)
	{
		this.left = left;
		this.right = right;
		this.middle = middle;
		return this;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_pressed = this[eventData.button].Bubble(this, eventData, ExecuteEvents.pointerDownHandler, otherGameObject);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (this[eventData.button].Bubble(this, eventData, ExecuteEvents.pointerUpHandler, otherGameObject) == _pressed && (bool)_pressed && !eventData.dragging)
		{
			ExecuteEvents.Execute(_pressed, eventData, ExecuteEvents.pointerClickHandler);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
	}
}
