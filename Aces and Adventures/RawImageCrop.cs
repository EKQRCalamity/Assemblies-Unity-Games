using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage), typeof(RawImageLayoutElement))]
public class RawImageCrop : MonoBehaviour, IInitializePotentialDragHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
	[Range(1f, 3f)]
	public float maxMagnification = 1.5f;

	[SerializeField]
	protected UVCoordsEvent _onUVChange;

	private RawImage _rawImage;

	private RawImageLayoutElement _layoutElement;

	private Int2 _preferredSize = Int2.MinValue;

	private UVCoords _uvCoords = UVCoords.Min;

	private UVCoords _beginDragUVCoords;

	private Vector2 _beginDragPointerUV;

	private float _beginDragScale;

	public RawImage rawImage => this.CacheComponent(ref _rawImage);

	public Int2 textureDimensions
	{
		get
		{
			if (!rawImage.texture)
			{
				return _preferredSize;
			}
			return (rawImage.texture as Texture2D).PixelSize();
		}
	}

	public RawImageLayoutElement layoutElement => this.CacheComponent(ref _layoutElement);

	public RectTransform rectTransform => rawImage.rectTransform;

	public UVCoords uvCoords
	{
		get
		{
			return _uvCoords;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _uvCoords, _ProcessUVCoords(value)))
			{
				onUVChange.Invoke(_uvCoords);
			}
		}
	}

	public UVCoordsEvent onUVChange => _onUVChange ?? (_onUVChange = new UVCoordsEvent());

	public Int2 preferredSize
	{
		get
		{
			return _preferredSize;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _preferredSize, value))
			{
				_OnPreferredSizeChage();
			}
		}
	}

	private void _OnPreferredSizeChage()
	{
		uvCoords = GetDefaultUVCoords();
		layoutElement.maxSize = preferredSize;
	}

	private UVCoords _ProcessUVCoords(UVCoords uv)
	{
		return uv.FitIntoRange(UVCoords.Default);
	}

	private Vector2 _GetPointerUV(PointerEventData eventData)
	{
		return rectTransform.GetWorldRect3D().GetLerpAmount(base.transform.GetPlane(PlaneAxes.XY).ClosestPointOnPlane(RectTransformUtility.ScreenPointToRay(eventData.pressEventCamera, Input.mousePosition)));
	}

	private void _SetScale(float scale)
	{
		uvCoords = uvCoords.SetSize(GetDefaultUVCoords().size * Mathf.Clamp(scale, _MaxMagnification(), 1f));
	}

	private float _MaxMagnification()
	{
		return 1f / (Mathf.Max(1f, textureDimensions.ToVector2().Multiply(preferredSize.ToVector2().Inverse()).Max()) * maxMagnification);
	}

	private void Awake()
	{
		onUVChange.AddListener(delegate(UVCoords uv)
		{
			rawImage.uvRect = uv;
		});
	}

	public UVCoords GetDefaultUVCoords()
	{
		return UVCoords.FromAspectRatio(preferredSize.ToVector2().normalized.Multiply(textureDimensions.ToVector2().normalized.Inverse().InsureNonZero()));
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
		eventData.useDragThreshold = false;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		_beginDragPointerUV = _GetPointerUV(eventData);
		_beginDragUVCoords = uvCoords;
		_beginDragScale = uvCoords.size.Max();
	}

	public void OnDrag(PointerEventData eventData)
	{
		Vector2 vector = _beginDragPointerUV - _GetPointerUV(eventData);
		switch (eventData.button)
		{
		case PointerEventData.InputButton.Left:
			if (InputManager.I[KeyModifiers.Shift])
			{
				vector = vector.KeepAbsMaxAxis();
			}
			uvCoords = _beginDragUVCoords + (vector * uvCoords.size.Max()).Multiply(uvCoords.aspect.InsureNonZero());
			break;
		case PointerEventData.InputButton.Right:
			_SetScale(_beginDragScale + (vector.x + vector.y) * (1f - _MaxMagnification()));
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case PointerEventData.InputButton.Middle:
			break;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Middle)
		{
			uvCoords = GetDefaultUVCoords();
		}
	}
}
