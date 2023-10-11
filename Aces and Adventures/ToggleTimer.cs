using UnityEngine;

public class ToggleTimer : MonoBehaviour
{
	public bool useScaledTime;

	[SerializeField]
	protected bool _isOnByDefault;

	[SerializeField]
	protected BoolEvent _onValueChange;

	private bool? _isOn;

	[SerializeField]
	[HideInInspector]
	private float _toggleTimeRemaining;

	public BoolEvent onValueChange => _onValueChange ?? (_onValueChange = new BoolEvent());

	public bool isOn
	{
		get
		{
			return _isOn.GetValueOrDefault();
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _isOn, value))
			{
				onValueChange.Invoke(value);
			}
		}
	}

	private void Awake()
	{
		isOn = _isOnByDefault;
	}

	private void Update()
	{
		if (!(_toggleTimeRemaining <= 0f) && isOn)
		{
			_toggleTimeRemaining -= GameUtil.GetDeltaTime(useScaledTime);
			if (_toggleTimeRemaining <= 0f)
			{
				isOn = false;
			}
		}
	}

	public void SetIsOn(bool on)
	{
		isOn = on;
		_toggleTimeRemaining = 0f;
	}

	public void SetTrueForDuration(float duration)
	{
		isOn = true;
		_toggleTimeRemaining = duration;
	}
}
