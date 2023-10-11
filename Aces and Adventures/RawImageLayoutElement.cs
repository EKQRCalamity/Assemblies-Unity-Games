using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage), typeof(RectTransform))]
[ExecuteInEditMode]
public class RawImageLayoutElement : UIBehaviour, ILayoutElement
{
	[Range(0f, 2048f)]
	public float maxWidth = 512f;

	[Range(0f, 2048f)]
	public float maxHeight = 512f;

	[Range(0f, 100f)]
	public float widthFlexibility;

	[Range(0f, 1024f)]
	public float widthMin = 64f;

	[Range(0f, 1024f)]
	public float heightMin = 64f;

	public bool useMaxAsPreferred;

	private RawImage _rawImage;

	private Texture _mainTexture;

	private float _currentWidth;

	private float _previousPreferredWidth;

	private float _previousPreferredHeight;

	public Vector2 maxSize
	{
		get
		{
			return new Vector2(maxWidth, maxHeight);
		}
		set
		{
			maxWidth = value.x;
			maxHeight = value.y;
		}
	}

	private RawImage rawImage => this.CacheComponent(ref _rawImage);

	public float minWidth => widthMin;

	public float preferredWidth
	{
		get
		{
			if (!rawImage.texture)
			{
				return _previousPreferredWidth;
			}
			if (useMaxAsPreferred && maxWidth > 0f)
			{
				return maxWidth;
			}
			Vector2 vector = rawImage.UVPixelDimensions();
			float num = 1f;
			if (maxWidth > 0f && vector.x > maxWidth)
			{
				num = maxWidth / vector.x;
			}
			if (maxHeight > 0f && vector.y > maxHeight)
			{
				num = Mathf.Min(num, maxHeight / vector.y);
			}
			return _previousPreferredWidth = vector.x * num;
		}
	}

	public float flexibleWidth => widthFlexibility;

	public float minHeight => heightMin;

	public float preferredHeight
	{
		get
		{
			if (!rawImage.texture)
			{
				return _previousPreferredHeight;
			}
			return _previousPreferredHeight = _currentWidth / rawImage.UVPixelAspectRatio();
		}
	}

	public float flexibleHeight => 0f;

	public int layoutPriority => 1;

	protected override void Awake()
	{
		base.Awake();
		_mainTexture = rawImage.mainTexture;
	}

	private void Update()
	{
		if ((bool)rawImage && SetPropertyUtility.SetObject(ref _mainTexture, rawImage.mainTexture))
		{
			SetLayoutDirty();
		}
	}

	public void SetMaxDimensions(int max)
	{
		maxWidth = (maxHeight = max);
	}

	public void SetMaxDimensions(Int2 max)
	{
		maxWidth = max.x;
		maxHeight = max.y;
	}

	public void SetLayoutDirty()
	{
		LayoutRebuilder.MarkLayoutForRebuild(base.transform as RectTransform);
	}

	public void CalculateLayoutInputHorizontal()
	{
	}

	public void CalculateLayoutInputVertical()
	{
		_currentWidth = (base.transform as RectTransform).rect.width;
	}
}
