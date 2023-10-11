using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StateAnimator3D : MonoBehaviour
{
	[Header("Transforms================================================================")]
	[SerializeField]
	protected List<Transform> _stateTransforms;

	[Header("ANIMATION=================================================================")]
	public bool useScaledTime;

	[Header("Position Spring")]
	[Range(1f, 2000f)]
	public float positionSpringConstant = 100f;

	[Range(1f, 100f)]
	public float positionSpringDampening = 20f;

	[Header("Rotation Spring")]
	[Range(1f, 2000f)]
	public float rotationSpringConstant = 100f;

	[Range(1f, 100f)]
	public float rotationSpringDampening = 20f;

	[Header("Scale Spring")]
	[Range(1f, 2000f)]
	public float scaleSpringConstant = 100f;

	[Range(1f, 100f)]
	public float scaleSpringDampening = 20f;

	[Header("EVENTS=====================================================================")]
	public bool finishImmediatelyOnEnable = true;

	[SerializeField]
	protected int _stateIndex;

	[SerializeField]
	protected IntEvent _OnStateChange;

	[Range(0.01f, 1000f)]
	public float onStateAnimationCompleteThresholdMultiplier = 1f;

	[SerializeField]
	protected IntEvent _OnStateAnimationComplete;

	[Header("MENU BACK=====================================================================")]
	public bool doMenuBackLogic = true;

	public int menuBackIndex;

	[SerializeField]
	protected UnityEvent _OnMenuBackAtRoot;

	private TransformTarget _velocity;

	private Quaternion _targetOriginalRotation;

	private int? _animationFinished;

	private MenuBackHook _menuBack;

	public int Count => _stateTransforms.SafeCount(0);

	public List<Transform> stateTransforms
	{
		get
		{
			if (_stateTransforms.IsNullOrEmpty())
			{
				List<Transform> obj = new List<Transform> { base.transform };
				List<Transform> result = obj;
				_stateTransforms = obj;
				return result;
			}
			return _stateTransforms;
		}
	}

	public int stateIndex
	{
		get
		{
			return Mathf.Clamp(_stateIndex, 0, stateTransforms.Count);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _stateIndex, Mathf.Clamp(value, 0, stateTransforms.Count)))
			{
				_OnStatIndexChange();
			}
		}
	}

	public IntEvent OnStateChange => _OnStateChange ?? (_OnStateChange = new IntEvent());

	public IntEvent OnStateAnimationComplete => _OnStateAnimationComplete ?? (_OnStateAnimationComplete = new IntEvent());

	public UnityEvent OnMenuBackAtRoot => _OnMenuBackAtRoot ?? (_OnMenuBackAtRoot = new UnityEvent());

	protected Transform _targetTransform => stateTransforms[stateIndex];

	protected int? animationFinished
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

	protected MenuBackHook menuBack
	{
		get
		{
			if (!_menuBack)
			{
				_menuBack = this.GetOrAddComponent<MenuBackHook>();
				_menuBack.onBackRequest.AddListener(_OnMenuBackRequest);
			}
			return _menuBack;
		}
	}

	private void _OnStatIndexChange()
	{
		_velocity = _velocity.ResetRotationData();
		_targetOriginalRotation = base.transform.localRotation;
		OnStateChange.Invoke(stateIndex);
		animationFinished = null;
	}

	private void _OnAnimationFinished()
	{
		if (animationFinished.HasValue)
		{
			OnStateAnimationComplete.Invoke(animationFinished.Value);
		}
	}

	private void _FinishImmediately()
	{
		base.transform.CopyFromLocal(_targetTransform);
		_velocity = default(TransformTarget);
		OnStateChange.Invoke(stateIndex);
		animationFinished = stateIndex;
	}

	private void _OnMenuBackRequest()
	{
		if (_stateIndex == menuBackIndex)
		{
			OnMenuBackAtRoot.Invoke();
		}
		else
		{
			stateIndex = menuBackIndex;
		}
	}

	private void OnEnable()
	{
		if (finishImmediatelyOnEnable)
		{
			_FinishImmediately();
			return;
		}
		int num = _stateIndex;
		_stateIndex = -1;
		stateIndex = num;
	}

	private void Update()
	{
		if (doMenuBackLogic)
		{
			_ = menuBack;
		}
		float deltaTime = GameUtil.GetDeltaTime(useScaledTime);
		TransformTarget current = new TransformTarget(base.transform);
		TransformTarget transformTarget = new TransformTarget(_targetTransform);
		transformTarget.originalRotation = _targetOriginalRotation;
		TransformTarget transformTarget2 = transformTarget;
		TransformTarget.SpringToTarget(current, transformTarget2, ref _velocity, positionSpringConstant, positionSpringDampening, rotationSpringConstant, rotationSpringDampening, scaleSpringConstant, scaleSpringDampening, deltaTime).SetTransformValues(base.transform);
		if (!animationFinished.HasValue && current.IsRoughlyEqual(transformTarget2, onStateAnimationCompleteThresholdMultiplier, 0.01f) && _velocity.position.magnitude < 0.01f * onStateAnimationCompleteThresholdMultiplier)
		{
			animationFinished = stateIndex;
		}
	}

	public void CycleState()
	{
		stateIndex = (stateIndex + 1) % Count;
	}
}
