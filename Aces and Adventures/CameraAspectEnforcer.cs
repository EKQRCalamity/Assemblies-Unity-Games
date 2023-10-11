using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAspectEnforcer : MonoBehaviour
{
	[Range(1f, 100f)]
	[SerializeField]
	protected float _width = 16f;

	[Range(1f, 100f)]
	[SerializeField]
	protected float _height = 9f;

	[Range(1f, 100f)]
	[SerializeField]
	protected float _maxWidth = 64f;

	[Range(1f, 100f)]
	[SerializeField]
	protected float _maxHeight = 27f;

	private Camera _camera;

	private Int2 _previousScreenSize;

	public float width
	{
		get
		{
			return _width;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _width, value))
			{
				_SetDirty();
			}
		}
	}

	public float height
	{
		get
		{
			return _height;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _height, value))
			{
				_SetDirty();
			}
		}
	}

	public float maxWidth
	{
		get
		{
			return _maxWidth;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _maxWidth, value))
			{
				_SetDirty();
			}
		}
	}

	public float maxHeight
	{
		get
		{
			return _maxHeight;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _maxHeight, value))
			{
				_SetDirty();
			}
		}
	}

	public float minAspect
	{
		get
		{
			return width / height;
		}
		set
		{
			width = value;
			height = 1f;
		}
	}

	public float maxAspect
	{
		get
		{
			return maxWidth / maxHeight;
		}
		set
		{
			maxWidth = value;
			maxHeight = 1f;
		}
	}

	private Camera camera => this.CacheComponent(ref _camera);

	private void _SetDirty()
	{
		_previousScreenSize = Int2.MinValue;
	}

	private void Update()
	{
		Int2 @int = new Int2(Screen.width, Screen.height);
		if (!(_previousScreenSize == @int))
		{
			bool flag = (@int - new Int2(0, 1)).aspectRatio >= minAspect;
			bool flag2 = (@int + new Int2(0, 1)).aspectRatio <= maxAspect;
			camera.rect = ((flag && flag2) ? GraphicsUtil.ViewSpaceRect : GraphicsUtil.ViewSpaceRect.GetOptimalInscirbedAspectRatioRect(((!flag) ? minAspect : maxAspect) / @int.aspectRatio, new Vector2(0.5f, -0.5f)));
			_previousScreenSize = @int;
		}
	}
}
