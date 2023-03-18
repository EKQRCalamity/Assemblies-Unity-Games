using System.Collections;
using Gameplay.GameControllers.Enemies.Processioner.Animator;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Processioner.AI;

public class ShooterProcessionerBehaviour : ProcessionerBehaviour
{
	public float MaxDistanceReached = 5f;

	public float ShootCoolDown = 3f;

	public float _currentShootCoolDown;

	private bool isMovingForward;

	public ShooterProcessioner ShooterProcessioner { get; private set; }

	public ShooterProcessionerAnimator ProcessionerAnimator { get; private set; }

	protected Vector3 OriginPosition { get; private set; }

	protected float DistanceToOrigin { get; private set; }

	public override void OnStart()
	{
		base.OnStart();
		ShooterProcessioner = (ShooterProcessioner)Entity;
		ProcessionerAnimator = (ShooterProcessionerAnimator)ShooterProcessioner.ProcessionerAnimator;
		OriginPosition = new Vector2(base.transform.position.x, base.transform.position.y);
		ShooterProcessioner.ProjectileAttack.SetProjectileWeaponDamage((int)ShooterProcessioner.Stats.Strength.Final);
		isMovingForward = true;
		ResetCoolDown();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (ShooterProcessioner.Status.Dead || ShooterProcessioner.IsAttacking)
		{
			StopMovement();
			return;
		}
		_currentShootCoolDown -= Time.deltaTime;
		if (base.TargetSeen && _currentShootCoolDown <= 0f)
		{
			StartCoroutine(ShootCoroutine(1.5f));
			ResetCoolDown();
		}
		DistanceToOrigin = Vector2.Distance(OriginPosition, base.transform.position);
		if (DistanceToOrigin <= MaxDistanceReached)
		{
			Move(isMovingForward);
			return;
		}
		isMovingForward = !isMovingForward;
		OriginPosition = new Vector2(base.transform.position.x, base.transform.position.y);
	}

	private IEnumerator ShootCoroutine(float delay)
	{
		ProcessionerAnimator.Shoot();
		yield return new WaitForSeconds(delay);
		ProcessionerAnimator.ChargeLoop(charge: false);
	}

	private void Move(bool forward = true)
	{
		float horizontalInput = ((ShooterProcessioner.Status.Orientation != 0) ? ((!forward) ? 1f : (-1f)) : ((!forward) ? (-1f) : 1f));
		ShooterProcessioner.Input.HorizontalInput = horizontalInput;
		ProcessionerAnimator.WalkBackward(!forward);
	}

	public override void StopMovement()
	{
		ShooterProcessioner.Input.HorizontalInput = 0f;
	}

	public void ResetCoolDown()
	{
		if (_currentShootCoolDown < ShootCoolDown)
		{
			_currentShootCoolDown = ShootCoolDown;
		}
	}
}
