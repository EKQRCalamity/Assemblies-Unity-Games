using DG.Tweening;
using Gameplay.GameControllers.Enemies.MeltedLady.IA;
using Gameplay.GameControllers.Penitent.Gizmos;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.MeltedLady.Animator;

public class MeltedLadyAnimatorInyector : FloatingLadyAnimatorInjector
{
	private Vector3 _target;

	public AnimationCurve AttackAnimationCurve;

	public float RecoilLapse = 0.15f;

	public float RecoilDistance = 0.5f;

	public override void Attack()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("ATTACK");
		}
	}

	private Quaternion GetShotRotation()
	{
		MeltedLady meltedLady = (MeltedLady)OwnerEntity;
		Vector3 position = meltedLady.transform.position;
		Vector2 vector = ((!(meltedLady.Target.gameObject.transform.position.x < position.x)) ? ((Vector2)(meltedLady.Target.transform.position - OwnerEntity.transform.position)) : ((Vector2)(OwnerEntity.transform.position - meltedLady.Target.transform.position)));
		float value = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		value = Mathf.Clamp(value, -20f, 20f);
		return Quaternion.AngleAxis(value, Vector3.forward);
	}

	public override void Hurt()
	{
		if ((bool)base.EntityAnimator && !base.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
		{
			base.EntityAnimator.SetTrigger("HURT");
			base.EntityAnimator.ResetTrigger("ATTACK");
			SetDefaultRotation(0.1f);
		}
	}

	public override void Death()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public override void TeleportOut()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("TELEPORT_OUT");
		}
	}

	public override void TeleportIn()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger("TELEPORT_IN");
		}
	}

	public void AttackAnimationEvent()
	{
	}

	public void SetTargetRotation(float timeLapse)
	{
		MeltedLady meltedLady = (MeltedLady)OwnerEntity;
		meltedLady.SpriteRenderer.transform.DORotateQuaternion(GetShotRotation(), timeLapse).SetEase(AttackAnimationCurve);
	}

	public void SetDefaultRotation(float timeLapse)
	{
		MeltedLady meltedLady = (MeltedLady)OwnerEntity;
		DOTween.Kill(meltedLady.SpriteRenderer.transform);
		meltedLady.SpriteRenderer.transform.DORotateQuaternion(Quaternion.identity, timeLapse);
	}

	public void GetTarget()
	{
		MeltedLady meltedLady = (MeltedLady)OwnerEntity;
		_target = meltedLady.Target.transform.position;
	}

	public void LaunchProjectile()
	{
		MeltedLady meltedLady = (MeltedLady)OwnerEntity;
		RootMotionDriver projectileLaunchRoot = meltedLady.ProjectileLaunchRoot;
		Vector3 vector = ((meltedLady.Status.Orientation != 0) ? projectileLaunchRoot.FlipedPosition : projectileLaunchRoot.transform.position);
		Vector3 target = _target;
		target.y += 1f;
		Vector3 normalized = (target - vector).normalized;
		meltedLady.Attack.Shoot(vector, normalized);
		Recoil(OwnerEntity.transform.position + normalized.normalized * (0f - RecoilDistance));
	}

	private void Recoil(Vector2 dir)
	{
		OwnerEntity.transform.DOMove(dir, RecoilLapse);
	}

	public void ResetCoolDownAttack()
	{
		MeltedLadyBehaviour componentInChildren = OwnerEntity.GetComponentInChildren<MeltedLadyBehaviour>();
		if (componentInChildren != null)
		{
			componentInChildren.ResetCoolDown();
		}
	}

	public void TriggerTeleport()
	{
		MeltedLady meltedLady = (MeltedLady)OwnerEntity;
		if (meltedLady.Status.IsHurt)
		{
			meltedLady.Behaviour.TeleportToTarget();
		}
	}

	public void Dispose()
	{
		OwnerEntity.gameObject.SetActive(value: false);
	}
}
