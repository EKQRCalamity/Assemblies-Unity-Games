using System;
using DG.Tweening;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Swimmer.Animator;

public class SwimmerAnimatorInyector : EnemyAnimatorInyector
{
	protected Vector2 GroundPosition { get; private set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();
		SetJumpParam();
		if (OwnerEntity.Status.IsGrounded)
		{
			GroundPosition = OwnerEntity.transform.position;
		}
	}

	public void SetJumpParam()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("GROUNDED", OwnerEntity.Status.IsGrounded);
		}
	}

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void Dispose()
	{
		OwnerEntity.gameObject.SetActive(value: false);
	}

	public void SpriteVisible(bool visible, float timeElapsed, Action callback = null)
	{
		OwnerEntity.SpriteRenderer.DOFade((!visible) ? 0f : 1f, timeElapsed).OnComplete(delegate
		{
			if (callback != null)
			{
				callback();
			}
		});
	}

	public void TerrainEffectRising(int isRising)
	{
		Swimmer swimmer = (Swimmer)OwnerEntity;
		Vector3 position = swimmer.transform.position;
		position.y = GroundPosition.y;
		swimmer.Behaviour.RisingTerrainEffect(isRising > 0, position);
	}

	public void Attack()
	{
		Swimmer swimmer = (Swimmer)OwnerEntity;
		swimmer.Behaviour.Attack();
	}
}
