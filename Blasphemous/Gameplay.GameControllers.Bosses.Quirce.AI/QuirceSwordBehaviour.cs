using Framework.FrameworkCore;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent.Gizmos;
using Maikel.StatelessFSM;
using Plugins.Maikel.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Quirce.AI;

public class QuirceSwordBehaviour : Entity
{
	[FoldoutGroup("References", 0)]
	public RootMotionDriver rootMotion;

	[FoldoutGroup("References", 0)]
	public Entity ownerEntity;

	[FoldoutGroup("Floating settings", 0)]
	public float floatingFrequency;

	[FoldoutGroup("Floating settings", 0)]
	public float floatingAmplitude;

	[FoldoutGroup("Follow settings", 0)]
	public float invertedYOffset;

	[FoldoutGroup("Follow settings", 0)]
	public float smoothTranslationFactor;

	[FoldoutGroup("Follow settings", 0)]
	public float smoothRotationFactor;

	[FoldoutGroup("Follow settings", 0)]
	public bool doFollow = true;

	[FoldoutGroup("VFX", 0)]
	public GameObject sparksPrefab;

	[FoldoutGroup("VFX", 0)]
	public Vector2 collisionPoint;

	[FoldoutGroup("VFX", 0)]
	public float collisionRadius;

	[FoldoutGroup("VFX", 0)]
	public ContactFilter2D filter;

	[FoldoutGroup("VFX", 0)]
	public float secondsBetweenInstances = 0.5f;

	public GhostTrailGenerator ghostTrail;

	private bool _inverted;

	private EntityOrientation _orientation;

	private float _targetAngle;

	private Vector2 _targetPosition;

	private Vector2 _followPosition;

	private Vector2 _floatingOffset;

	private RaycastHit2D[] _results;

	private float _instantiationTimer;

	private StateMachine<QuirceSwordBehaviour> _fsm;

	private State<QuirceSwordBehaviour> stIdle;

	private State<QuirceSwordBehaviour> stSpinning;

	private State<QuirceSwordBehaviour> stSpinToPoint;

	private float _oldSmoothTranslationFactor;

	protected override void OnStart()
	{
		base.OnStart();
		ghostTrail = GetComponentInChildren<GhostTrailGenerator>();
		_results = new RaycastHit2D[5];
		stIdle = new QuirceSwordSt_Idle();
		stSpinning = new QuirceSwordSt_Spinning();
		stSpinToPoint = new QuirceSwordSt_SpinToPoint();
		_fsm = new StateMachine<QuirceSwordBehaviour>(this, stIdle);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		_fsm.DoUpdate();
	}

	public void SetAutoFollow(bool follow)
	{
		doFollow = follow;
	}

	public void GoToPoint(Vector2 point)
	{
		_targetPosition = point;
		_fsm.ChangeState(stSpinToPoint);
	}

	public void SetVisible(bool visible)
	{
		base.Animator.SetBool("VISIBLE", visible);
	}

	public void SetReversed(bool reversed)
	{
		_inverted = reversed;
	}

	public void SetSpinning(bool spin)
	{
		_fsm.ChangeState((!spin) ? stIdle : stSpinning);
		_instantiationTimer = secondsBetweenInstances;
		base.Animator.SetBool("SPIN", spin);
	}

	public void SetGhostTrail(bool active)
	{
		ghostTrail.EnableGhostTrail = active;
	}

	public bool IsCloseToPoint()
	{
		return Vector2.Distance(base.transform.position, _targetPosition) < 0.2f;
	}

	public void ReturnToSpinFollow()
	{
		_fsm.ChangeState(stSpinning);
	}

	public void SetSpeedFactor(float spdFactor)
	{
		_oldSmoothTranslationFactor = smoothTranslationFactor;
		smoothTranslationFactor = spdFactor;
	}

	public void SetNormalSpeed()
	{
		smoothTranslationFactor = _oldSmoothTranslationFactor;
	}

	public void ResetRotation()
	{
		base.transform.rotation = Quaternion.identity;
	}

	public void CheckCollision()
	{
		if (_instantiationTimer > 0f)
		{
			_instantiationTimer -= Time.deltaTime;
			return;
		}
		_instantiationTimer = secondsBetweenInstances;
		Vector2 origin = (Vector2)base.transform.position + collisionPoint;
		if (Physics2D.Raycast(origin, Vector2.down, filter, _results, collisionRadius) > 0)
		{
			Vector2 point = _results[0].point;
			Object.Instantiate(sparksPrefab, point, Quaternion.identity);
		}
	}

	public void UpdateFloatingOffset()
	{
		float y = Mathf.Sin(floatingFrequency * Time.time) * floatingAmplitude;
		_floatingOffset = new Vector2(0f, y);
	}

	public void SetTargetPosition()
	{
		if (!(rootMotion == null))
		{
			if (ownerEntity.Status.Orientation == EntityOrientation.Right)
			{
				_targetPosition = rootMotion.transform.position;
			}
			else
			{
				_targetPosition = rootMotion.ReversePosition;
			}
		}
	}

	public void SetTargetRotation()
	{
		if (!(rootMotion == null))
		{
			if (ownerEntity.Status.Orientation == EntityOrientation.Right)
			{
				_targetAngle = rootMotion.transform.localEulerAngles.z;
				_targetAngle += 90 * (_inverted ? 1 : 0);
			}
			else
			{
				_targetAngle = 0f - rootMotion.transform.localEulerAngles.z;
				_targetAngle += 90 * ((!_inverted) ? (-1) : (-2));
			}
		}
	}

	public void ApplyPosition()
	{
		_followPosition = _targetPosition + _floatingOffset;
		if (_inverted)
		{
			_followPosition += Vector2.up * invertedYOffset;
		}
		base.transform.position = Vector3.Lerp(base.transform.position, _followPosition, smoothTranslationFactor);
	}

	public void ApplyRotation()
	{
		Quaternion b = Quaternion.Euler(0f, 0f, _targetAngle);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, smoothRotationFactor);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(base.transform.position + (Vector3)collisionPoint, base.transform.position + (Vector3)collisionPoint + Vector3.down * collisionRadius);
	}
}
