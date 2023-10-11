using System;
using UnityEngine;

public class ToggleAnimator3D : MonoBehaviour
{
	private static System.Random _random;

	[Header("Transforms================================================================")]
	[SerializeField]
	protected Transform _offSiblingTransform;

	[SerializeField]
	protected Transform _onSiblingTransform;

	[Header("ANIMATION=================================================================")]
	public bool useScaledTime;

	[Header("Position Spring")]
	[Range(1f, 2000f)]
	public float positionSpringConstant = 200f;

	[Range(1f, 100f)]
	public float positionSpringDampening = 20f;

	[Header("Rotation Spring")]
	[Range(1f, 2000f)]
	public float rotationSpringConstant = 200f;

	[Range(1f, 100f)]
	public float rotationSpringDampening = 20f;

	public bool rotateInOneDirection;

	[Header("Scale Spring")]
	[Range(1f, 2000f)]
	public float scaleSpringConstant = 200f;

	[Range(1f, 100f)]
	public float scaleSpringDampening = 20f;

	[Header("State Based Spring Multipliers")]
	[Range(0.1f, 10f)]
	public float onSpringConstantMultiplier = 1f;

	[Range(0.1f, 10f)]
	public float onSpringDampeningMultiplier = 1f;

	[Range(0.1f, 10f)]
	public float offSpringConstantMultiplier = 1f;

	[Range(0.1f, 10f)]
	public float offSpringDampeningMultiplier = 1f;

	[Header("AUDIO======================================================================")]
	[SerializeField]
	protected ToggleAnimator3DSoundPack _soundPack;

	[Header("EVENTS=====================================================================")]
	public bool finishImmediatelyOnEnable = true;

	[SerializeField]
	protected bool _isOn;

	[SerializeField]
	protected BoolEvent _OnToggle;

	[Range(0.01f, 100f)]
	public float onToggleAnimationCompleteThresholdMultiplier = 1f;

	[Range(0f, 10f)]
	public float stopAnimatingAfterFinishTime;

	public bool syncAnimationCompleteToAudio;

	[SerializeField]
	protected BoolEvent _OnToggleAnimationComplete;

	private TransformTarget _velocity;

	private Quaternion _targetOriginalRotation;

	private PooledAudioSource _previousSource;

	private long _previousSourcePlayId;

	private bool? _animationFinished;

	private bool _skipNextSoundCue;

	private float _targetRotationLerp;

	private float _elapsedTimeAfterAnimationFinish;

	protected static System.Random _Random => _random ?? (_random = new System.Random());

