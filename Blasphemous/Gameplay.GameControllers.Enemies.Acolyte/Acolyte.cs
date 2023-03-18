using System;
using CreativeSpore.SmartColliders;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Combat;
using Gameplay.GameControllers.Effects.NPCs.BloodDecals;
using Gameplay.GameControllers.Enemies.Acolyte.Animator;
using Gameplay.GameControllers.Enemies.Acolyte.Audio;
using Gameplay.GameControllers.Enemies.Acolyte.IA;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Acolyte;

public class Acolyte : Enemy, IDamageable
{
	private BloodDecalPumper _bloodDecalPumper;

	private EnemyDamageArea _enemyDamageArea;

	private AcolyteAttack _acolyteAttack;

	private EnemyAI _acolyteAi;

	private SmartPlatformCollider _acolyteSmartCollider;

	private int _defaultLayer;

	private int _enemyLayer;

	public float MaxSpeed = 3.5f;

	public float MinSpeed = 1.4f;

	public float MaxSpeedBeforeFalling;

	private bool _hasEnemyLayer;

	[Range(0f, 1f)]
	public float FreezeTime = 0.1f;

	[Range(0f, 1f)]
	public float FreezeTimeFactor = 0.2f;

	private float _timeDead;

	public MotionLerper MotionLerper { get; private set; }

	public AcolyteBehaviour Behaviour { get; private set; }

	public EntityExecution EntExecution { get; set; }

	public AcolyteAnimatorInyector AnimatorInyector { get; set; }

	public AcolyteAudio Audio { get; set; }

	public NPCInputs Inputs { get; set; }

	public AttackArea AttackArea { get; private set; }

	public Rigidbody2D Rigidbody { get; private set; }

	public AcolyteAttackAnimations AttackAnimations { get; private set; }

	public VisionCone VisionCone { get; private set; }

