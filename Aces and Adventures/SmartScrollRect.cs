using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SmartScrollRect : ScrollRect
{
	public PointerEventData.InputButton dragButton = PointerEventData.InputButton.Middle;

	private Action<PointerEventData> _onInitializePotentialDrag;

	private Action<PointerEventData> _onBeginDrag;

	private Action<PointerEventData> _onDrag;

	private Action<PointerEventData> _onEndDrag;

	private Action<PointerEventData> onInitializePotentialDrag => base.OnInitializePotentialDrag;

	private Action<PointerEventData> onBeginDrag => base.OnBeginDrag;

	private Action<PointerEventData> onDrag => base.OnDrag;

	private Action<PointerEventData> onEndDrag => base.OnEndDrag;

	private void _RemapDrag(PointerEventData eventData, Action<PointerEventData> dragFunction)
	{
		if (eventData.button == dragButton)
		{
			PointerEventData.InputButton button = eventData.button;
			eventData.button = PointerEventData.InputButton.Left;
			dragFunction(eventData);
			eventData.button = button;
		}
	}

	public override void OnInitializePotentialDrag(PointerEventData eventData)
	{
		_RemapDrag(eventData, onInitializePotentialDrag);
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		_RemapDrag(eventData, onBeginDrag);
	}

	public override void OnDrag(PointerEventData eventData)
	{
		_RemapDrag(eventData, onDrag);
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		_RemapDrag(eventData, onEndDrag);
	}
}
