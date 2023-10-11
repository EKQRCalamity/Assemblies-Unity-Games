using System;
using UnityEngine;

public abstract class ACurves : MonoBehaviour
{
	[Serializable]
	public class FloatCurve
	{
		public bool enabled = true;

		public float min;

		public float max = 1f;

		public AnimationCurve interpolation = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public bool isMultiplier = true;

		public float initialValue { get; set; }

		public float GetValue(float t)
		{
			return MathUtil.Lerp(min, max, interpolation.EvaluateWithExtrapolation(t)) * (isMultiplier ? initialValue : 1f);
		}
	}

	[Serializable]
	public class Vector2Curve
	{
		public bool enabled = true;

		public Vector2 min = Vector2.zero;

		public Vector2 max = Vector2.one;

		public AnimationCurve interpolation = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public bool isMultiplier = true;

		public Vector2 initialValue { get; set; }

		public Vector2 GetValue(float t)
		{
			return min.Lerp(max, interpolation.EvaluateWithExtrapolation(t)).Multiply(isMultiplier ? initialValue : Vector2.one);
		}
	}

	[Serializable]
	public class Vector3Curve
	{
		public bool enabled = true;

		public Vector3 min = Vector3.zero;

		public Vector3 max = Vector3.one;

		public AnimationCurve interpolation = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public bool isMultiplier = true;

		public Vector3 initialValue { get; set; }

		public Vector3 GetValue(float t)
		{
			return min.Lerp(max, interpolation.EvaluateWithExtrapolation(t)).Multiply(isMultiplier ? initialValue : Vector3.one);
		}
	}

	[Serializable]
	public class ColorCurve
	{
		public bool enabled = true;

		public Gradient gradient;

		public bool isMultiplier = true;

		public bool alphaOnly;

		public Color initialValue { get; set; }

		public Color GetValue(Color currentColor, float t)
		{
			if (alphaOnly)
			{
				return currentColor.SetAlpha(gradient.Evaluate(t).a * (isMultiplier ? initialValue.a : 1f));
			}
			return gradient.Evaluate(t).Multiply(isMultiplier ? initialValue : Color.white);
		}
	}

	public FloatAnimator inputAnimation;

	[Header("GameObject Active Automation=============================================================================")]
	public bool automateGameObjectActiveState = true;

	public WaitForChildrenType waitForChildrenActiveStates = WaitForChildrenType.ImmediateChildren;

	public WaitForSpecificChildrenType waitForSpecificChildren = WaitForSpecificChildrenType.AudioVisual;

	[Header("Events=============================================================================")]
	public FloatEvent OnInputChange;

	private float _input = -1f;

	private WaitForChildren _waitForChildren;

	private float input
	{
		get
		{
			return _input;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _input, value))
			{
				_OnInputChanged();
			}
		}
	}

	private WaitForChildren waitForChildren
	{
		get
		{
			if (!_waitForChildren)
			{
				return _waitForChildren = base.gameObject.GetOrAddComponent<WaitForChildren>().SetData(waitForChildrenActiveStates, waitForSpecificChildren);
			}
			return _waitForChildren;
		}
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void Update()
	{
		if (!inputAnimation.isFinished)
		{
			input = inputAnimation.Update();
		}
		else
		{
			_CheckIfShouldDeactivate();
		}
	}

	protected virtual void OnDisable()
	{
		inputAnimation.Reset(0);
		input = 0f;
		if (!base.gameObject.activeSelf)
		{
			_input = -1f;
		}
	}

	private void _OnInputChanged()
	{
		_input = Mathf.Max(0f, _input);
		_CheckIfShouldActivate();
		_Input(input);
		OnInputChange.Invoke(input);
		_CheckIfShouldDeactivate();
	}

	private void _CheckIfShouldActivate()
	{
		if (automateGameObjectActiveState && input > 0f && !base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	private void _CheckIfShouldDeactivate()
	{
		if (automateGameObjectActiveState && input <= 0f && inputAnimation.IsFinished(0f) && _IsSafeToDeactivate())
		{
			base.gameObject.SetActive(value: false);
		}
	}

	protected abstract void _Input(float t);

	protected virtual bool _IsSafeToDeactivate()
	{
		if (waitForChildrenActiveStates != 0)
		{
			return waitForChildren.childrenFinished;
		}
		return true;
	}

	public void Input(float t)
	{
		input = t;
	}

	public void Input(bool isOn)
	{
		inputAnimation.targetPosition = (isOn ? 1 : 0);
		if (isOn && !base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
			_OnInputChanged();
		}
	}
}
