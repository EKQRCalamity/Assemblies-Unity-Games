using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DiceRoller : MonoBehaviour
{
	public PhysicsDice die;

	public Transform startingPositionTransform;

	public RigidBodyLauncher launcher;

	public Transform resultDisplayTransform;

	[Range(0.5f, 5f)]
	public float resultDisplayTime = 1f;

	public bool useScaledTime = true;

	public UnityEvent onLaunch;

	public IntBoolEvent onEarlyResult;

	public BoolEvent onResult;

	public IntEvent onResultValue;

	private int _targetValue;

	private int _result;

	private Transform _resultDisplayChild;

	private float _resultDisplayTime;

	private int _frameOfLastLaunch = -1;

	private void _OnDiceResult(int value)
	{
		_result = value;
		Transform face = die.GetFace(value);
		_resultDisplayChild.localRotation = face.localRotation.Inverse();
		die.GetComponent<SpringToTarget>().target = _resultDisplayChild;
		die.body.useGravity = false;
		die.body.isKinematic = true;
		onEarlyResult.Invoke(_result, _result >= _targetValue);
	}

	private void _OnResult()
	{
		_resultDisplayTime = 0f;
		die.GetComponent<SpringToTarget>().target = null;
		die.GetComponent<EaseToTarget>().target = startingPositionTransform;
		onResultValue.Invoke(_result);
		onResult.Invoke(_result >= _targetValue);
		die.GetComponent<EaseToTarget>().onTargetReached += delegate
		{
			die.GetComponent<EaseToTarget>().target = null;
			die.body.useGravity = true;
			die.body.isKinematic = false;
		};
	}

	private void Awake()
	{
		_resultDisplayChild = new GameObject("_DiceRollerResultDisplayChild").transform.SetParentAndReturn(resultDisplayTransform, worldPositionStays: false);
		die.GetOrAddComponent<SpringToTarget>();
		die.GetOrAddComponent<EaseToTarget>();
		die.GetOrAddComponent<ShowCanDrag>();
		die.GetComponent<AnimateNoise3d>().enabled = false;
		die.onResult.AddListener(_OnDiceResult);
	}

	private void Update()
	{
		PointerClick3D component = die.GetComponent<PointerClick3D>();
		bool flag2 = (die.GetComponent<PointerDrag3D>().enabled = die.GetComponent<EaseToTarget>().target == launcher.transform && die.GetComponent<EaseToTarget>().atTargetPosition);
		component.enabled = flag2;
		if (!(die.GetComponent<SpringToTarget>().target != _resultDisplayChild) && (_resultDisplayTime += GameUtil.GetDeltaTime(useScaledTime)) >= resultDisplayTime)
		{
			_OnResult();
		}
	}

	public void Roll(int targetValue)
	{
		_targetValue = targetValue;
		die.body.useGravity = false;
		die.GetComponent<EaseToTarget>().target = launcher.transform;
		die.GetComponent<AnimateNoise3d>().enabled = true;
	}

	public void Launch(PointerEventData eventData)
	{
		if (_frameOfLastLaunch != Time.frameCount)
		{
			_frameOfLastLaunch = Time.frameCount;
			die.GetComponent<AnimateNoise3d>().enabled = false;
			die.GetComponent<EaseToTarget>().target = null;
			die.body.useGravity = true;
			launcher.Launch(eventData);
			die.BeginRoll();
			PointerClick3D component = die.GetComponent<PointerClick3D>();
			bool flag2 = (die.GetComponent<PointerDrag3D>().enabled = false);
			component.enabled = flag2;
			onLaunch.Invoke();
		}
	}
}
