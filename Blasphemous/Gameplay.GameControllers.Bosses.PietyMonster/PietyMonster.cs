using CreativeSpore.SmartColliders;
using DamageEffect;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.PietyMonster.Animation;
using Gameplay.GameControllers.Bosses.PietyMonster.Attack;
using Gameplay.GameControllers.Bosses.PietyMonster.IA;
using Gameplay.GameControllers.Bosses.PietyMonster.Sound;
using Gameplay.GameControllers.Effects.Player.Dust;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.EntityGizmos;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PietyMonster;

public class PietyMonster : Enemy, IDamageable
{
	public enum BossStatus
	{
		First,
		Second,
		Third,
		Forth,
		Fifth,
		Sixth
	}

	public AnimationCurve slowTimeCurve;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string finalHit;

	public BossStatus CurrentBossStatus;

	public PietyMonsterAnimatorInyector AnimatorInyector { get; private set; }

	public NPCInputs Inputs { get; private set; }

	public PietyMonsterBehaviour PietyBehaviour { get; private set; }

	public BossBodyBarrier BodyBarrier { get; private set; }

	public DamageEffectScript DamageEffect { get; set; }

	public EnemyDamageArea DamageArea { get; set; }

	public PietyMonsterAudio Audio { get; set; }

	public EntityRootMotion SpitingMouth { get; set; }

	public PietyAnimatorBridge AnimatorBridge { get; set; }

	public StepDustSpawner DustSpawner { get; set; }

	public SmartPlatformCollider Collider { get; private set; }

	public EntityRumble Rumble { get; private set; }

	public PietyRootsManager PietyRootsManager { get; set; }

	public bool CanMove { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Rumble = GetComponentInChildren<EntityRumble>();
		AnimatorInyector = GetComponentInChildren<PietyMonsterAnimatorInyector>();
		AnimatorBridge = GetComponentInChildren<PietyAnimatorBridge>();
		Inputs = GetComponent<NPCInputs>();
		base.EnemyBehaviour = GetComponentInChildren<EnemyBehaviour>();
		PietyBehaviour = (PietyMonsterBehaviour)base.EnemyBehaviour;
		BodyBarrier = GetComponentInChildren<BossBodyBarrier>();
		DamageEffect = GetComponentInChildren<DamageEffectScript>();
		Audio = GetComponentInChildren<PietyMonsterAudio>();
		base.Controller = GetComponent<PlatformCharacterController>();
		SpitingMouth = GetComponentInChildren<EntityRootMotion>();
		PietyRootsManager = Object.FindObjectOfType<PietyRootsManager>();
		DustSpawner = GetComponentInChildren<StepDustSpawner>();
		Collider = GetComponent<SmartPlatformCollider>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
		PietyBehaviour.PauseBehaviour();
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		base.Target = Core.Logic.Penitent.gameObject;
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.Target != null)
		{
			base.DistanceToTarget = Vector2.Distance(base.transform.position, base.Target.transform.position);
		}
		bool enable = base.DistanceToTarget <= ActivationRange;
		if (!Status.Dead)
		{
			EnablePiety(enable);
		}
	}

	public override EnemyFloorChecker EnemyFloorChecker()
	{
		return null;
	}

	public override EnemyAttack EnemyAttack()
	{
		return PietyBehaviour.StompAttack;
	}

	public override EnemyBumper EnemyBumper()
	{
		return null;
	}

	private void EnablePiety(bool enable = true)
	{
		EnablePhysics(enable);
	}

	protected override void EnablePhysics(bool enable = true)
	{
		if (Collider == null)
		{
			return;
		}
		if (enable)
		{
			if (!Collider.enabled)
			{
				Collider.enabled = true;
			}
			if (!base.Controller.enabled)
			{
				base.Controller.enabled = true;
			}
		}
		else
		{
			if (Collider.enabled)
			{
				Collider.enabled = false;
			}
			if (base.Controller.enabled)
			{
				base.Controller.enabled = false;
			}
		}
	}

	private void EnableBehaviour(bool enable = true)
	{
		if (PietyBehaviour == null)
		{
			return;
		}
		if (enable)
		{
			if (!PietyBehaviour.enabled)
			{
				PietyBehaviour.enabled = true;
			}
			PietyBehaviour.StartBehaviour();
		}
		else
		{
			if (PietyBehaviour.enabled)
			{
				PietyBehaviour.enabled = false;
			}
			PietyBehaviour.PauseBehaviour();
		}
	}

	public void Damage(Hit hit)
	{
		if (!Status.Dead && !(Core.Logic.Penitent.Stats.Life.Current <= 0f))
		{
			if (WillDieByHit(hit))
			{
				hit.HitSoundId = finalHit;
			}
			DamageArea.TakeDamage(hit);
			if (Status.Dead)
			{
				Core.Logic.ScreenFreeze.Freeze(0.01f, 2f, 0f, slowTimeCurve);
				Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeBW(0.5f);
			}
			else
			{
				DamageEffect.Blink(0f, 0.1f);
				SleepTimeByHit(hit);
			}
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}
}
