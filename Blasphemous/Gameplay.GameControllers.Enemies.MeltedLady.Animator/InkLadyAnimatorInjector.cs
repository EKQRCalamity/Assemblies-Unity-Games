using System.Collections;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.MeltedLady.Animator;

public class InkLadyAnimatorInjector : FloatingLadyAnimatorInjector
{
	private static readonly int In = UnityEngine.Animator.StringToHash("TELEPORT_IN");

	private static readonly int Out = UnityEngine.Animator.StringToHash("TELEPORT_OUT");

	private static readonly int DeathParam = UnityEngine.Animator.StringToHash("DEATH");

	private static readonly int AttackParam = UnityEngine.Animator.StringToHash("ATTACK");

	private static readonly int HurtParam = UnityEngine.Animator.StringToHash("HURT");

	protected InkLady InkLady { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		InkLady = (InkLady)OwnerEntity;
	}

	public override void Attack()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger(AttackParam);
			InkLady.IsAttacking = true;
			LaunchBeam();
		}
	}

	public override void Hurt()
	{
		if ((bool)base.EntityAnimator && !InkLady.IsAttacking && !base.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
		{
			base.EntityAnimator.SetTrigger(HurtParam);
			base.EntityAnimator.ResetTrigger(AttackParam);
		}
	}

	public override void Death()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.ResetTrigger(AttackParam);
			base.EntityAnimator.SetTrigger(DeathParam);
		}
	}

	public override void TeleportOut()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger(Out);
		}
	}

	public override void TeleportIn()
	{
		if ((bool)base.EntityAnimator)
		{
			base.EntityAnimator.SetTrigger(In);
		}
	}

	public void TriggerTeleport()
	{
		InkLady inkLady = (InkLady)OwnerEntity;
		if (inkLady.Status.IsHurt)
		{
			inkLady.Behaviour.TeleportToTarget();
		}
	}

	private void SetBeamPosition()
	{
		Vector3 localPosition = InkLady.Attack.transform.localPosition;
		localPosition.x = Mathf.Abs(localPosition.x);
		localPosition.x = ((!InkLady.SpriteRenderer.flipX) ? localPosition.x : (0f - localPosition.x));
		InkLady.Attack.transform.localPosition = localPosition;
	}

	private void LaunchBeam()
	{
		if (!InkLady.Status.Dead)
		{
			LookAtTarget();
			float num = 0.55f;
			InkLady.Attack.DelayedTargetedBeam(InkLady.Target.transform, num, InkLady.BeamAttackTime, InkLady.Status.Orientation, forceOrientation: true);
			StartCoroutine(CheckIfDeadBeforeFiring(num * 0.9f));
		}
	}

	private IEnumerator CheckIfDeadBeforeFiring(float warningDelay)
	{
		yield return new WaitForSeconds(warningDelay);
		if (InkLady.Status.Dead)
		{
			InkLady.Attack.Clear();
		}
	}

	public void LookAtTarget()
	{
		InkLady.Behaviour.LookAtTarget(InkLady.Target.transform.position);
		SetBeamPosition();
	}

	public void Dispose()
	{
		OwnerEntity.gameObject.SetActive(value: false);
	}
}
