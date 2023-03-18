using System;
using CreativeSpore.SmartColliders;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Amanecidas.Audio;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Amanecidas;

public class Amanecidas : Enemy, IDamageable
{
	public AnimationCurve slowTimeCurve;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string finalHit;

	public GameObject shieldPrefab;

	public Transform shieldParent;

	public float shieldMaxHP;

	public float shieldCurrentHP;

	public AmanecidaShield shield;

	public bool IsLaudes;

	public AmanecidasBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public NPCInputs Input { get; set; }

	public MasterShaderEffects DamageEffect { get; private set; }

	public GhostTrailGenerator GhostTrail { get; private set; }

	public AmanecidasAnimatorInyector AnimatorInyector { get; private set; }

	public LaudesArena LaudesArena { get; private set; }

	public AmanecidasAudio Audio { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.Controller = GetComponent<PlatformCharacterController>();
		Behaviour = GetComponent<AmanecidasBehaviour>();
		Audio = GetComponent<AmanecidasAudio>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Input = GetComponentInChildren<NPCInputs>();
		GhostTrail = GetComponentInChildren<GhostTrailGenerator>();
		DamageEffect = GetComponentInChildren<MasterShaderEffects>();
		AnimatorInyector = GetComponentInChildren<AmanecidasAnimatorInyector>();
		UnityEngine.Object.Instantiate(shieldPrefab, shieldParent);
		shield = GetComponentInChildren<AmanecidaShield>();
		shield.amanecidas = this;
		ActivateShield();
	}

	public override void SetOrientation(EntityOrientation orientation, bool allowFlipRenderer = true, bool searchForRenderer = false)
	{
		Status.Orientation = orientation;
		if (allowFlipRenderer)
		{
			switch (Status.Orientation)
			{
			case EntityOrientation.Left:
				AnimatorInyector.Flip(flip: true);
				break;
			case EntityOrientation.Right:
				AnimatorInyector.Flip(flip: false);
				break;
			}
		}
	}

	public void DamageFlash()
	{
		DamageEffect.DamageEffectBlink(0f, 0.07f);
	}

	private void TakeShieldDamage(Hit hit)
	{
		if (shieldCurrentHP <= 0f)
		{
			return;
		}
		Debug.Log("SHIELD takes " + hit.DamageAmount + " damage");
		if (!AnimatorInyector.IsOut())
		{
			shield.FlashShieldFromPenitent();
		}
		float num = shieldCurrentHP / shieldMaxHP;
		shield.SetDamagePercentage(num);
		shieldCurrentHP -= hit.DamageAmount;
		SleepTimeByHit(hit);
		if (shieldCurrentHP <= 0f)
		{
			shieldCurrentHP = 0f;
			BreakShield();
			if (IsLaudes && Behaviour.currentWeapon == AmanecidasAnimatorInyector.AMANECIDA_WEAPON.BOW)
			{
				Behaviour.MoveBattleBoundsIfNeeded();
			}
			Audio.PlayShieldDestroy_AUDIO();
		}
		else
		{
			Audio.PlayShield_AUDIO(num);
		}
	}

	public void ActivateShield()
	{
		base.IsGuarding = true;
		shieldCurrentHP = shieldMaxHP;
		shield.FlashShieldFromDown();
	}

	public void ForceBreakShield()
	{
		Hit hit = default(Hit);
		hit.DamageAmount = shieldCurrentHP;
		hit.DamageType = Gameplay.GameControllers.Entities.DamageArea.DamageType.Heavy;
		Hit hit2 = hit;
		TakeShieldDamage(hit2);
	}

	private void BreakShield()
	{
		Debug.Log("SHIELD BROKEN!!");
		shield.BreakShield();
		base.IsGuarding = false;
	}

	public void SetLaudesArena(LaudesArena arena, Vector2 origin, bool onlySetBoundaries)
	{
		LaudesArena = arena;
		LaudesArena.SetLaudesArena(this, origin, onlySetBoundaries);
	}

