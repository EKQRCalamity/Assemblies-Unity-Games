using UnityEngine;

public class LerpToTarget : ATransformToTarget
{
	[Header("Lerp to Target")]
	public Transform lerpFrom;

	[Range(0.01f, 100f)]
	public float duration = 1f;

	public AnimationCurve positionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve rotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve scaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public bool resetOnEnable = true;

	private float _t;

	protected override bool _isValid
	{
		get
		{
			if (base._isValid)
			{
				return lerpFrom;
			}
			return false;
		}
	}

	public override bool atTarget => _t == 1f;

	private void OnEnable()
	{
		if (resetOnEnable)
		{
			_t = 0f;
		}
	}

	private void Update()
	{
		if (_isValid)
		{
			_t = Mathf.Clamp01(_t + GameUtil.GetDeltaTime(useScaledTime) / duration);
		}
	}

	protected override void _UpdatePosition(float deltaTime)
	{
		base.transform.position = lerpFrom.position.Lerp(target.position, positionCurve.Evaluate(_t));
	}

	protected override void _UpdateRotation(float deltaTime)
	{
		base.transform.rotation = Quaternion.SlerpUnclamped(lerpFrom.rotation, target.rotation, rotationCurve.Evaluate(_t));
	}

	protected override void _UpdateScale(float deltaTime)
	{
		base.transform.SetWorldScale(lerpFrom.GetWorldScale().Lerp(target.GetWorldScale(), scaleCurve.Evaluate(_t)));
	}

	public override void Finish()
	{
		base.Finish();
		_t = 1f;
	}

	public LerpToTarget SetData(Transform lerpFrom, Transform target)
	{
		this.lerpFrom = lerpFrom;
		base.target = target;
		return this;
	}

	public LerpToTarget SetData(Transform lerpFrom, Transform target, float duration, bool useScaledTime, AnimationCurve positionCurve, AnimationCurve rotationCurve, AnimationCurve scaleCurve, bool positionEnabled, bool rotationEnabled, bool scaleEnabled)
	{
		this.duration = duration;
		base.useScaledTime = useScaledTime;
		this.positionCurve = positionCurve;
		this.rotationCurve = rotationCurve;
		this.scaleCurve = scaleCurve;
		base.positionEnabled = positionEnabled;
		base.rotationEnabled = rotationEnabled;
		base.scaleEnabled = scaleEnabled;
		return SetData(lerpFrom, target);
	}
}
