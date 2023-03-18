using System;
using DamageEffect;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.WallEnemy.Audio;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.WallEnemy;

public class WallEnemy : Enemy, IDamageable
{
	public LayerMask WallLayer;

	public EnemyDamageArea DamageArea { get; private set; }

	public DamageEffectScript DamageEffect { get; set; }

	public WallEnemyAudio Audio { get; set; }

	public GameObject ClimbableWall { get; private set; }

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	protected override void OnStart()
	{
		base.OnStart();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		base.EnemyBehaviour = GetComponentInChildren<EnemyBehaviour>();
		DamageEffect = GetComponentInChildren<DamageEffectScript>();
		Audio = GetComponentInChildren<WallEnemyAudio>();
	}

	public void Damage(Hit hit)
	{
		if (!Status.Dead)
		{
			Audio.PlayDamage();
			DamageArea.TakeDamage(hit);
			base.EnemyBehaviour.Damage();
			DamageEffect.Blink(0f, 0.1f);
			SleepTimeByHit(hit);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!Status.Dead && (WallLayer.value & (1 << other.gameObject.layer)) > 0)
		{
			ClimbableWall = other.gameObject;
			ClimbableWall.SetActive(value: false);
		}
	}

	private new void OnDestroy()
	{
		if ((bool)ClimbableWall)
		{
			ClimbableWall.SetActive(value: true);
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
}
