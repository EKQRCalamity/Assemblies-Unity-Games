using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshCollider))]
public class PhysicsDice : MonoBehaviour
{
	private static readonly float VELOCITY_THRESHOLD = Mathf.Pow(0.01f, 2f);

	private static readonly float ANGULAR_VELOCITY_THRESHOLD = Mathf.Pow(57.29578f, 2f);

	[SerializeField]
	protected bool _useScaledTime;

	[SerializeField]
	[Range(0f, 1f)]
	protected float _restTimeToResult = 0.33f;

	[SerializeField]
	[Range(0.25f, 4f)]
	protected float _restThresholdMultiplier = 1f;

	[SerializeField]
	protected Transform[] _faces;

	[SerializeField]
	protected IntEvent _onResult;

	[Header("Rigid Body Settings")]
	[SerializeField]
	[Range(0f, 1000f)]
	protected float _maxAngularVelocity = 50f;

	[SerializeField]
	[Range(1f, 100f)]
	protected int _solverIterations = 30;

	[SerializeField]
	[Range(1f, 100f)]
	protected int _solverVelocityIterations = 1;

	private Rigidbody _body;

	private MeshCollider _meshCollider;

	private bool _rolling;

	private bool _atRest;

	private bool _onSurface;

	private float _timeAtRest;

	public Rigidbody body => this.CacheComponent(ref _body);

	public MeshCollider meshCollider => this.CacheComponent(ref _meshCollider);

	protected Transform[] faces => _faces ?? (_faces = new Transform[0]);

	public IntEvent onResult => _onResult ?? (_onResult = new IntEvent());

	private void _UpdateRigidBodySettings()
	{
		body.maxAngularVelocity = _maxAngularVelocity;
		body.solverIterations = _solverIterations;
		body.solverVelocityIterations = _solverVelocityIterations;
	}

	private void _EndRoll()
	{
		_rolling = false;
		int num = 0;
		float num2 = float.MinValue;
		for (int i = 0; i < faces.Length; i++)
		{
			float y = faces[i].forward.y;
			if (!(y < num2))
			{
				num2 = y;
				num = i;
			}
		}
		onResult.Invoke(num + 1);
	}

	private void Awake()
	{
		_UpdateRigidBodySettings();
	}

	private void FixedUpdate()
	{
		_onSurface = false;
		_atRest = _rolling && body.velocity.sqrMagnitude < VELOCITY_THRESHOLD * _restThresholdMultiplier && body.angularVelocity.sqrMagnitude < ANGULAR_VELOCITY_THRESHOLD * _restThresholdMultiplier;
	}

	private void OnCollisionStay(Collision collision)
	{
		_onSurface = true;
	}

	private void Update()
	{
		if ((_onSurface || body.IsSleeping()) && _atRest)
		{
			_timeAtRest += GameUtil.GetDeltaTime(_useScaledTime);
		}
		else
		{
			_timeAtRest = 0f;
		}
		if (_timeAtRest >= _restTimeToResult)
		{
			_EndRoll();
		}
	}

	public void BeginRoll()
	{
		_rolling = true;
	}

	public Transform GetFace(int value)
	{
		return faces[value - 1];
	}
}
