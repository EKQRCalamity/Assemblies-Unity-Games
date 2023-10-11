using UnityEngine;

public class ColorLerper : MonoBehaviour
{
	[SerializeField]
	protected Color32 _minColor;

	[SerializeField]
	protected Color32 _maxColor = Color.white;

	[SerializeField]
	protected float _min;

	[SerializeField]
	protected float _max = 1f;

	[SerializeField]
	protected float _value;

	private bool _upToDate;

	public ColorEvent OnColorChange;

	public Color32Event OnColor32Change;

	public Color32 minColor
	{
		get
		{
			return _minColor;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _minColor, value))
			{
				_SetDirty();
			}
		}
	}

	public Color32 maxColor
	{
		get
		{
			return _maxColor;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _maxColor, value))
			{
				_SetDirty();
			}
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

	public float value
	{
		get
		{
			return _value;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _value, value))
			{
				_SetDirty();
			}
		}
	}

	private void _SetDirty()
	{
		_upToDate = false;
	}

	private void Update()
	{
		if (!_upToDate)
		{
			Color32 color = minColor.LerpInHSVOutputRGB(maxColor, Mathf.Clamp01(MathUtil.GetLerpAmount(min, max, value)));
			OnColorChange.Invoke(color);
			OnColor32Change.Invoke(color);
			_upToDate = true;
		}
	}
}
