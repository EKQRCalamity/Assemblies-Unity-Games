using UnityEngine;
using UnityEngine.Events;

public class FloatComparer : MonoBehaviour
{
	[SerializeField]
	protected float _value;

	[Header("Simple Events")]
	public UnityEvent OnLessThan;

	public UnityEvent OnEqualTo;

	public UnityEvent OnGreaterThan;

	[Header("Boolean Events")]
	public BoolEvent OnLessThanChange;

	public BoolEvent OnEqualToChange;

	public BoolEvent OnGreaterThanChange;

	[Header("Float Events")]
	public FloatEvent OnLessThanValueChange;

	public FloatEvent OnGreaterThanValueChange;

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
				float comparisonValue = _value;
				_value = value;
				CompareTo(comparisonValue);
			}
		}
	}

	public void CompareTo(float comparisonValue)
	{
		bool arg = false;
		bool arg2 = false;
		bool arg3 = false;
		if (comparisonValue > value)
		{
			OnGreaterThan.Invoke();
			OnGreaterThanValueChange.Invoke(comparisonValue);
			arg = true;
		}
		else if (comparisonValue < value)
		{
			OnLessThan.Invoke();
			OnLessThanValueChange.Invoke(comparisonValue);
			arg2 = true;
		}
		else
		{
			OnEqualTo.Invoke();
			arg3 = true;
		}
		OnLessThanChange.Invoke(arg2);
		OnEqualToChange.Invoke(arg3);
		OnGreaterThanChange.Invoke(arg);
	}

	public void CompareTo(int comparisonValue)
	{
		CompareTo((float)comparisonValue);
	}
}