	public void SetNextLaudesArena(LaudesArena arena = null)
	{
		if (arena != null)
		{
			LaudesArena = arena;
		}
		if (LaudesArena == null)
		{
			Debug.LogError("No LaudesArena!");
		}
		else
		{
			LaudesArena.SetNextArena(this);
		}
	}

	public void SetupFight(AmanecidasFightSpawner.AMANECIDAS_FIGHTS fightType)
	{
		switch (fightType)
		{
		case AmanecidasFightSpawner.AMANECIDAS_FIGHTS.LANCE:
			Behaviour.SetWeapon(AmanecidasAnimatorInyector.AMANECIDA_WEAPON.LANCE);
			AnimatorInyector.SetAmanecidaColor(AmanecidasAnimatorInyector.AMANECIDA_COLOR.SKYBLUE);
			break;
		case AmanecidasFightSpawner.AMANECIDAS_FIGHTS.AXE:
			Behaviour.SetWeapon(AmanecidasAnimatorInyector.AMANECIDA_WEAPON.AXE);
			AnimatorInyector.SetAmanecidaColor(AmanecidasAnimatorInyector.AMANECIDA_COLOR.RED);
			break;
		case AmanecidasFightSpawner.AMANECIDAS_FIGHTS.FALCATA:
			Behaviour.SetWeapon(AmanecidasAnimatorInyector.AMANECIDA_WEAPON.SWORD);
			AnimatorInyector.SetAmanecidaColor(AmanecidasAnimatorInyector.AMANECIDA_COLOR.WHITE);
			break;
		case AmanecidasFightSpawner.AMANECIDAS_FIGHTS.BOW:
			Behaviour.SetWeapon(AmanecidasAnimatorInyector.AMANECIDA_WEAPON.BOW);
			AnimatorInyector.SetAmanecidaColor(AmanecidasAnimatorInyector.AMANECIDA_COLOR.BLUE);
			break;
		case AmanecidasFightSpawner.AMANECIDAS_FIGHTS.LAUDES:
			AnimatorInyector.SetAmanecidaColor(AmanecidasAnimatorInyector.AMANECIDA_COLOR.LAUDES);
			IsLaudes = true;
			Behaviour.SetWeapon(AmanecidasAnimatorInyector.AMANECIDA_WEAPON.SWORD);
			break;
		}
	}

	public override void Parry()
	{
		base.Parry();
		Behaviour.Parry();
	}

	public void Damage(Hit hit)
	{
		if (Behaviour.DodgeHit(hit) || Core.Logic.Penitent.Stats.Life.Current <= 0f || Status.Dead)
		{
			return;
		}
		if (GuardHit(hit))
		{
			SleepTimeByHit(hit);
			Behaviour.ShieldDamage(hit);
			TakeShieldDamage(hit);
			return;
		}
		DamageFlash();
		if (WillDieByHit(hit))
		{
			hit.HitSoundId = finalHit;
		}
		DamageArea.TakeDamage(hit);
		if (Status.Dead)
		{
			Core.Logic.ScreenFreeze.Freeze(0.05f, 4f, 0f, slowTimeCurve);
			Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeBW(0.5f);
			DamageArea.DamageAreaCollider.enabled = false;
			shield.Death();
			Behaviour.Death();
		}
		else
		{
			DamageFlash();
			Behaviour.Damage(hit);
			SleepTimeByHit(hit);
		}
	}

	public override EnemyAttack EnemyAttack()
	{
		throw new NotImplementedException();
	}

	public override EnemyBumper EnemyBumper()
	{
		throw new NotImplementedException();
	}

	public override EnemyFloorChecker EnemyFloorChecker()
	{
		throw new NotImplementedException();
	}

	public Vector3 GetPosition()
	{
		throw new NotImplementedException();
	}

	protected override void EnablePhysics(bool enable = true)
	{
		throw new NotImplementedException();
	}

	public float GetHpPercentage()
	{
		return Stats.Life.Current / Stats.Life.CurrentMax;
	}
}
