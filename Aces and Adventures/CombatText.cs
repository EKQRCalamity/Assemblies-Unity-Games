using UnityEngine;

public class CombatText : MonoBehaviour
{
	[Range(0.5f, 10f)]
	public float displayDuration = 2f;

	[Range(0f, 10f)]
	public float combineDuration = 1f;

	[Header("Color")]
	[ColorUsage(true, true)]
	public Color damageColor = new Color(1f, 0f, 0f, 1f);

	[ColorUsage(true, true)]
	public Color healColor = new Color(0f, 1f, 0f, 1f);

	[Header("Events")]
	public StringEvent onTextChange;

	public ColorEvent onTintChange;

	public BoolEvent onActiveChange;

	private float _timeOfLastDelta;

	private int? _delta;

	private void LateUpdate()
	{
		if (_delta.HasValue && !(Time.time - _timeOfLastDelta < displayDuration))
		{
			_delta = null;
			onActiveChange.Invoke(arg0: false);
		}
	}

	public void AddDelta(int delta)
	{
		if (delta != 0)
		{
			if (Time.time - _timeOfLastDelta > combineDuration)
			{
				_delta = null;
			}
			_delta = _delta.GetValueOrDefault() + delta;
			onActiveChange.Invoke(arg0: true);
			onTextChange.Invoke((_delta > 0).ToText("+") + _delta.Value);
			onTintChange.Invoke((_delta > 0) ? healColor : damageColor);
			_timeOfLastDelta = Time.time;
		}
	}

	public void Hide()
	{
		_timeOfLastDelta = 0f - displayDuration;
	}
}
