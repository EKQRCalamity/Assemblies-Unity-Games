using System;
using CreativeSpore.SmartColliders;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Combat;
using Gameplay.GameControllers.Effects.NPCs.BloodDecals;
using Gameplay.GameControllers.Enemies.Flagellant.Animator;
using Gameplay.GameControllers.Enemies.Flagellant.Audio;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Flagellant;

public class Flagellant : Enemy, IDamageable
{
	private MotionLerper _flagellantMotionLerper;

	private BloodDecalPumper _bloodDecalPumper;

	private EnemyDamageArea _enemyDamageArea;

	private BoxCollider2D _damageAreaCollider;

	private SmartPlatformCollider _flagellantSmartCollider;

	public float MAX_SPEED = 3.5f;

	public float MIN_SPEED = 1.4f;

	public float maxSpeedBeforeFalling;

	public bool canBeStunLocked = true;

	[Range(0f, 1f)]
	public float freezeTime = 0.1f;

	[Range(0f, 1f)]
	public float freezeTimeFactor = 0.2f;

	private float timeDead;

	public EntityExecution EntExecution { get; set; }

	public VisionCone VisionCone { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	public Rigidbody2D RigidBody { get; private set; }

	public MotionLerper MotionLerper { get; private set; }

	public FlagellantAnimatorInyector AnimatorInyector { get; set; }

	public FlagellantAudio Audio { get; set; }

	public NPCInputs Inputs { get; private set; }

	protected void flagellant_OnTrapFall()
	{
		float current = Stats.Life.Current;
		Damage(current, string.Empty);
		AnimatorInyector.Death();
	}

	public bool IsVisible()
	{
		return Entity.IsVisibleFrom(base.SpriteRenderer, UnityEngine.Camera.main);
	}

	public override EnemyFloorChecker EnemyFloorChecker()
	{
		return enemyFloorChecker;
	}

	public override EnemyAttack EnemyAttack()
	{
		return enemyAttack;
	}

	public override EnemyBumper EnemyBumper()
	{
		return enemyBumper;
	}

	private void OnEnable()
	{
		if (base.Landing)
		{
			base.Landing = !base.Landing;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		base.EnemyBehaviour = GetComponent<EnemyBehaviour>();
		RigidBody = GetComponent<Rigidbody2D>();
		base.Controller = GetComponent<PlatformCharacterController>();
		AnimatorInyector = GetComponentInChildren<FlagellantAnimatorInyector>();
		Audio = GetComponent<FlagellantAudio>();
		_flagellantMotionLerper = GetComponent<MotionLerper>();
		_bloodDecalPumper = GetComponentInChildren<BloodDecalPumper>();
		_enemyDamageArea = GetComponentInChildren<EnemyDamageArea>();
		_damageAreaCollider = _enemyDamageArea.GetComponent<BoxCollider2D>();
		enemyFloorChecker = GetComponentInChildren<EnemyFloorChecker>();
		Inputs = GetComponent<NPCInputs>();
		_flagellantSmartCollider = GetComponent<SmartPlatformCollider>();
		EntExecution = GetComponent<EntityExecution>();
		VisionCone = GetComponentInChildren<VisionCone>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		SetOrientation(EntityOrientation.Left);
		entityCurrentState = EntityStates.Idle;
		MotionLerper = GetComponent<MotionLerper>();
		SmartPlatformCollider flagellantSmartCollider = _flagellantSmartCollider;
		flagellantSmartCollider.OnSideCollision = (SmartRectCollider2D.OnSideCollisionDelegate)Delegate.Combine(flagellantSmartCollider.OnSideCollision, new SmartRectCollider2D.OnSideCollisionDelegate(flagellant_OnSideCollision));
		enemyAttack = GetComponentInChildren<EnemyAttack>();
		enemyBumper = GetComponentInChildren<EnemyBumper>();
		EnemyFloorChecker obj = enemyFloorChecker;
		obj.OnTrapFall = (Core.SimpleEvent)Delegate.Combine(obj.OnTrapFall, new Core.SimpleEvent(flagellant_OnTrapFall));
		base.Target = Core.Logic.Penitent.gameObject;
		EntExecution.enabled = false;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!base.Target)
		{
			base.Target = Core.Logic.Penitent.gameObject;
		}
		base.DistanceToTarget = Vector2.Distance(base.transform.position, base.Target.transform.position);
		Status.IsVisibleOnCamera = IsVisible();
		if (Status.Dead)
		{
			timeDead += Time.deltaTime;
			DisableDamageArea();
		}
		else if (base.Landing)
		{
			EnablePhysics();
		}
		if (timeDead >= 1f && Status.IsGrounded)
		{
			EnablePhysics(enable: false);
		}
		if (!base.Landing)
		{
			base.Landing = true;
			SetPositionAtStart();
		}
	}

	public override void HitDisplacement(Vector3 enemyPos, DamageArea.DamageType damageType)
	{
		if (!(base.Controller.SlopeAngle > 1f))
		{
			float x = base.transform.position.x;
			Vector2 vector = base.transform.right;
			if (enemyPos.x > x)
			{
				vector *= -1f;
			}
			if (!MotionLerper.IsLerping)
			{
				MotionLerper.TimeTakenDuringLerp = 0.25f;
				MotionLerper.distanceToMove = 2f;
				MotionLerper.StartLerping(vector);
			}
		}
	}

	public void SetMovementSpeed(float newSpeed)
	{
		base.Controller.MaxWalkingSpeed = newSpeed;
	}

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		float groundDist = base.Controller.GroundDist;
		Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y - groundDist, base.transform.position.z);
		base.transform.position = position;
	}

	protected override void EnablePhysics(bool enable = true)
	{
		if (enable)
		{
			if (!base.Controller.enabled)
			{
				base.Controller.enabled = true;
				base.EnemyBehaviour.StartBehaviour();
				base.Controller.PlatformCharacterPhysics.GravityScale = 1f;
			}
			if (!Inputs.enabled)
			{
				Inputs.enabled = true;
			}
			if (!_flagellantSmartCollider.EnableCollision2D)
			{
				_flagellantSmartCollider.EnableCollision2D = true;
			}
		}
		else
		{
			if (base.Controller.enabled)
			{
				base.Controller.enabled = false;
				base.EnemyBehaviour.PauseBehaviour();
				base.Controller.PlatformCharacterPhysics.GravityScale = 0f;
			}
			if (Inputs.enabled)
			{
				Inputs.enabled = false;
			}
			if (_flagellantSmartCollider.EnableCollision2D)
			{
				_flagellantSmartCollider.EnableCollision2D = false;
			}
		}
	}

	private void flagellant_OnSideCollision(SmartCollision2D collision, GameObject collidedObject)
	{
		if (!Status.IsVisibleOnCamera)
		{
			return;
		}
		SetFlag("SIDE_BLOCKED", active: false);
		for (int i = 0; i < collision.contacts.Length; i++)
		{
			if ((Vector2)collision.contacts[i].normal == -Vector2.right || (Vector2)collision.contacts[i].normal == Vector2.right)
			{
				SetFlag("SIDE_BLOCKED", active: true);
				if (_flagellantMotionLerper.IsLerping)
				{
					_flagellantMotionLerper.StopLerping();
					break;
				}
			}
		}
	}

	public void DisableDamageArea()
	{
		if ((bool)_damageAreaCollider)
		{
			_damageAreaCollider.enabled = false;
		}
	}

	protected void PumpBloodDecal()
	{
	}

	public void Damage(Hit hit)
	{
		if (Status.Unattacable)
		{
			return;
		}
		if (Execution(hit))
		{
			GetStun(hit);
			return;
		}
		_enemyDamageArea.TakeDamage(hit);
		if (canBeStunLocked)
		{
			AnimatorInyector.DamageImpact();
			SetOrientationbyHit(hit.AttackingEntity.transform.position);
		}
		SleepTimeByHit(hit);
		PumpBloodDecal();
		base.EnemyBehaviour.GotParry = false;
	}

	public override void Parry()
	{
		base.Parry();
		base.EnemyBehaviour.Parry();
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public override void GetStun(Hit hit)
	{
		base.GetStun(hit);
		if (!base.IsStunt && Mathf.Abs(base.Controller.SlopeAngle) < 1f)
		{
			Core.Audio.EventOneShotPanned(hit.HitSoundId, base.transform.position);
			base.EnemyBehaviour.Execution();
		}
	}
}