	public bool isOn
	{
		get
		{
			return _isOn;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _isOn, value))
			{
				_OnIsOnChange();
			}
		}
	}

	public BoolEvent OnToggle => _OnToggle ?? (_OnToggle = new BoolEvent());

	public BoolEvent OnToggleAnimationComplete => _OnToggleAnimationComplete ?? (_OnToggleAnimationComplete = new BoolEvent());

	protected ToggleAnimator3DSoundPack soundPack => _soundPack ?? (_soundPack = ScriptableObject.CreateInstance<ToggleAnimator3DSoundPack>());

	protected Transform _targetTransform
	{
		get
		{
			if (!_isOn)
			{
				return _offSiblingTransform;
			}
			return _onSiblingTransform;
		}
	}

	public bool? animationFinished
	{
		get
		{
			return _animationFinished;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _animationFinished, value))
			{
				_OnAnimationFinished();
			}
		}
	}

	public bool isFinished => animationFinished.HasValue;

	private void _OnIsOnChange()
	{
		_velocity = _velocity.ResetRotationData();
		_targetOriginalRotation = base.transform.localRotation;
		OnToggle.Invoke(isOn);
		if ((bool)_previousSource && _previousSource.isPlaying && _previousSource.playId == _previousSourcePlayId)
		{
			_previousSource.Stop();
		}
		_previousSource = (_skipNextSoundCue ? null : soundPack.GetAudioPack(isOn).Play(base.transform, _Random, soundPack.mixerGroup));
		if ((bool)_previousSource)
		{
			_previousSourcePlayId = _previousSource.playId;
		}
		animationFinished = null;
		_skipNextSoundCue = false;
		if (!rotateInOneDirection)
		{
			_targetRotationLerp = 1f;
			return;
		}
		float num = Quaternion.Dot(_onSiblingTransform.localRotation, _offSiblingTransform.localRotation.Opposite());
		float num2 = Quaternion.Dot(_targetTransform.localRotation, _targetOriginalRotation);
		_targetRotationLerp = ((num > num2) ? (0f - (1f - num2)) : 1f);
	}

	private void _OnAnimationFinished()
	{
		_elapsedTimeAfterAnimationFinish = 0f;
		if (animationFinished.HasValue)
		{
			OnToggleAnimationComplete.Invoke(animationFinished.Value);
		}
	}

	private void OnEnable()
	{
		if (finishImmediatelyOnEnable)
		{
			FinishImmediately();
		}
		else
		{
			isOn = !(_isOn = !_isOn);
		}
	}

	private void Update()
	{
		if (_elapsedTimeAfterAnimationFinish > stopAnimatingAfterFinishTime)
		{
			return;
		}
		float deltaTime = GameUtil.GetDeltaTime(useScaledTime);
		TransformTarget current = new TransformTarget(base.transform);
		TransformTarget transformTarget = new TransformTarget(_targetTransform);
		transformTarget.originalRotation = _targetOriginalRotation;
		TransformTarget transformTarget2 = transformTarget;
		float num = (isOn ? onSpringConstantMultiplier : offSpringConstantMultiplier);
		float num2 = (isOn ? onSpringDampeningMultiplier : offSpringDampeningMultiplier);
		TransformTarget.SpringToTarget(current, transformTarget2, ref _velocity, positionSpringConstant * num, positionSpringDampening * num2, rotationSpringConstant * num, rotationSpringDampening * num2, scaleSpringConstant * num, scaleSpringDampening * num2, deltaTime, _targetRotationLerp).SetTransformValues(base.transform);
		if (animationFinished.HasValue)
		{
			if (stopAnimatingAfterFinishTime > 0f)
			{
				_elapsedTimeAfterAnimationFinish += GameUtil.GetDeltaTime(useScaledTime);
			}
		}
		else if (syncAnimationCompleteToAudio && (bool)_previousSource)
		{
			if (!_previousSource.isPlaying)
			{
				animationFinished = isOn;
			}
		}
		else if (current.IsRoughlyEqual(transformTarget2, onToggleAnimationCompleteThresholdMultiplier, 0.01f) && _velocity.position.magnitude < 0.01f * onToggleAnimationCompleteThresholdMultiplier)
		{
			animationFinished = isOn;
		}
	}

	private void OnDisable()
	{
		_previousSource = null;
	}

	public void FinishImmediately()
	{
		base.transform.CopyFromLocal(_targetTransform);
		_velocity = default(TransformTarget);
		OnToggle.Invoke(isOn);
		animationFinished = isOn;
	}

	public void Toggle()
	{
		isOn = !isOn;
	}

	public void SetIsOn(bool on)
	{
		isOn = on;
	}

	public void SetIsOnSkipSoundCue(bool on)
	{
		_skipNextSoundCue = on != isOn;
		SetIsOn(on);
	}

	public void SetAsSiblingToggleTransform(Transform transformToSetAsSibling, bool isOnTransform)
	{
		transformToSetAsSibling.SetParent(base.transform.parent, worldPositionStays: true);
		if (isOnTransform)
		{
			_onSiblingTransform = transformToSetAsSibling;
		}
		else
		{
			_offSiblingTransform = transformToSetAsSibling;
		}
	}

	public void SetCurrentAsToggleTransform(bool isOnTransform)
	{
		GameObject gameObject = new GameObject(string.Format("{0} Transform (Generated)", isOnTransform ? "On" : "Off"));
		gameObject.transform.CopyFrom(base.transform);
		SetAsSiblingToggleTransform(gameObject.transform, isOnTransform);
	}
}
