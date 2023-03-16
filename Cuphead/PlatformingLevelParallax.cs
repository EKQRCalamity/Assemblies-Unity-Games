using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlatformingLevelParallax : AbstractPausableComponent
{
	public enum Sides
	{
		Background,
		Foreground
	}

	[SerializeField]
	private PlatformingLevel.Theme _theme;

	[SerializeField]
	private Color _color = Color.white;

	[SerializeField]
	private Sides _side;

	[SerializeField]
	[Range(0f, 19f)]
	private int _layer;

	[SerializeField]
	[Range(-2000f, 2000f)]
	private int _sortingOrderOffset;

	[HideInInspector]
	public Vector3 basePos;

	[HideInInspector]
	public Vector3 lastPos;

	public bool overrideLayerYSpeed;

	public float overrideYSpeed;

	private Transform levelCameraTransform;

	private ParallaxLayer _parallaxLayer;

	private SpriteRenderer[] _s;

	private bool transformCached;

	private Transform _cachedTransform;

	private bool layerPropertiesCached;

	private ParallaxPropertiesData.ThemeProperties.Layer _layerProperties;

	public PlatformingLevel.Theme Theme => _theme;

	public Color Color => _color;

	public Sides Side => _side;

	public int Layer => _layer;

	public int SortingOrderOffset => _sortingOrderOffset;

	private SpriteRenderer[] _spriteRenderers
	{
		get
		{
			if (_s == null)
			{
				List<SpriteRenderer> list = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
				_s = list.ToArray();
			}
			return _s;
		}
	}

	protected new Transform transform
	{
		get
		{
			if (!transformCached)
			{
				_cachedTransform = base.transform;
				transformCached = true;
			}
			return _cachedTransform;
		}
	}

	private ParallaxPropertiesData.ThemeProperties.Layer LayerProperties
	{
		get
		{
			if (!layerPropertiesCached)
			{
				_layerProperties = ParallaxPropertiesData.Instance.GetProperty(_theme, _layer, _side);
				layerPropertiesCached = true;
			}
			return _layerProperties;
		}
	}

	private void Start()
	{
		FrameDelayedCallback(DelayedStart, 1);
		UpdatePosition();
	}

	private void DelayedStart()
	{
		SetSpriteProperties();
		UpdatePosition();
	}

	public void UpdateBasePosition()
	{
		if (levelCameraTransform == null)
		{
			CupheadLevelCamera cupheadLevelCamera = Object.FindObjectOfType<CupheadLevelCamera>();
			if (cupheadLevelCamera == null)
			{
				return;
			}
			levelCameraTransform = cupheadLevelCamera.transform;
			if (levelCameraTransform == null)
			{
				return;
			}
		}
		if (overrideLayerYSpeed)
		{
			basePos.x = transform.position.x - levelCameraTransform.position.x * LayerProperties.speed;
			basePos.y = transform.position.y - levelCameraTransform.position.y * overrideYSpeed;
		}
		else
		{
			basePos = transform.position - levelCameraTransform.position * LayerProperties.speed;
		}
	}

	public void SetSpriteProperties()
	{
		SpriteRenderer[] spriteRenderers = _spriteRenderers;
		foreach (SpriteRenderer spriteRenderer in spriteRenderers)
		{
			spriteRenderer.sortingLayerName = ((_side != 0) ? SpriteLayer.Foreground.ToString() : SpriteLayer.Background.ToString());
			spriteRenderer.sortingOrder = LayerProperties.sortingOrder + _sortingOrderOffset;
			PlatformingLevelParallaxChild component = spriteRenderer.gameObject.GetComponent<PlatformingLevelParallaxChild>();
			if (component != null)
			{
				spriteRenderer.sortingOrder += component.SortingOrderOffset;
			}
			spriteRenderer.color = _color;
		}
	}

	private void LateUpdate()
	{
		UpdatePosition();
	}

	private void UpdatePosition()
	{
		if (levelCameraTransform == null)
		{
			CupheadLevelCamera cupheadLevelCamera = Object.FindObjectOfType<CupheadLevelCamera>();
			if (cupheadLevelCamera == null)
			{
				return;
			}
			levelCameraTransform = cupheadLevelCamera.transform;
			if (levelCameraTransform == null)
			{
				return;
			}
		}
		if (overrideLayerYSpeed)
		{
			transform.SetPosition(basePos.x + levelCameraTransform.position.x * LayerProperties.speed, basePos.y + levelCameraTransform.position.y * overrideYSpeed);
		}
		else
		{
			transform.position = basePos + levelCameraTransform.position * LayerProperties.speed;
		}
	}

	private void OnValidate()
	{
		SpriteRenderer[] spriteRenderers = _spriteRenderers;
		foreach (SpriteRenderer spriteRenderer in spriteRenderers)
		{
			if (!(spriteRenderer == null))
			{
				spriteRenderer.hideFlags = HideFlags.None;
			}
		}
	}
}
