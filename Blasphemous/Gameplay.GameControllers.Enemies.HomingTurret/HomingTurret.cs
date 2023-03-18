using System;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.HomingTurret.AI;
using Gameplay.GameControllers.Enemies.HomingTurret.Animation;
using Gameplay.GameControllers.Enemies.HomingTurret.Audio;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.HomingTurret;

public class HomingTurret : Enemy, IDamageable
{
	public VisionCone Vision { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public HomingTurretAnimationInyector AnimationInyector { get; private set; }

	public HomingTurretAudio Audio { get; private set; }

	private MasterShaderEffects ColorBlink { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Vision = GetComponentInChildren<VisionCone>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		base.EnemyBehaviour = GetComponentInChildren<EnemyBehaviour>();
		AnimationInyector = GetComponentInChildren<HomingTurretAnimationInyector>();
		ColorBlink = GetComponentInChildren<MasterShaderEffects>();
		Audio = GetComponentInChildren<HomingTurretAudio>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		SetPositionAtStart();
	}

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		if (!base.Landing)
		{
			float distance = Physics2D.Raycast(base.transform.position, -Vector2.up, 5f, base.EnemyBehaviour.BlockLayerMask).distance;
			Vector3 position = base.transform.position;
			position.y -= distance;
			base.transform.position = position;
			base.Landing = true;
		}
	}

	public override EnemyFloorChecker EnemyFloorChecker()
	{
		throw new NotImplementedException();
	}

	public override EnemyAttack EnemyAttack()
	{
		throw new NotImplementedException();
	}

	public override EnemyBumper EnemyBumper()
	{
		throw new NotImplementedException();
	}

	protected override void EnablePhysics(bool enable = true)
	{
		throw new NotImplementedException();
	}

	public void Damage(Hit hit)
	{
		DamageArea.TakeDamage(hit);
		SleepTimeByHit(hit);
		ColorBlink.DamageEffectBlink(0f, ColorBlink.FlashTimeAmount, ColorBlink.damageEffectTestMaterial);
		HomingTurretBehaviour homingTurretBehaviour = (HomingTurretBehaviour)base.EnemyBehaviour;
		if (Status.Dead)
		{
			homingTurretBehaviour.Dead();
			AnimationInyector.Death();
			DisableEnemyBarrier();
		}
		else
		{
			homingTurretBehaviour.Damage();
		}
	}

	private void DisableEnemyBarrier()
	{
		EnemyBarrier componentInChildren = GetComponentInChildren<EnemyBarrier>();
		if ((bool)componentInChildren)
		{
			componentInChildren.gameObject.SetActive(value: false);
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}
}
