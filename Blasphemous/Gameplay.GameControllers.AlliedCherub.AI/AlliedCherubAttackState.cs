using Gameplay.GameControllers.Bosses.Snake;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.AlliedCherub.AI;

public class AlliedCherubAttackState : State
{
	public Vector2 lastTargetPos;

	private AlliedCherub AlliedCherub { get; set; }

	private Entity Target { get; set; }

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		AlliedCherub = machine.GetComponent<AlliedCherub>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		Target = AlliedCherub.Behaviour.Target;
	}

	private void ChaseEnemy()
	{
		if (Target == null)
		{
			AlliedCherub.Behaviour.OnTargetLost();
			return;
		}
		Vector2 vector = lastTargetPos;
		float chasingEnemyElongation = AlliedCherub.Behaviour.ChasingEnemyElongation;
		float chasingEnemySpeed = AlliedCherub.Behaviour.ChasingEnemySpeed;
		DamageArea damageArea = GetDamageArea(Target);
		if (damageArea != null)
		{
			vector = damageArea.DamageAreaCollider.bounds.center;
		}
		AlliedCherub.Behaviour.ChaseEntity(vector, chasingEnemyElongation, chasingEnemySpeed);
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		AccquireTarget();
		if (!AlliedCherub.Behaviour.IsShooting() && AlliedCherub.Behaviour.CanAttack() && AlliedCherub.Behaviour.IsInAttackRange(lastTargetPos))
		{
			DamageArea damageArea = GetDamageArea(Target);
			if (damageArea != null)
			{
				AlliedCherub.Behaviour.ShootRailgun(damageArea.DamageAreaCollider);
			}
		}
		else if (!AlliedCherub.Behaviour.IsShooting())
		{
			ChaseEnemy();
		}
	}

	private DamageArea GetDamageArea(Entity target)
	{
		if (target is Snake)
		{
			return (target as Snake).GetActiveDamageArea();
		}
		return Target.GetComponent<Entity>().EntityDamageArea;
	}

	private void AccquireTarget()
	{
		if (Target != null)
		{
			lastTargetPos = Target.transform.position;
			DamageArea damageArea = GetDamageArea(Target);
			if (damageArea != null)
			{
				lastTargetPos = damageArea.DamageAreaCollider.bounds.center;
			}
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		Target = null;
	}
}
