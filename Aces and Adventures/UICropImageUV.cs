using UnityEngine;

public class UICropImageUV : MonoBehaviour
{
	[SerializeField]
	[Range(1f, 5f)]
	protected float _maximumMagnification = 2f;

	[SerializeField]
	protected Texture2DEvent _onTextureChange;

	[SerializeField]
	protected Vector2Event _onPreferredSizeChange;

	[SerializeField]
	protected Vector2Event _onScaleRangeChange;

	[SerializeField]
	protected UVCoordsEvent _onUVChange;

	private Texture2D _texture;

	private Vector2 _preferredSize = new Vector2(float.MinValue, float.MinValue);

	private Vector2 _scaleRange = new Vector2(float.MinValue, float.MinValue);

	private UVCoords _uvCoords = UVCoords.Min;

	public float maximumMagnification
	{
		get
		{
			return _maximumMagnification;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _maximumMagnification, value))
			{
				_UpdateScaleRange();
			}
		}
	}

	public Texture2D texture
	{
		get
		{
			return _texture;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _texture, value))
			{
				onTextureChange.Invoke(value);
			}
		}
	}

	public Vector2 preferredSize
	{
		get
		{
			return _preferredSize;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _preferredSize, value))
			{
				onPreferredSizeChange.Invoke(value);
			}
		}
	}

	public Vector2 scaleRange
	{
		get
		{
			return _scaleRange;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _scaleRange, value))
			{
				onScaleRangeChange.Invoke(value);
			}
		}
	}

	public UVCoords uvCoords
	{
		get
		{
			return _uvCoords;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _uvCoords, value))
			{
				onUVChange.Invoke(_uvCoords);
			}
		}
	}

	public Texture2DEvent onTextureChange => _onTextureChange ?? (_onTextureChange = new Texture2DEvent());

	public Vector2Event onPreferredSizeChange => _onPreferredSizeChange ?? (_onPreferredSizeChange = new Vector2Event());

	public Vector2Event onScaleRangeChange => _onScaleRangeChange ?? (_onScaleRangeChange = new Vector2Event());

	public UVCoordsEvent onUVChange => _onUVChange ?? (_onUVChange = new UVCoordsEvent());

	private void _UpdateScaleRange()
	{
		if ((bool)texture && !(preferredSize.Min() <= 0f))
		{
			float num = preferredSize.Multiply(texture.PixelSize().ToVector2().Inverse()
				.InsureNonZero()).Max();
			scaleRange = new Vector2(num, Mathf.Max(num, maximumMagnification));
		}
	}

	private void Awake()
	{
		onPreferredSizeChange.AddListener(delegate
		{
			_UpdateScaleRange();
		});
		onTextureChange.AddListener(delegate
		{
			_UpdateScaleRange();
		});
	}
}
