using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
[ScriptOrder(1)]
public class SyncCameraViewportToRectTransform : MonoBehaviour
{
	[SerializeField]
	protected RectTransform _syncViewportTo;

	private Camera _camera;

	private Camera _syncCamera;

	private CanvasScaler _canvasScaler;

	private Vector2 _canvasReferenceResolution;

	public RectTransform syncViewportTo
	{
		get
		{
			return _syncViewportTo;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _syncViewportTo, value))
			{
				_syncCamera = null;
			}
		}
	}

	public Camera camera => this.CacheComponent(ref _camera);

	private Camera syncCamera
	{
		get
		{
			if (!_syncCamera)
			{
				if (!syncViewportTo)
				{
					return null;
				}
				return syncViewportTo.GetComponentInParent<Camera>();
			}
			return _syncCamera;
		}
	}

	private void Start()
	{
		_canvasScaler = GetComponentInChildren<CanvasScaler>();
		if ((bool)_canvasScaler)
		{
			_canvasReferenceResolution = _canvasScaler.referenceResolution;
			_canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
		}
	}

	private void Update()
	{
		if ((bool)syncViewportTo && (bool)camera)
		{
			camera.rect = new Rect3D(syncViewportTo).WorldToViewportRect(syncCamera).Project2D();
			if ((bool)_canvasScaler)
			{
				Vector2 vector = camera.rect.size.Clamp01();
				_canvasScaler.referenceResolution = _canvasReferenceResolution.Multiply(Vector2.one.Lerp(vector, vector.OneMinus().Max()).Inverse());
				_canvasScaler.matchWidthOrHeight = ((vector.x < vector.y) ? Mathf.Lerp(0f, 0.5f, vector.x / vector.y.InsureNonZero()) : ((vector.y < vector.x) ? Mathf.Lerp(1f, 0.5f, vector.y / vector.x.InsureNonZero()) : 0.5f));
			}
		}
	}

	private void LateUpdate()
	{
		Update();
	}

	public SyncCameraViewportToRectTransform SetData(RectTransform rectToSyncTo)
	{
		syncViewportTo = rectToSyncTo;
		return this;
	}
}
