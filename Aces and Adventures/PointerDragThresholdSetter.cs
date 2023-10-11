using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerDragThresholdSetter : MonoBehaviour, IDragThresholdSetter, IEventSystemHandler
{
	public enum UnitType : byte
	{
		Pixels,
		Inches
	}

	public UnitType units;

	[Range(1f, 200f)]
	[HideInInspectorIf("_hidePixels", false)]
	public int pixels;

	[Range(0.001f, 2f)]
	[HideInInspectorIf("_hideInches", false)]
	public float inches;

	public Func<int> getPixelDragThreshold { get; set; }

	private bool _hidePixels => units != UnitType.Pixels;

	private bool _hideInches => units != UnitType.Inches;

	public void OnSetDragThreshold(PointerEventData eventData)
	{
		this.SetDragThreshold((getPixelDragThreshold != null) ? getPixelDragThreshold() : ((units == UnitType.Pixels) ? pixels : Mathf.RoundToInt(Screen.dpi * inches)));
	}
}
