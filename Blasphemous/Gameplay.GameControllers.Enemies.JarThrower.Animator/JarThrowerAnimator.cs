using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.JarThrower.Animator;

public class JarThrowerAnimator : EnemyAnimatorInyector
{
	public JarThrower JarThrower { get; private set; }

	protected override void OnStart()
	{
		base.OnStart();
		JarThrower = (JarThrower)OwnerEntity;
		if ((bool)JarThrower)
		{
			JarThrower.JumpAttack.OnJumpLanded += OnJumpLanded;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		JarThrower.Animator.SetBool("GROUNDED", JarThrower.Status.IsGrounded);
		if (JarThrower.IsAttacking && JarThrower.Controller.PlatformCharacterPhysics.VSpeed < -0.1f && !JarThrower.Status.IsGrounded)
		{
			JarThrower.Animator.SetBool("REACH_MAX_HEIGHT", value: true);
		}
	}

	public void Walk(bool walk = true)
	{
		JarThrower.Animator.SetBool("WALK", walk);
		JarThrower.Animator.ResetTrigger("JUMP");
		if (walk)
		{
			Run(run: false);
		}
	}

	public void Run(bool run = true)
	{
		JarThrower.Animator.SetBool("RUN", run);
		JarThrower.Animator.ResetTrigger("JUMP");
		if (run)
		{
			Walk(walk: false);
		}
	}

	public void Death()
	{
		JarThrower.Animator.SetBool("DEATH", value: true);
		if ((bool)JarThrower)
		{
			JarThrower.JumpAttack.OnJumpLanded -= OnJumpLanded;
		}
	}

	public void Healing()
	{
		JarThrower.Animator.SetTrigger("DRINK");
	}

	public void Jump()
	{
		if (JarThrower.IsAttacking)
		{
			JumpAttack();
		}
		else
		{
			JumpChase();
		}
	}

	private void JumpChase()
	{
		JarThrower.JumpAttack.OnJumpAdvancedEvent += JumpAttackOnOnJumpAdvancedEvent;
		JarThrower.Animator.SetTrigger("JUMP");
		JarThrower.Animator.SetBool("REACH_MAX_HEIGHT", value: false);
		JarThrower.Behaviour.LookAtTarget(JarThrower.Target.transform.position);
	}

	private void OnJumpLanded()
	{
		JarThrower.Animator.SetBool("ATTACKING", value: false);
		JarThrower.Animator.SetBool("REACH_MAX_HEIGHT", value: false);
		if (!JarThrower.Animator.GetCurrentAnimatorStateInfo(0).IsName("Landing"))
		{
			JarThrower.Animator.Play("Landing");
		}
	}

	private void JumpAttack()
	{
		JarThrower.Animator.SetTrigger("JUMP");
		JarThrower.Animator.SetBool("REACH_MAX_HEIGHT", value: false);
		JarThrower.Animator.SetBool("ATTACKING", value: true);
		JarThrower.Behaviour.LookAtTarget(JarThrower.Target.transform.position);
	}

	public void ResetJumpAttack()
	{
		JarThrower.Animator.SetBool("ATTACKING", value: false);
		JarThrower.IsAttacking = false;
	}

	public void ThrowJar()
	{
		JarThrower.IsFalling = true;
		Vector3 targetPosition = GetTargetPosition(JarThrower.Target.transform.position);
		Vector3 direction = targetPosition - JarThrower.JarAttack.projectileSource.position;
		JarThrower.JarAttack.SetProjectileWeaponDamage((int)JarThrower.Stats.Strength.Final);
		StraightProjectile straightProjectile = JarThrower.JarAttack.Shoot(targetPosition);
		straightProjectile.Init(direction, JarThrower.Behaviour.JarProjectileSpeed);
	}

	private Vector3 GetTargetPosition(Vector3 targetPosition)
	{
		Vector3 result = targetPosition;
		Vector3 position = JarThrower.JarAttack.projectileSource.position;
		if (JarThrower.Status.Orientation == EntityOrientation.Left)
		{
			if (result.x > position.x)
			{
				result.x = position.x;
			}
		}
		else if (result.x < position.x)
		{
			result.x = position.x;
		}
		return result;
	}

	public void UseJumpCurve()
	{
		if (JarThrower.IsAttacking)
		{
			StartCoroutine(JarThrower.Behaviour.JumpAttackCoroutine());
			return;
		}
		JarThrower.Behaviour.LookAtTarget(JarThrower.Target.transform.position);
		JarThrower.JumpAttack.Use(JarThrower.transform, JarThrower.Target.transform.position);
	}

	private void JumpAttackOnOnJumpAdvancedEvent(Vector2 obj)
	{
		if (obj.y < 0.1f)
		{
			JarThrower.Animator.SetBool("REACH_MAX_HEIGHT", value: true);
			JarThrower.JumpAttack.OnJumpAdvancedEvent -= JumpAttackOnOnJumpAdvancedEvent;
		}
	}

	public void DisableEntity()
	{
		JarThrower.gameObject.SetActive(value: false);
		JarThrower.Behaviour.enabled = false;
	}
}
