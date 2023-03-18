using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Projectiles;

public class BoomerangProjectile : StraightProjectile
{
	public enum BOOMERANG_PHASES
	{
		TO_TARGET,
		BRAKE,
		ACCELERATE,
		BACK_TO_ORIGIN
	}

	public delegate void BoomerangProjectileDelegate(BoomerangProjectile b);

	public float brakeDistance = 5f;

	public float brakeSeconds = 0.5f;

	public float accelerationSeconds = 0.5f;

	public float accelerationFactor = 1.2f;

	public float lastDistanceToTarget;

	public AnimationCurve brakeCurve;

	public AnimationCurve accelCurve;

	public ProjectileReaction reactionChecker;

	public BOOMERANG_PHASES _currentPhase;

	private Vector3 originPosition;

	private Vector3 targetPosition;

	private float _distanceToTarget;

	private float _maxSpeed;

	private float _counter;

	[Header("DEBUG")]
	private float debugMarkersDuration = 3f;

	private float pickupDistance = 0.5f;

	public event BoomerangProjectileDelegate OnBackToOrigin;

	public override void Init(Vector3 origin, Vector3 target, float speed)
	{
		base.Init(origin, target, speed);
		targetPosition = target;
		originPosition = origin;
		_maxSpeed = speed;
		_currentPhase = BOOMERANG_PHASES.TO_TARGET;
		reactionChecker.OnProjectileHit += OnProjectileHitsBlade;
	}

	private void OnProjectileHitsBlade(ProjectileReaction obj)
	{
		if (_currentPhase == BOOMERANG_PHASES.TO_TARGET || _currentPhase == BOOMERANG_PHASES.BRAKE)
		{
			_currentPhase = BOOMERANG_PHASES.ACCELERATE;
			_counter = 0f;
			velocity = (originPosition - base.transform.position).normalized;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		switch (_currentPhase)
		{
		case BOOMERANG_PHASES.TO_TARGET:
			Debug.DrawLine(base.transform.position, base.transform.position + Vector3.up * 0.1f, Color.green, debugMarkersDuration);
			UpdatePhaseToTarget();
			break;
		case BOOMERANG_PHASES.BRAKE:
			Debug.DrawLine(base.transform.position, base.transform.position + Vector3.up * 0.1f, Color.yellow, debugMarkersDuration);
			UpdatePhaseBrake();
			break;
		case BOOMERANG_PHASES.ACCELERATE:
			Debug.DrawLine(base.transform.position, base.transform.position - Vector3.up * 0.1f, Color.red, debugMarkersDuration);
			UpdatePhaseAccelerate();
			break;
		case BOOMERANG_PHASES.BACK_TO_ORIGIN:
			Debug.DrawLine(base.transform.position, base.transform.position - Vector3.up * 0.1f, Color.cyan, debugMarkersDuration);
			UpdatePhaseBackToOrigin();
			break;
		}
	}

	private void UpdatePhaseToTarget()
	{
		_distanceToTarget = Vector2.Distance(base.transform.position, targetPosition);
		lastDistanceToTarget = _distanceToTarget;
		if (_distanceToTarget < brakeDistance)
		{
			_counter = 0f;
			_currentPhase = BOOMERANG_PHASES.BRAKE;
		}
	}

	private void UpdatePhaseBrake()
	{
		float t = brakeCurve.Evaluate(_counter / brakeSeconds);
		Vector2 vector = Vector2.Lerp(velocity, Vector2.zero, t);
		_counter += Time.deltaTime;
		velocity = vector;
		if (_counter >= brakeSeconds)
		{
			_currentPhase = BOOMERANG_PHASES.ACCELERATE;
			_counter = 0f;
			velocity = (originPosition - base.transform.position).normalized;
		}
	}

	private void UpdatePhaseAccelerate()
	{
		float t = accelCurve.Evaluate(_counter / accelerationSeconds);
		Vector2 vector = Vector2.Lerp(velocity, velocity.normalized * _maxSpeed, t);
		_counter += Time.deltaTime;
		velocity = vector;
		_distanceToTarget = (base.transform.position - originPosition).magnitude;
		if (_distanceToTarget < pickupDistance)
		{
			OnReachedOrigin();
		}
		if (_counter >= accelerationSeconds)
		{
			_currentPhase = BOOMERANG_PHASES.BACK_TO_ORIGIN;
		}
	}

	private void UpdatePhaseBackToOrigin()
	{
		_distanceToTarget = (base.transform.position - originPosition).magnitude;
		if (_distanceToTarget < pickupDistance)
		{
			OnReachedOrigin();
		}
	}

	private void OnReachedOrigin()
	{
		Debug.Log("BOOMERANG: BACK TO ORIGIN");
		Debug.DrawLine(base.transform.position - Vector3.up * 0.5f, base.transform.position + Vector3.up * 0.5f, Color.red, debugMarkersDuration);
		Debug.DrawLine(base.transform.position - Vector3.right * 0.5f, base.transform.position + Vector3.right * 0.5f, Color.red, debugMarkersDuration);
		if (this.OnBackToOrigin != null)
		{
			this.OnBackToOrigin(this);
		}
	}
}
