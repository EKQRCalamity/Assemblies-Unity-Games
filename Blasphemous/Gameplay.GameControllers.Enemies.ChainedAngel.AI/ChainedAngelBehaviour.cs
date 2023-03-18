using System;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Framework.IA;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ChainedAngel.AI;

public class ChainedAngelBehaviour : EnemyBehaviour
{
	public float AttackLapse = 2f;

	public Vector2 AttackOffsetPosition;

	public ChainedAngel ChainedAngel { get; private set; }

	public bool CanSeeTarget => ChainedAngel.VisionCone.CanSeeTarget(ChainedAngel.Target.transform, "Penitent");

	public override void OnStart()
	{
		base.OnStart();
		ChainedAngel = (ChainedAngel)Entity;
		ChainedAngel.StateMachine.SwitchState<ChainedAngelIdleState>();
		ChainedAngel.OnDeath += OnDeath;
	}

	private void OnDeath()
	{
		ChainedAngel.OnDeath -= OnDeath;
		ChainedAngel.StateMachine.enabled = false;
		ChainedAngel.BodyChainMaster.ForceStopAttack();
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (targetPos.x > ChainedAngel.transform.position.x)
		{
			if (ChainedAngel.Status.Orientation != 0)
			{
				ChainedAngel.SetOrientation(EntityOrientation.Right);
			}
		}
		else if (ChainedAngel.Status.Orientation != EntityOrientation.Left)
		{
			ChainedAngel.SetOrientation(EntityOrientation.Left);
		}
	}

	public override void Idle()
	{
		throw new NotImplementedException();
	}

	public override void Wander()
	{
		throw new NotImplementedException();
	}

	public override void Chase(Transform targetPosition)
	{
		throw new NotImplementedException();
	}

	public override void Attack()
	{
		ChainedAngel.BodyChainMaster.SnakeAttack(AttackOffsetPosition);
		ChainedAngel.Audio.PlayAttack();
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
