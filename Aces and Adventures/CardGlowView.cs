using UnityEngine;

public class CardGlowView : MonoBehaviour
{
	[Range(1f, 100f)]
	public float easeSpeed = 5f;

	public ColorEvent onGlowColorChange;

	public BoolEvent onGlowActiveChange;

	private Color? _desiredColor;

	private Color? _color;

	private bool? _active;

	public Color color
	{
		get
		{
			return _color ?? Colors.TRANSPARENT;
		}
		private set
		{
			Color value2 = value;
			Color? desiredColor = _desiredColor;
			if (value2 == desiredColor)
			{
				value = _desiredColor.Value;
			}
			if (!(_color == value))
			{
				_color = value;
				onGlowColorChange?.Invoke(value);
				active = value.a > 0f;
				value2 = value;
				desiredColor = _desiredColor;
				if (value2 == desiredColor)
				{
					_desiredColor = null;
				}
			}
		}
	}

	public bool active
	{
		get
		{
			return _active.GetValueOrDefault();
		}
		private set
		{
			if (SetPropertyUtility.SetStruct(ref _active, value))
			{
				onGlowActiveChange?.Invoke(value);
				if (!value)
				{
					_color = (_desiredColor = null);
				}
			}
		}
	}

	private void Update()
	{
		if (_desiredColor.HasValue)
		{
			color = color.Lerp(_desiredColor.Value, MathUtil.CalculateEaseStiffnessSubjectToTime(easeSpeed, Time.deltaTime)).DeltaSnap(_desiredColor.Value);
		}
	}

	public void SetDesiredGlowColor(Color desiredColor)
	{
		_desiredColor = desiredColor;
	}

	public void SetGlowActive(bool setActive)
	{
		_desiredColor = (setActive ? _desiredColor : new Color?(Colors.TRANSPARENT));
	}
}
