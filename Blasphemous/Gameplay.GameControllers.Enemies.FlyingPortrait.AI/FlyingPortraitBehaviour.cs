using System;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.FlyingPortrait.AI;

public class FlyingPortraitBehaviour : EnemyBehaviour
{
	protected FlyingPortrait FlyingPortrait;

	[FoldoutGroup("Attack settings", true, 0)]
	public float AttackCoolDown = 2f;

	[FoldoutGroup("Attack settings", true, 0)]
	public float DistanceAttack = 3f;

	[FoldoutGroup("Attack settings", true, 0)]
	public float ChasingSpeed = 3f;

	public bool IsAwake { get; set; }

	public override void OnStart()
	{
		base.OnStart();
		FlyingPortrait = (FlyingPortrait)Entity;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (FlyingPortrait.Status.Dead || base.GotParry)
		{
			FlyingPortrait.StateMachine.SwitchState<FlyingPortraitDeathState>();
		}
		else if (base.PlayerSeen || base.PlayerHeard)
		{
			FlyingPortrait.StateMachine.SwitchState<FlyingPortraitAttackState>();
		}
		else
		{
			FlyingPortrait.StateMachine.SwitchState<FlyingPortraitWanderState>();
		}
	}

	public override void Idle()
	{
		throw new NotImplementedException();
	}

	public override void Wander()
	{
		float num = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		base.transform.Translate(Vector2.right * num * Time.deltaTime, Space.World);
	}

	public override void Chase(Transform targetPosition)
	{
		if (!base.IsAttacking)
		{
			float num = ((!(targetPosition.transform.position.x > Entity.transform.position.x)) ? (-1f) : 1f);
			base.transform.Translate(Vector2.right * num * ChasingSpeed * Time.deltaTime, Space.World);
		}
	}

	public override void Parry()
	{
		base.Parry();
		base.GotParry = true;
		FlyingPortrait.AnimatorInjector.ParryReaction();
	}

	public override void Execution()
	{
		base.Execution();
		base.GotParry = true;
		FlyingPortrait.gameObject.layer = LayerMask.NameToLayer("Default");
		FlyingPortrait.SpriteRenderer.enabled = false;
		FlyingPortrait.Attack.gameObject.SetActive(value: false);
		Core.Logic.Penitent.Audio.PlaySimpleHitToEnemy();
		FlyingPortrait.Execution.InstantiateExecution();
		if (FlyingPortrait.Execution != null)
		{
			FlyingPortrait.Execution.enabled = true;
		}
	}

	public override void Alive()
	{
		base.Alive();
		base.GotParry = false;
		FlyingPortrait.gameObject.layer = LayerMask.NameToLayer("Enemy");
		FlyingPortrait.SpriteRenderer.enabled = true;
		FlyingPortrait.Attack.gameObject.SetActive(value: true);
		FlyingPortrait.CurrentLife = FlyingPortrait.Stats.Life.Base / 2f;
		FlyingPortrait.AnimatorInjector.Alive();
		if (FlyingPortrait.Execution != null)
		{
			FlyingPortrait.Execution.enabled = false;
		}
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void Damage()
	{
		throw new NotImplementedException();
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}
}
