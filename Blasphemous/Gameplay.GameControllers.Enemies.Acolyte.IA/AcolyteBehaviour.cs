using System.Collections;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Acolyte.IA;

public class AcolyteBehaviour : EnemyBehaviour
{
	private Acolyte _acolyte;

	private AttackArea _attackArea;

	private RaycastHit2D[] _bottomHits;

	private RaycastHit2D[] _forwardsHits;

	private float _currentGroundDetection;

	private float _deltaAttackTime;

	private int _maxHitsAllocated;

	[Header("Sensors variables")]
	private float _myWidth;

	[Header("Sensors variables")]
	private float _myHeight;

	public bool AllowEntityOrientation;

	public float AttackTime = 1f;

	public float MaxRangeGroundDetection = 5f;

	public float attackRange = 5f;

	[Tooltip("The length of the block detection raycast")]
	[Range(0f, 1f)]
	public float RangeBlockDectection = 0.5f;

	[Tooltip("The length og the ground detection raycast")]
	[Range(0f, 10f)]
	public float rangeGroundDetection = 2f;

	public LayerMask TargetLayer;

	public GameObject Target { get; set; }

	public bool IsTargetOnRange { get; set; }

	public bool IsTargetOnSight { get; set; }

	public bool IsAttackWindowOpen { get; set; }

	public bool IsIdleTimeElapsed { get; set; }

	public bool CanSeeTarget => _acolyte.VisionCone.CanSeeTarget(_acolyte.Target.transform, "Penitent");

	public override void OnAwake()
	{
		base.OnAwake();
		_acolyte = GetComponent<Acolyte>();
	}

	public override void OnStart()
	{
		base.OnStart();
		BoxCollider2D componentInChildren = GetComponentInChildren<BoxCollider2D>();
		_myWidth = componentInChildren.bounds.extents.x;
		_myHeight = componentInChildren.bounds.extents.y;
		_currentGroundDetection = rangeGroundDetection;
		_attackArea = _acolyte.AttackArea;
		_maxHitsAllocated = 2;
		_bottomHits = new RaycastHit2D[_maxHitsAllocated];
		_forwardsHits = new RaycastHit2D[_maxHitsAllocated];
		_acolyte.OnDeath += AcolyteOnEntityDie;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		IsIdleTimeElapsed = _deltaAttackTime >= AttackTime;
		if ((bool)Target)
		{
			TargetOnRange(Target);
		}
		IsTargetOnSight = TargetOnSight();
		if (Entity.Status.Orientation == EntityOrientation.Left)
		{
			Vector2 vector = (Vector2)base.transform.position - ((Vector2)base.transform.right * _myWidth * 1.5f + Vector2.up * (_myHeight * 2f));
			Debug.DrawLine(vector, vector - Vector2.up * _currentGroundDetection, Color.yellow);
			base.SensorHitsFloor = Physics2D.LinecastNonAlloc(vector, vector - Vector2.up * _currentGroundDetection, _bottomHits, BlockLayerMask) > 0;
			Debug.DrawLine(vector, vector - (Vector2)base.transform.right * RangeBlockDectection, Color.yellow);
			isBlocked = Physics2D.LinecastNonAlloc(vector, vector - (Vector2)base.transform.right * RangeBlockDectection, _forwardsHits, BlockLayerMask) > 0;
		}
		else
		{
			Vector2 vector = (Vector2)base.transform.position + ((Vector2)base.transform.right * _myWidth * 1.5f - Vector2.up * (_myHeight * 2f));
			Debug.DrawLine(vector, vector - Vector2.up * _currentGroundDetection, Color.yellow);
			base.SensorHitsFloor = Physics2D.LinecastNonAlloc(vector, vector - Vector2.up * _currentGroundDetection, _bottomHits, BlockLayerMask) > 0;
			Debug.DrawLine(vector, vector + (Vector2)base.transform.right * RangeBlockDectection, Color.yellow);
			isBlocked = Physics2D.LinecastNonAlloc(vector, vector + (Vector2)base.transform.right * RangeBlockDectection, _forwardsHits, BlockLayerMask) > 0;
		}
		TrapDetected = DetectTrap(_bottomHits);
		CheckGrounded();
		if (!base.SensorHitsFloor && _acolyte.MotionLerper.IsLerping)
		{
			_acolyte.MotionLerper.StopLerping();
		}
	}

	public void CheckGrounded()
	{
		if (!(_acolyte == null))
		{
			bool isGrounded = _acolyte.Status.IsGrounded;
			_acolyte.AnimatorInyector.Grounded(isGrounded);
		}
	}

	public override void StopMovement()
	{
		if (!(_acolyte == null))
		{
			_acolyte.SetMovementSpeed(0f);
			_acolyte.Inputs.HorizontalInput = 0f;
			if (Mathf.Abs(_acolyte.Controller.PlatformCharacterPhysics.HSpeed) > 0f)
			{
				_acolyte.Controller.PlatformCharacterPhysics.HSpeed = 0f;
			}
			_acolyte.Controller.PlatformCharacterPhysics.Velocity = Vector3.zero;
		}
	}

