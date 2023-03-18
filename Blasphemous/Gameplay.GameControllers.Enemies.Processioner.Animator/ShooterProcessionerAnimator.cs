using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Processioner.AI;
using Gameplay.GameControllers.Enemies.Processioner.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Processioner.Animator;

public class ShooterProcessionerAnimator : ProcessionerAnimator
{
	protected ShooterProcessioner Processioner { get; set; }

	protected ProcesionerAudio Audio { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		Processioner = (ShooterProcessioner)OwnerEntity;
		Audio = Processioner.GetComponentInChildren<ProcesionerAudio>();
		SetProjectileShootPosition();
	}

	public void WalkBackward(bool backward)
	{
		base.EntityAnimator.SetBool("BACK", backward);
	}

	public void Shoot()
	{
		base.EntityAnimator.SetTrigger("SHOOT");
		ChargeLoop();
	}

	public void ChargeLoop(bool charge = true)
	{
		Audio.StartChargeLoop();
		base.EntityAnimator.SetBool("CHARGING", charge);
	}

	public void SetAttacking(int attacking)
	{
		Processioner.IsAttacking = attacking > 0;
	}

	public void Death()
	{
		base.EntityAnimator.ResetTrigger("SHOOT");
		base.EntityAnimator.SetTrigger("DEATH");
	}

	public void Dispose()
	{
		Audio.StopChargeLoop();
		Processioner.gameObject.SetActive(value: false);
	}

	public void LaunchProjectile()
	{
		if (Processioner.Target == null)
		{
			return;
		}
		Transform transform = Processioner.Target.transform;
		if (!Processioner.Status.Dead)
		{
			Vector3 normalized = (transform.position - Processioner.transform.position).normalized;
			Processioner.ProjectileAttack.Shoot(normalized);
			Audio.StopChargeLoop();
			ShooterProcessionerBehaviour shooterProcessionerBehaviour = Processioner.Behaviour as ShooterProcessionerBehaviour;
			if ((bool)shooterProcessionerBehaviour)
			{
				shooterProcessionerBehaviour.ResetCoolDown();
			}
		}
	}

	private void SetProjectileShootPosition()
	{
		if (Processioner.Status.Orientation == EntityOrientation.Left)
		{
			Vector3 localScale = Processioner.ProjectileAttack.gameObject.transform.localScale;
			localScale.x *= -1f;
			Processioner.ProjectileAttack.gameObject.transform.localScale = localScale;
		}
	}

	private void OnDestroy()
	{
		if ((bool)Audio)
		{
			Audio.StopChargeLoop();
		}
	}
}
