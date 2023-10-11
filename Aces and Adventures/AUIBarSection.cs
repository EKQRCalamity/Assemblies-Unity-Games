using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public abstract class AUIBarSection : MonoBehaviour
{
	[SerializeField]
	protected float _rangeMin;

	[SerializeField]
	protected float _rangeMax;

	[SerializeField]
	protected float _min;

	[SerializeField]
	protected float _max;

	[SerializeField]
	private BoolEvent _OnVisibileChanged;

	[SerializeField]
	private BoolEvent _OnIsPositiveChanged;

	[Range(0f, 1f)]
	public float visibilityThresholdDistance = 0.01f;

	public bool useNormalizedThresholdDistance;

	private bool _dirty;

	private RectTransform _rect;

	public float rangeMin
	{
		get
		{
			return _rangeMin;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _rangeMin, value))
			{
				_SetDirty();
			}
		}
	}

	public float rangeMax
	{
		get
		{
			return _rangeMax;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _rangeMax, value))
			{
				_SetDirty();
			}
		}
	}

	public Vector2 rangeMinMax
	{
		get
		{
			return new Vector2(rangeMin, rangeMax);
		}
		set
		{
			rangeMin = value.x;
			rangeMax = value.y;
		}
	}

	public float min
	{
		get
		{
			return _min;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _min, value))
			{
				_SetDirty();
			}
		}
	}

	public float max
	{
		get
		{
			return _max;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _max, value))
			{
				_SetDirty();
			}
		}
	}

	public Vector2 minMax
	{
		get
		{
			return new Vector2(min, max);
		}
		set
		{
			min = value.x;
			max = value.y;
		}
	}

	public float normalizedMin
	{
		get
		{
			return Mathf.Clamp01(MathUtil.GetLerpAmount(rangeMin, rangeMax, min));
		}
		set
		{
			min = Mathf.Lerp(rangeMin, rangeMax, value);
		}
	}

	public float normalizedMax
	{
		get
		{
			return Mathf.Clamp01(MathUtil.GetLerpAmount(rangeMin, rangeMax, max));
		}
		set
		{
			max = Mathf.Lerp(rangeMin, rangeMax, value);
		}
	}

	public Vector2 normalizedMinMax
	{
		get
		{
			return new Vector2(normalizedMin, normalizedMax);
		}
		set
		{
			normalizedMin = value.x;
			normalizedMax = value.y;
		}
	}

	public RectTransform rect
	{
		get
		{
			if (!_rect)
			{
				return _rect = GetComponent<RectTransform>();
			}
			return _rect;
		}
	}

	public BoolEvent OnVisibleChanged => _OnVisibileChanged ?? (_OnVisibileChanged = new BoolEvent());

	public BoolEvent OnIsPositiveChanged => _OnIsPositiveChanged ?? (_OnIsPositiveChanged = new BoolEvent());

	public abstract Graphic graphic { get; }

	public bool visibile
	{
		get
		{
			return graphic.enabled;
		}
		set
		{
			if (value != graphic.enabled)
			{
				graphic.enabled = value;
				OnVisibleChanged.Invoke(value);
			}
		}
	}

	public bool isPositive => (useNormalizedThresholdDistance ? (normalizedMax - normalizedMin) : (max - min)) >= 0f;

	protected void _SetDirty()
	{
		_dirty = true;
	}

	private void _UpdateLayout()
	{
		_dirty = false;
		_UpdateLayoutUnique();
		visibile = (useNormalizedThresholdDistance ? normalizedMinMax : minMax).Range() >= visibilityThresholdDistance;
		OnIsPositiveChanged.Invoke(isPositive);
	}

	protected abstract void _UpdateLayoutUnique();

	protected virtual void OnEnable()
	{
		_SetDirty();
	}

	protected virtual void Update()
	{
		if (_dirty)
		{
			_UpdateLayout();
		}
	}

	public void SetMinAndMax(float value)
	{
		min = value;
		max = value;
	}
}