	public RigidbodyType2D RigidbodyType
	{
		get
		{
			return Rigidbody.bodyType;
		}
		set
		{
			Rigidbody.bodyType = value;
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	private void acolyte_OnAttack(object param)
	{
		StartCoroutine(FreezeAnimator(FreezeTime));
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
		AnimatorInyector = GetComponentInChildren<AcolyteAnimatorInyector>();
		Inputs = GetComponent<NPCInputs>();
		base.Controller = GetComponent<PlatformCharacterController>();
		base.EnemyBehaviour = GetComponent<EnemyBehaviour>();
		Audio = GetComponent<AcolyteAudio>();
		MotionLerper = GetComponent<MotionLerper>();
		_bloodDecalPumper = GetComponentInChildren<BloodDecalPumper>();
		_enemyDamageArea = GetComponentInChildren<EnemyDamageArea>();
		_acolyteAttack = GetComponentInChildren<AcolyteAttack>();
		enemyFloorChecker = GetComponentInChildren<EnemyFloorChecker>();
		_acolyteSmartCollider = GetComponent<SmartPlatformCollider>();
		Rigidbody = GetComponent<Rigidbody2D>();
		AttackArea = GetComponentInChildren<AttackArea>();
		AttackAnimations = GetComponentInChildren<AcolyteAttackAnimations>();
		VisionCone = GetComponentInChildren<VisionCone>();
		_defaultLayer = LayerMask.NameToLayer("Default");
		_enemyLayer = LayerMask.NameToLayer("Enemy");
		_hasEnemyLayer = true;
	}

	protected override void OnStart()
	{
		base.OnStart();
		AcolyteAttack acolyteAttack = _acolyteAttack;
		acolyteAttack.OnEntityAttack = (Core.GenericEvent)Delegate.Combine(acolyteAttack.OnEntityAttack, new Core.GenericEvent(acolyte_OnAttack));
		SetOrientation(EntityOrientation.Left);
		entityCurrentState = EntityStates.Idle;
		Behaviour = GetComponent<AcolyteBehaviour>();
		EnemyFloorChecker obj = enemyFloorChecker;
		obj.OnTrapFall = (Core.SimpleEvent)Delegate.Combine(obj.OnTrapFall, new Core.SimpleEvent(acolyte_OnTrapFall));
		enemyAttack = GetComponentInChildren<EnemyAttack>();
		enemyBumper = GetComponentInChildren<EnemyBumper>();
		EntExecution = GetComponent<EntityExecution>();
		base.Target = Core.Logic.Penitent.gameObject;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.Target == null)
		{
			base.Target = Core.Logic.Penitent.gameObject;
		}
		base.DistanceToTarget = Vector2.Distance(base.transform.position, base.Target.transform.position);
		Status.IsVisibleOnCamera = IsVisible();
		if (Status.Dead)
		{
			_timeDead += Time.deltaTime;
			DisableDamageArea();
			return;
		}
		if (_timeDead >= 1f && HasFlag("GROUNDED"))
		{
			EnablePhysics(enable: false);
		}
		if (!Status.IsGrounded)
		{
			StopMovementLerping();
		}
		if (!base.Landing)
		{
			base.Landing = true;
			EnablePhysics();
			SetPositionAtStart();
		}
	}

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		float groundDist = base.Controller.GroundDist;
		Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y - groundDist, base.transform.position.z);
		base.transform.position = position;
	}

	public void SetMovementSpeed(float movementSpeed)
	{
		base.Controller.MaxWalkingSpeed = movementSpeed;
	}

	public void StopMovementLerping()
	{
		if (MotionLerper.IsLerping)
		{
			MotionLerper.StopLerping();
		}
	}

	public override void HitDisplacement(Vector3 enemyPos, DamageArea.DamageType damageType)
	{
		base.HitDisplacement(enemyPos, damageType);
		if (!(base.Controller.SlopeAngle > 1f))
		{
			Vector3 right = base.transform.right;
			right = ((!(enemyPos.x >= base.transform.position.x)) ? right : (-right));
			if (!MotionLerper.IsLerping)
			{
				MotionLerper.TimeTakenDuringLerp = 0.5f;
				MotionLerper.distanceToMove = 1.5f;
				MotionLerper.StartLerping(right);
			}
		}
	}

	protected override void EnablePhysics(bool enable = true)
	{
		if (enable)
		{
			if (!base.Controller.enabled)
			{
				base.Controller.enabled = true;
				Behaviour.StartBehaviour();
				base.Controller.PlatformCharacterPhysics.GravityScale = 1f;
			}
			if (!Inputs.enabled)
			{
				Inputs.enabled = true;
			}
			if (!_acolyteSmartCollider.EnableCollision2D)
			{
				_acolyteSmartCollider.EnableCollision2D = true;
			}
		}
		else
		{
			if (base.Controller.enabled)
			{
				base.Controller.enabled = false;
				Behaviour.PauseBehaviour();
				base.Controller.PlatformCharacterPhysics.GravityScale = 0f;
			}
			if (Inputs.enabled)
			{
				Inputs.enabled = false;
			}
			if (_acolyteSmartCollider.EnableCollision2D)
			{
				_acolyteSmartCollider.EnableCollision2D = false;
			}
		}
	}

	public void EnableEnemyLayer(bool enable = true)
	{
		if (enable && !_hasEnemyLayer)
		{
			_hasEnemyLayer = true;
			base.gameObject.layer = _enemyLayer;
			_enemyDamageArea.gameObject.layer = _enemyLayer;
		}
		else if (!enable && _hasEnemyLayer)
		{
			_hasEnemyLayer = false;
			base.gameObject.layer = _defaultLayer;
			_enemyDamageArea.gameObject.layer = _defaultLayer;
		}
	}

	public void DisableDamageArea()
	{
		if (_enemyDamageArea != null)
		{
			_enemyDamageArea.DamageAreaCollider.enabled = false;
		}
	}

	public void EnableDamageArea()
	{
		if (_enemyDamageArea != null)
		{
			_enemyDamageArea.DamageAreaCollider.enabled = true;
		}
	}

	private void acolyte_OnTrapFall()
	{
		AnimatorInyector.Dead();
		float current = Stats.Life.Current;
		Damage(current, string.Empty);
	}

	public void Damage(Hit hit)
	{
		if (base.EnemyBehaviour.GotParry)
		{
			hit.HitSoundId = string.Empty;
		}
		if (Execution(hit))
		{
			GetStun(hit);
			return;
		}
		string id = ((hit.DamageType != 0) ? "PenitentHeavyEnemyHit" : "PenitentSimpleEnemyHit");
		Core.Audio.PlaySfxOnCatalog(id);
		if (!base.Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
		{
			_enemyDamageArea.TakeDamage(hit);
			AttackAnimations.ColorFlash.TriggerColorFlash();
			SleepTimeByHit(hit);
			AnimatorInyector.Damage();
			SetOrientationbyHit(hit.AttackingEntity.transform.position);
		}
		if (Behaviour.GotParry)
		{
			Behaviour.GotParry = false;
			Behaviour.StartBehaviour();
		}
	}

	public override void Parry()
	{
		base.Parry();
		Behaviour.GotParry = true;
		StopMovementLerping();
		base.EnemyBehaviour.Parry();
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
