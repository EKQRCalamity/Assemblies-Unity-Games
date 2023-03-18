using System;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Framework.IA;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Stoners.AI;

public class StonerBehaviour : EnemyBehaviour
{
	private Stoners _stoners;

	public float CurrentAtackWaitTime { get; set; }

	public bool IsRaised { get; set; }

	public bool IsVisible => _stoners.Status.IsVisibleOnCamera;

	public override void OnAwake()
	{
		base.OnAwake();
		_stoners = GetComponent<Stoners>();
		Entity = _stoners;
	}

	public override void OnStart()
	{
		base.OnStart();
		_stoners.OnDeath += StonersOnEntityDie;
	}

	public void Raise(Vector3 targetPos)
	{
		_stoners.AnimatorInyector.Raise(targetPos);
	}

	public override void Idle()
	{
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
		_stoners.AnimatorInyector.Attack();
	}

	public override void Damage()
	{
		throw new NotImplementedException();
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (Entity == null)
		{
			return;
		}
		Entity.SpriteRenderer.flipX = false;
		if (targetPos.x >= Entity.transform.position.x)
		{
			if (Entity.Status.Orientation != 0)
			{
				_stoners.SetOrientation(EntityOrientation.Right, allowFlipRenderer: false);
				_stoners.AnimatorInyector.AllowOrientation(allow: true);
			}
		}
		else if (Entity.Status.Orientation != EntityOrientation.Left)
		{
			_stoners.SetOrientation(EntityOrientation.Left, allowFlipRenderer: false);
			_stoners.AnimatorInyector.AllowOrientation(allow: true);
		}
	}

	private void StonersOnEntityDie()
	{
		base.BehaviourTree.StopBehaviour();
	}
}