	private void AcolyteOnEntityDie()
	{
		base.BehaviourTree.StopBehaviour();
		StopMovement();
		_acolyte.StopMovementLerping();
		_acolyte.AnimatorInyector.Dead();
	}

	public bool TargetOnRange(GameObject target)
	{
		IsTargetOnRange = Vector2.Distance(base.transform.position, target.transform.position) <= attackRange;
		return IsTargetOnRange;
	}

	public bool TargetOnSight()
	{
		int num = ((_acolyte.Status.Orientation == EntityOrientation.Right) ? 1 : (-1));
		return Physics2D.Raycast(_attackArea.transform.position, Vector2.right * num, 10f, TargetLayer);
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (!_acolyte.Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
		{
			base.LookAtTarget(targetPos);
		}
	}

	public override void ReverseOrientation()
	{
		if (!_acolyte.Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
		{
			base.ReverseOrientation();
		}
	}

	public override void Idle()
	{
		if (!(_acolyte == null))
		{
			if (!_acolyte.Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
			{
				_deltaAttackTime += Time.deltaTime;
			}
			_acolyte.AnimatorInyector.Idle();
			StopMovement();
		}
	}

	public override void Wander()
	{
		if (!(_acolyte == null))
		{
			bool flag = _acolyte.Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
			if (!flag)
			{
				_deltaAttackTime += Time.deltaTime;
			}
			_acolyte.AnimatorInyector.Wander();
			float horizontalInput = ((_acolyte.Status.Orientation != 0) ? (-1f) : 1f);
			_acolyte.Inputs.HorizontalInput = horizontalInput;
			if (!flag)
			{
				_acolyte.SetMovementSpeed(_acolyte.MinSpeed);
			}
		}
	}

	public override void Chase(Transform targetPosition)
	{
		if (_acolyte == null)
		{
			return;
		}
		bool flag = _acolyte.Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
		if (!flag)
		{
			_deltaAttackTime += Time.deltaTime;
		}
		_acolyte.AnimatorInyector.Wander();
		if (!flag)
		{
			float horizontalInput = ((_acolyte.Status.Orientation != 0) ? (-1f) : 1f);
			if (_acolyte.IsFalling)
			{
				StopMovement();
				return;
			}
			_acolyte.SetMovementSpeed(_acolyte.MinSpeed);
			_acolyte.Inputs.HorizontalInput = horizontalInput;
		}
	}

	public void StopChase()
	{
		StopMovement();
		_acolyte.AnimatorInyector.StopChasing();
	}

	public override void Attack()
	{
		if (!(_acolyte == null))
		{
			_deltaAttackTime = 0f;
			bool isGrounded = _acolyte.Controller.IsGrounded;
			_acolyte.AnimatorInyector.Attack(isGrounded);
			StopMovement();
		}
	}

	public override void Damage()
	{
		if (!(_acolyte == null))
		{
			if (_acolyte.Animator.speed > 1f)
			{
				_acolyte.Animator.speed = 1f;
			}
			StopMovement();
			_acolyte.SetMovementSpeed(_acolyte.MinSpeed);
		}
	}

	public override void Parry()
	{
		base.Parry();
		_acolyte.AnimatorInyector.ParryReaction();
		Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("Parry");
		_acolyte.Animator.speed = 1.75f;
		StopBehaviour();
		StartCoroutine(AdjustParryDistance());
	}

	private Vector2 GetDirFromOrientation()
	{
		return new Vector2((_acolyte.Status.Orientation == EntityOrientation.Right) ? 1 : (-1), 0f);
	}

	private IEnumerator AdjustParryDistance()
	{
		float parryDistance = 2.5f;
		float curDist = Vector2.Distance(base.transform.position, Core.Logic.Penitent.transform.position);
		if (curDist > parryDistance)
		{
			StopMovement();
			float oldDistance = _acolyte.MotionLerper.distanceToMove;
			Vector2 dir = GetDirFromOrientation();
			_acolyte.MotionLerper.distanceToMove = curDist - parryDistance;
			_acolyte.MotionLerper.StartLerping(dir);
			float counter = 0.5f;
			while (counter > 0f && curDist > parryDistance)
			{
				counter -= Time.deltaTime;
				yield return null;
			}
			_acolyte.MotionLerper.StopLerping();
			_acolyte.MotionLerper.distanceToMove = oldDistance;
		}
	}

	public override void Execution()
	{
		base.Execution();
		_acolyte.gameObject.layer = LayerMask.NameToLayer("Default");
		StopMovement();
		StopBehaviour();
		_acolyte.SpriteRenderer.enabled = false;
		Core.Logic.Penitent.Audio.PlaySimpleHitToEnemy();
		_acolyte.EntExecution.InstantiateExecution();
	}

	public override void Alive()
	{
		base.Alive();
		_acolyte.gameObject.layer = LayerMask.NameToLayer("Enemy");
		_acolyte.SpriteRenderer.enabled = true;
		_acolyte.AnimatorInyector.Idle();
		_acolyte.Animator.speed = 1f;
		_acolyte.Animator.Play("Idle");
		_acolyte.CurrentLife = _acolyte.Stats.Life.Base / 2f;
		StartBehaviour();
		if (_acolyte.EntExecution != null)
		{
			_acolyte.EntExecution.enabled = false;
		}
	}
}
