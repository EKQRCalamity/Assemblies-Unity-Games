using System;
using System.Collections;
using CreativeSpore.SmartColliders;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.PietyMonster.Animation;
using Gameplay.GameControllers.Enemies.BellCarrier.Animator;
using Gameplay.GameControllers.Enemies.BellCarrier.Attack;
using Gameplay.GameControllers.Enemies.BellCarrier.Audio;
using Gameplay.GameControllers.Enemies.BellCarrier.IA;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.BellCarrier;

public class BellCarrier : Enemy, IDamageable
{
	public BellCarrierAudio Audio;

	public BellCarrierBehaviour BellCarrierBehaviour;

	public NPCInputs Inputs;

	public MotionLerper MotionLerper;

	public BellCarrierAnimatorInyector AnimatorInyector { get; set; }

	public EnemyDamageArea DamageArea { get; set; }

	public BossBodyBarrier BodyBarrier { get; set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	public bool IsVisible => Entity.IsVisibleFrom(base.SpriteRenderer, UnityEngine.Camera.main);

	public void Damage(Hit hit)
	{
		if (Status.Dead)
		{
			return;
		}
		DamageArea.LastHit = hit;
		GameObject attackingEntity = hit.AttackingEntity;
		if (Status.Orientation == EntityOrientation.Left)
		{
			if (!(attackingEntity.transform.position.x >= DamageArea.DamageAreaCollider.bounds.center.x))
			{
				Audio.PlayBellCarrierFrontHit();
				return;
			}
			Audio.PlayDamageSound();
			AnimatorInyector.TriggerColorFlash();
			BellCarrierBehaviour.Damage();
			SleepTimeByHit(hit);
			hit.HitSoundId = string.Empty;
			DamageArea.TakeDamage(hit);
		}
		else if (!(attackingEntity.transform.position.x < DamageArea.DamageAreaCollider.bounds.center.x))
		{
			Audio.PlayBellCarrierFrontHit();
		}
		else
		{
			Audio.PlayDamageSound();
			AnimatorInyector.TriggerColorFlash();
			BellCarrierBehaviour.Damage();
			SleepTimeByHit(hit);
			hit.HitSoundId = string.Empty;
			DamageArea.TakeDamage(hit);
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
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
		AnimatorInyector = GetComponentInChildren<BellCarrierAnimatorInyector>();
		base.EnemyBehaviour = GetComponent<BellCarrierBehaviour>();
		BellCarrierBehaviour = (BellCarrierBehaviour)base.EnemyBehaviour;
		Inputs = GetComponentInChildren<NPCInputs>();
		base.Controller = GetComponentInChildren<PlatformCharacterController>();
		enemyAttack = GetComponentInChildren<BellCarrierAttack>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Audio = GetComponentInChildren<BellCarrierAudio>();
		MotionLerper = GetComponent<MotionLerper>();
		BodyBarrier = GetComponentInChildren<BossBodyBarrier>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		base.EnemyBehaviour.enabled = false;
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		base.Target = penitent.gameObject;
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.EnemyBehaviour.enabled = true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		Status.IsVisibleOnCamera = IsVisible;
		Status.IsGrounded = base.Controller.IsGrounded;
		DamageArea.DamageAreaCollider.transform.localScale = Vector2.one;
		if (base.Target == null)
		{
			base.Target = Core.Logic.Penitent.gameObject;
		}
		base.DistanceToTarget = Vector2.Distance(base.transform.position, base.Target.transform.position);
		if (base.Landing)
		{
			EnablePhysics(enable: true);
		}
		if (!base.Landing)
		{
			base.Landing = true;
			SetPositionAtStart();
		}
	}

	public override EnemyFloorChecker EnemyFloorChecker()
	{
		throw new NotImplementedException();
	}

	public override EnemyAttack EnemyAttack()
	{
		return enemyAttack;
	}

	public override EnemyBumper EnemyBumper()
	{
		throw new NotImplementedException();
	}

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		float groundDist = base.Controller.GroundDist;
		Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y - groundDist, base.transform.position.z);
		base.transform.position = position;
		base.Controller.PlatformCharacterPhysics.GravityScale = 0f;
	}

	protected override void EnablePhysics(bool enable)
	{
		if (enable)
		{
			if (!base.Controller.enabled)
			{
				base.Controller.enabled = true;
				base.EnemyBehaviour.StartBehaviour();
			}
		}
		else if (base.Controller.enabled)
		{
			base.Controller.enabled = false;
			base.EnemyBehaviour.PauseBehaviour();
		}
	}

	protected void SetGravityScale(float gravityScale)
	{
		if (!(base.Controller == null))
		{
			base.Controller.PlatformCharacterPhysics.Gravity = new Vector3(0f, gravityScale, 0f);
		}
	}

	protected IEnumerator RestoreGravityScale(float gravityScale, float lapse)
	{
		yield return new WaitForSeconds(lapse);
		SetGravityScale(gravityScale);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}
}
