using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Animations;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Acolyte.Animator;

public class AcolyteAttackAnimations : AttackAnimationsEvents
{
	private Acolyte _acolyte;

	private AcolyteAttack _acolyteAttack;

	private MotionLerper _acolyteMotionLerper;

	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	[Tooltip("The displacement distance when acolyte attacks")]
	public float attackDisplacement;

	[Tooltip("The speed of the displacement (lerping time)")]
	public float displacementSpeed;

	public ColorFlash ColorFlash { get; private set; }

	public bool ReboundAttack { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		_acolyte = GetComponentInParent<Acolyte>();
		if (_acolyte != null)
		{
			_acolyteMotionLerper = _acolyte.MotionLerper;
		}
		ReboundAttack = false;
		SpawnManager.OnPlayerSpawn += OnPenitentReady;
		if (_acolyte != null)
		{
			_acolyteAttack = _acolyte.GetComponentInChildren<AcolyteAttack>();
		}
		ColorFlash = GetComponentInChildren<ColorFlash>();
	}

	private void OnDestroy()
	{
		SpawnManager.OnPlayerSpawn -= OnPenitentReady;
	}

	private void OnPenitentReady(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		_penitent = penitent;
	}

	public override void WeaponBlowUp(float weaponBlowUp)
	{
	}

	public void GetCurrentPlayerPosition()
	{
	}

	public void EnableAttackFlag()
	{
		_acolyte.IsAttacking = true;
		_acolyte.Behaviour.IsAttackWindowOpen = true;
	}

	public void DisableAttackFlag()
	{
		_acolyte.IsAttacking = false;
		_acolyte.Behaviour.IsAttackWindowOpen = false;
	}

	public override void CurrentWeaponAttack(DamageArea.DamageType damageType)
	{
		if (!(_acolyteAttack == null))
		{
			_acolyteAttack.CurrentWeaponAttack(damageType);
		}
	}

	public void StopAttackDisplacement()
	{
		if (_acolyte.RigidbodyType == RigidbodyType2D.Dynamic)
		{
			_acolyte.Rigidbody.velocity = Vector2.zero;
			_acolyte.RigidbodyType = RigidbodyType2D.Kinematic;
		}
	}

	public void AttackDisplacement()
	{
		if (!(_acolyteMotionLerper == null) && !ReboundAttack && !_acolyteMotionLerper.IsLerping)
		{
			ReboundAttack = false;
			float num = ((_acolyte.Status.Orientation != 0) ? (-1f) : 1f);
			_acolyteMotionLerper.StartLerping(_acolyte.transform.right * (num * attackDisplacement));
		}
	}

	private bool AttackAreaOverlapsPlayer(EntityOrientation playerOrientation)
	{
		Vector2 point = _penitent.DamageArea.DamageAreaCollider.bounds.center;
		return _acolyte.AttackArea.WeaponCollider.OverlapPoint(point);
	}

	public override void Rebound()
	{
		if (AttackAreaOverlapsPlayer(_penitent.Status.Orientation) && _penitent.HasFlag("SIDE_BLOCKED") && _acolyte.Status.Orientation != _penitent.Status.Orientation && !ReboundAttack && _acolyte.IsAttacking)
		{
			_acolyteMotionLerper.StopLerping();
			_acolyteMotionLerper.distanceToMove *= -1f;
			Vector3 forwardTangent = _acolyte.GetForwardTangent(_acolyte.transform.right, _acolyte.EnemyFloorChecker().EnemyFloorCollisionNormal);
			ReboundAttack = true;
			_acolyteMotionLerper.StartLerping(forwardTangent);
		}
	}

	public void PlayChargeAttack()
	{
		if (_acolyte.IsVisible())
		{
			_acolyte.Audio.PlayChargeAttack();
		}
	}

	public void PlayReleaseChargeAttack()
	{
		if (_acolyte.IsVisible())
		{
			_acolyte.Audio.PlayReleaseAttack();
		}
	}

	public void PlayOverhrow()
	{
		if (_acolyte.IsVisible())
		{
			_acolyte.Audio.PlayOverthrow();
		}
	}

	public void PlayDeathOnCliffLede()
	{
		if (_acolyte.IsVisible())
		{
			_acolyte.Audio.PlayDeathOnCliffLede();
		}
	}

	public void PlayDeath()
	{
		if (_acolyte.IsVisible())
		{
			_acolyte.Audio.PlayDeath();
		}
	}

	public void PlayVaporizationDeath()
	{
		if (_acolyte.IsVisible())
		{
			_acolyte.Audio.PlayVaporizationDeath();
		}
	}
}
