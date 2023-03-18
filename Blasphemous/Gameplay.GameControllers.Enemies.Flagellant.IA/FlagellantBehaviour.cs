using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Flagellant.IA;

public class FlagellantBehaviour : EnemyBehaviour
{
	public const float IdleAnimClipDuration = 2.87f;

	public const float MaxTimeWandering = 8f;

	private RaycastHit2D[] _bottomHits;

	private RaycastHit2D[] _forwardsHits;

	private float _currentGroundDetection;

	private float _currentTimeWandering;

	private Flagellant _flagellant;

	private int _maxHitsAllocated;

	[Header("Sensors variables")]
	private float _myWidth;

	[Header("Sensors variables")]
	private float _myHeight;

	private float _wanderTimer;

	public bool AllowEntityOrientation;

	public float MaxRangeGroundDetection = 5f;

	[Tooltip("The length of the block detection raycast")]
	[Range(0f, 1f)]
	public float RangeBlockDectection = 0.5f;

	[Tooltip("The length og the ground detection raycast")]
	[Range(0f, 10f)]
	public float RangeGroundDetection = 2f;

	public bool CanSeeTarget => _flagellant.VisionCone.CanSeeTarget(_flagellant.Target.transform, "Penitent");

	public override void OnAwake()
	{
		base.OnAwake();
		_flagellant = GetComponent<Flagellant>();
		Entity = _flagellant;
	}

	public override void OnStart()
	{
		base.OnStart();
		BoxCollider2D componentInChildren = GetComponentInChildren<BoxCollider2D>();
		_myWidth = componentInChildren.bounds.extents.x;
		_myHeight = componentInChildren.bounds.extents.y;
		_currentGroundDetection = RangeGroundDetection;
		_maxHitsAllocated = 2;
		_bottomHits = new RaycastHit2D[_maxHitsAllocated];
		_forwardsHits = new RaycastHit2D[_maxHitsAllocated];
		_flagellant.OnDeath += FlagellantOnEntityDie;
		_currentTimeWandering = GetMaxWanderingLapse();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (Entity.Status.Orientation == EntityOrientation.Left)
		{
			Vector2 vector = (Vector2)base.transform.position - ((Vector2)base.transform.right * _myWidth * 0.75f + Vector2.up * (_myHeight * 2f));
			Debug.DrawLine(vector, vector - Vector2.up * _currentGroundDetection, Color.yellow);
			base.SensorHitsFloor = Physics2D.LinecastNonAlloc(vector, vector - Vector2.up * _currentGroundDetection, _bottomHits, BlockLayerMask) > 0;
			Debug.DrawLine(vector, vector - (Vector2)base.transform.right * RangeBlockDectection, Color.yellow);
			isBlocked = Physics2D.LinecastNonAlloc(vector, vector - (Vector2)base.transform.right * RangeBlockDectection, _forwardsHits, BlockLayerMask) > 0;
		}
		else
		{
			Vector2 vector = (Vector2)base.transform.position + ((Vector2)base.transform.right * _myWidth * 0.75f - Vector2.up * (_myHeight * 2f));
			Debug.DrawLine(vector, vector - Vector2.up * _currentGroundDetection, Color.yellow);
			base.SensorHitsFloor = Physics2D.LinecastNonAlloc(vector, vector - Vector2.up * _currentGroundDetection, _bottomHits, BlockLayerMask) > 0;
			Debug.DrawLine(vector, vector + (Vector2)base.transform.right * RangeBlockDectection, Color.yellow);
			isBlocked = Physics2D.LinecastNonAlloc(vector, vector + (Vector2)base.transform.right * RangeBlockDectection, _forwardsHits, BlockLayerMask) > 0;
		}
		TrapDetected = DetectTrap(_bottomHits);
		CheckGrounded();
	}

	protected void CheckGrounded()
	{
		if (!(_flagellant == null))
		{
			bool isGrounded = _flagellant.Controller.IsGrounded;
			_flagellant.AnimatorInyector.Grounded(isGrounded);
		}
	}

	public override void Idle()
	{
		if (!(_flagellant == null))
		{
			_currentGroundDetection = RangeGroundDetection;
			_flagellant.AnimatorInyector.Idle();
			_flagellant.Inputs.HorizontalInput = 0f;
			if (_flagellant.Controller.PlatformCharacterPhysics.Velocity.magnitude > 0f && _flagellant.Status.IsGrounded)
			{
				_flagellant.Controller.PlatformCharacterPhysics.Velocity = Vector3.zero;
			}
		}
	}

