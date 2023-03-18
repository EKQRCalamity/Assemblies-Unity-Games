using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Stoners.Animator;

public class StonerAnimatorInyector : EnemyAnimatorInyector
{
	public void Hurt()
	{
		base.EntityAnimator.SetTrigger("HURT");
	}

	public void Raise(Vector3 targetPos)
	{
		if (base.EntityAnimator.speed < 1f)
		{
			base.EntityAnimator.speed = 1f;
		}
		SetRaiseOrientation(targetPos);
		Stoners stoners = (Stoners)OwnerEntity;
		if ((bool)stoners)
		{
			stoners.StonersDamageArea.DamageAreaCollider.enabled = true;
		}
	}

	private void SetRaiseOrientation(Vector3 targetPos)
	{
		string stateName = ((!(targetPos.x >= base.transform.position.x)) ? "RisingLeft" : "RisingRight");
		base.EntityAnimator.Play(stateName);
	}

	public void Attack()
	{
		base.EntityAnimator.SetTrigger("ATTACK");
	}

	public void AllowOrientation(bool allow)
	{
		base.EntityAnimator.SetBool("FLIP", allow);
	}
}
