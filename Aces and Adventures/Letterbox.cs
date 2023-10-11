using UnityEngine;

public class Letterbox : MonoBehaviour
{
	private static readonly ResourceBlueprint<GameObject> _Blueprint = "UI/Letterbox";

	private static Letterbox _Instance;

	[Range(0f, 5f)]
	[SerializeField]
	protected float _minAspect;

	[Range(0f, 5f)]
	[SerializeField]
	protected float _maxAspect = 2.3703704f;

	public RectTransform minRect;

	public RectTransform maxRect;

	private Int2 _previousScreenSize;

	private Canvas _canvas;

	public static Letterbox Instance
	{
		get
		{
			if (!_Instance)
			{
				return _Instance = Object.Instantiate(_Blueprint.value).GetComponent<Letterbox>();
			}
			return _Instance;
		}
	}

	public float minAspect
	{
		get
		{
			return _minAspect;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _minAspect, value))
			{
				_SetDirty();
			}
		}
	}

	public float maxAspect
	{
		get
		{
			return _maxAspect;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _maxAspect, value))
			{
				_SetDirty();
			}
		}
	}

	public Canvas canvas => this.CacheComponentInParent(ref _canvas);

	private void _SetDirty()
	{
		_previousScreenSize = Int2.MinValue;
	}

	private void _SetActive(bool active)
	{
		if ((bool)canvas)
		{
			canvas.enabled = active;
		}
		minRect.gameObject.SetActive(active);
		maxRect.gameObject.SetActive(active);
	}

	private void Awake()
	{
		Object.DontDestroyOnLoad(this);
	}

	private void Update()
	{
		Int2 @int = new Int2(Screen.width, Screen.height);
		if (!(_previousScreenSize == @int))
		{
			_previousScreenSize = @int;
			float aspectRatio = @int.aspectRatio;
			if (aspectRatio > maxAspect + 0.001f)
			{
				_SetActive(active: true);
				float num = (1f - maxAspect / aspectRatio) * 0.5f;
				minRect.anchorMin = Vector2.zero;
				minRect.anchorMax = new Vector2(num, 1f);
				maxRect.anchorMin = new Vector2(1f - num, 0f);
				maxRect.anchorMax = Vector2.one;
			}
			else if (aspectRatio < minAspect - 0.001f)
			{
				_SetActive(active: true);
				float num2 = (1f - aspectRatio / minAspect) * 0.5f;
				minRect.anchorMin = Vector2.zero;
				minRect.anchorMax = new Vector2(1f, num2);
				maxRect.anchorMin = new Vector2(0f, 1f - num2);
				maxRect.anchorMax = Vector2.one;
			}
			else
			{
				_SetActive(active: false);
			}
		}
	}
}
