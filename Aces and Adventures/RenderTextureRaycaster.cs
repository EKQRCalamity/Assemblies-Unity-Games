using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RenderTextureRaycaster : GraphicRaycaster
{
	[SerializeField]
	protected RawImage _renderTextureImage;

	public bool requiresCanvasToHaveFocus = true;

	private GraphicRaycaster _otherRaycaster;

	private CanvasInputFocus _otherCanvasInputFocus;

	public RawImage renderTextureImage
	{
		get
		{
			return _renderTextureImage;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _renderTextureImage, value))
			{
				_OnRenderTextureImageChange();
			}
		}
	}

	private GraphicRaycaster otherRaycaster
	{
		get
		{
			if (!_otherRaycaster)
			{
				return _otherRaycaster = (renderTextureImage ? renderTextureImage.GetComponentInParent<GraphicRaycaster>() : null);
			}
			return _otherRaycaster;
		}
	}

	private CanvasInputFocus otherCanvasInputFocus
	{
		get
		{
			if (!_otherCanvasInputFocus)
			{
				return _otherCanvasInputFocus = renderTextureImage.gameObject.GetOrAddComponent<CanvasInputFocus>();
			}
			return _otherCanvasInputFocus;
		}
	}

	public static RenderTextureRaycaster ReplaceExistingGraphicRaycaster(GameObject go, RawImage renderTextureImage)
	{
		GraphicRaycaster componentInParent = go.GetComponentInParent<GraphicRaycaster>();
		RenderTextureRaycaster renderTextureRaycaster = componentInParent.gameObject.AddComponent<RenderTextureRaycaster>();
		renderTextureRaycaster.renderTextureImage = renderTextureImage;
		renderTextureRaycaster.ignoreReversedGraphics = componentInParent.ignoreReversedGraphics;
		renderTextureRaycaster.blockingObjects = componentInParent.blockingObjects;
		CanvasScaler component = componentInParent.GetComponent<CanvasScaler>();
		if ((bool)component)
		{
			RenderTextureCanvasScaler renderTextureCanvasScaler = component.gameObject.AddComponent<RenderTextureCanvasScaler>();
			renderTextureCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			renderTextureCanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
			renderTextureCanvasScaler.referenceResolution = component.referenceResolution;
			renderTextureCanvasScaler.referencePixelsPerUnit = component.referencePixelsPerUnit;
			Object.Destroy(component);
		}
		Object.Destroy(componentInParent);
		return renderTextureRaycaster;
	}

	private void _OnRenderTextureImageChange()
	{
		if ((bool)renderTextureImage)
		{
			renderTextureImage.raycastTarget = false;
		}
		_otherRaycaster = null;
		_otherCanvasInputFocus = null;
	}

	public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
	{
		if ((bool)renderTextureImage && (!requiresCanvasToHaveFocus || otherCanvasInputFocus.hasFocus) && (!eventData.dragging || !(eventData.pressEventCamera != eventCamera)))
		{
			Rect3D rect3D = new Rect3D(renderTextureImage.rectTransform).WorldToScreenSpaceRect(otherRaycaster.eventCamera);
			if (eventData.dragging || rect3D.ContainsProjection(eventData.position))
			{
				eventData.position = eventCamera.ViewportToScreenPoint(rect3D.GetLerpAmount(eventData.position));
				base.Raycast(eventData, resultAppendList);
			}
		}
	}
}
