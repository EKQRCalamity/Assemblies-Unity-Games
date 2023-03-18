using System;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Processioner.AI;

public class ProcessionerBehaviour : EnemyBehaviour
{
	private Processioner _processioner;

	private float _contactDamageCooldown;

	public bool CanMove => !_processioner.MotionChecker.HitsBlock;

	public bool TargetSeen => _processioner.VisionCone.CanSeeTarget(_processioner.Target.transform, "Penitent");

	public override void OnStart()
	{
		base.OnStart();
		_processioner = (Processioner)Entity;
		_processioner.OnDeath += OnDeath;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (_processioner.ContactDamage.IsTargetOverlapped && !_processioner.Status.Dead)
		{
			_contactDamageCooldown += Time.deltaTime;
			if (_contactDamageCooldown >= 0.1f)
			{
				_contactDamageCooldown = 0f;
				IDamageable component = _processioner.Target.GetComponent<IDamageable>();
				if (component != null)
				{
					_processioner.ContactDamage.EntityContactDamage(component);
				}
			}
		}
		else
		{
			_contactDamageCooldown = 0f;
		}
	}

	private void OnDeath()
	{
		_processioner.OnDeath -= OnDeath;
		EnableColliders(enableCollider: false);
		if (!(_processioner.ChainedAngel == null) && _processioner.ChainedAngel.StateMachine.enabled)
		{
			_processioner.ChainedAngel.StateMachine.enabled = false;
			_processioner.ChainedAngel.AnimatorInjector.Death();
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