	public override void Wander()
	{
		if (_flagellant == null)
		{
			return;
		}
		_currentTimeWandering += Time.deltaTime;
		if (_currentTimeWandering <= 8f)
		{
			_currentGroundDetection = RangeGroundDetection;
			_flagellant.AnimatorInyector.Wander();
			float horizontalInput = ((_flagellant.Status.Orientation != 0) ? (-1f) : 1f);
			_flagellant.Inputs.HorizontalInput = horizontalInput;
			_flagellant.SetMovementSpeed(_flagellant.MIN_SPEED);
			_wanderTimer = Time.time;
		}
		else
		{
			Idle();
			if (Time.time - _wanderTimer > 5.74f)
			{
				_currentTimeWandering = GetMaxWanderingLapse();
			}
		}
	}

	public override void Chase(Transform target)
	{
		if (_flagellant == null)
		{
			return;
		}
		_currentGroundDetection = RangeGroundDetection * 1f;
		bool isGrounded = _flagellant.Controller.IsGrounded;
		_flagellant.AnimatorInyector.Chase(isGrounded);
		float num = ((_flagellant.Status.Orientation != 0) ? (-1f) : 1f);
		if (_flagellant.IsFalling || base.IsAttacking)
		{
			StopMovement();
			return;
		}
		_flagellant.Inputs.HorizontalInput = num;
		if (Mathf.Abs(_flagellant.Controller.PlatformCharacterPhysics.HSpeed) < _flagellant.MAX_SPEED)
		{
			_flagellant.Controller.PlatformCharacterPhysics.HSpeed = _flagellant.MAX_SPEED * num;
		}
		_flagellant.SetMovementSpeed(_flagellant.MAX_SPEED);
	}

	public override void StopMovement()
	{
		if (!(_flagellant == null))
		{
			_flagellant.SetMovementSpeed(0f);
			_flagellant.Inputs.HorizontalInput = 0f;
			if (Mathf.Abs(_flagellant.Controller.PlatformCharacterPhysics.HSpeed) > 0f)
			{
				_flagellant.Controller.PlatformCharacterPhysics.HSpeed = 0f;
			}
		}
	}

	public override void Attack()
	{
		if (!(_flagellant == null))
		{
			bool isGrounded = _flagellant.Controller.IsGrounded;
			_flagellant.AnimatorInyector.Attack(isGrounded);
			StopMovement();
		}
	}

	public override void Damage()
	{
		if (!(_flagellant == null))
		{
			StopMovement();
			_flagellant.AnimatorInyector.Hurt();
			_flagellant.SetMovementSpeed(_flagellant.MIN_SPEED);
		}
	}

	public override void Parry()
	{
		base.Parry();
		base.GotParry = true;
		_flagellant.AnimatorInyector.ParryReaction();
	}

	public override void Execution()
	{
		base.Execution();
		_flagellant.gameObject.layer = LayerMask.NameToLayer("Default");
		StopMovement();
		StopBehaviour();
		_flagellant.SpriteRenderer.enabled = false;
		Core.Logic.Penitent.Audio.PlaySimpleHitToEnemy();
		_flagellant.EntExecution.InstantiateExecution();
		if (_flagellant.EntExecution != null)
		{
			_flagellant.EntExecution.enabled = true;
		}
	}

	public override void Alive()
	{
		base.Alive();
		_flagellant.gameObject.layer = LayerMask.NameToLayer("Enemy");
		_flagellant.SpriteRenderer.enabled = true;
		_flagellant.AnimatorInyector.Idle();
		_flagellant.Animator.Play("Idle");
		_flagellant.CurrentLife = _flagellant.Stats.Life.Base / 2f;
		StartBehaviour();
		if (_flagellant.EntExecution != null)
		{
			_flagellant.EntExecution.enabled = false;
		}
	}

	private float GetMaxWanderingLapse()
	{
		return Random.Range(0f, 4f);
	}

	private void FlagellantOnEntityDie()
	{
		StopMovement();
		base.BehaviourTree.StopBehaviour();
	}

	public bool IsPatrolBlocked()
	{
		return _flagellant.MotionChecker.HitsPatrolBlock;
	}
}
