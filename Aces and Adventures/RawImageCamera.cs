using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RawImageCamera : RawImage
{
	[SerializeField]
	protected Camera _cameraToRenderIntoImage;

	[SerializeField]
	protected List<Camera> _additionCamerasToRenderIntoImage;

	private RenderTexture _renderTexture;

	private Int2 _previousDimensions;

	private bool _renderTextureIsDirty;

	public Camera cameraToRenderIntoImage
	{
		get
		{
			return _cameraToRenderIntoImage;
		}
		set
		{
			if (!(_cameraToRenderIntoImage == value))
			{
				if ((bool)_cameraToRenderIntoImage)
				{
					_cameraToRenderIntoImage.targetTexture = null;
				}
				_cameraToRenderIntoImage = value;
				_UpdateRenderTexture(forceUpdate: true);
			}
		}
	}

	private PoolKeepItemListHandle<Camera> _cameras
	{
		get
		{
			PoolKeepItemListHandle<Camera> poolKeepItemListHandle = Pools.UseKeepItemList<Camera>();
			if ((bool)cameraToRenderIntoImage)
			{
				poolKeepItemListHandle.Add(cameraToRenderIntoImage);
			}
			if (!_additionCamerasToRenderIntoImage.IsNullOrEmpty())
			{
				foreach (Camera item in _additionCamerasToRenderIntoImage)
				{
					if ((bool)item)
					{
						poolKeepItemListHandle.Add(item);
					}
				}
				return poolKeepItemListHandle;
			}
			return poolKeepItemListHandle;
		}
	}

	private void _ReleaseRenderTexture()
	{
		if (!_renderTexture)
		{
			return;
		}
		foreach (Camera camera in _cameras)
		{
			camera.targetTexture = null;
		}
		_renderTexture.Release();
		Object.DestroyImmediate(_renderTexture);
		_renderTexture = null;
	}

	private void _UpdateRenderTexture(bool forceUpdate)
	{
		if (!cameraToRenderIntoImage || !base.canvas)
		{
			return;
		}
		Rect rect = RectTransformUtility.PixelAdjustRect(base.rectTransform, base.canvas);
		Int2 @int = new Int2(Mathf.RoundToInt(rect.width * base.canvas.scaleFactor), Mathf.RoundToInt(rect.height * base.canvas.scaleFactor)).Max(new Int2(4, 4));
		if (!forceUpdate && @int == _previousDimensions)
		{
			return;
		}
		_previousDimensions = @int;
		_ReleaseRenderTexture();
		_renderTexture = new RenderTexture(@int.x, @int.y, (cameraToRenderIntoImage.depthTextureMode != 0) ? 24 : 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default)
		{
			name = $"{cameraToRenderIntoImage.gameObject.name}: {base.gameObject.name}",
			antiAliasing = GraphicsUtil.antiAliasing
		};
		_renderTexture.hideFlags |= HideFlags.DontSave;
		foreach (Camera camera in _cameras)
		{
			camera.targetTexture = _renderTexture;
		}
		base.texture = _renderTexture;
	}

	public void RegisterToMainCamera()
	{
		cameraToRenderIntoImage = CameraManager.Instance.mainCamera;
	}

	public void AddAdditionalCamera(Camera additionalCamera)
	{
		if (_additionCamerasToRenderIntoImage.AddUnique(additionalCamera))
		{
			_UpdateRenderTexture(forceUpdate: true);
		}
	}

	public void RemoveAdditionalCamera(Camera additionalCamera)
	{
		if (_additionCamerasToRenderIntoImage.Remove(additionalCamera) && additionalCamera.targetTexture == _renderTexture)
		{
			additionalCamera.targetTexture = null;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_UpdateRenderTexture(forceUpdate: true);
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		_renderTextureIsDirty = true;
	}

	private void LateUpdate()
	{
		if (_renderTextureIsDirty && !(_renderTextureIsDirty = false))
		{
			_UpdateRenderTexture(forceUpdate: true);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_ReleaseRenderTexture();
	}
}
