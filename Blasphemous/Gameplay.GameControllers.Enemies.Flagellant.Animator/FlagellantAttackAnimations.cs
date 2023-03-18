using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Flagellant.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Flagellant.Animator;

public class FlagellantAttackAnimations : AttackAnimationsEvents
{
	private Flagellant _flagellant;

	private FlagellantAttack _flagellantAttack;

	private MotionLerper _flagellantMotionLerper;

	[SerializeField]
	[Range(0.01f, 10f)]
	protected float TimeTakenDuringLerp;

	private void Awake()
	{
		_flagellantAttack = base.transform.parent.GetComponentInChildren<FlagellantAttack>();
		_flagellantMotionLerper = GetComponentInParent<MotionLerper>();
		_flagellant = GetComponentInParent<Flagellant>();
	}

	public void MoveFlagellant(float distance)
	{
		if (!(_flagellantAttack == null) && !_flagellant.Status.IsOnCliffLede && !_flagellant.EnemyBehaviour.IsTrapDetected)
		{
			EntityOrientation orientation = _flagellant.Status.Orientation;
			distance = ((orientation != EntityOrientation.Left) ? distance : (0f - distance));
			_flagellantMotionLerper.distanceToMove = distance;
			_flagellantMotionLerper.TimeTakenDuringLerp = TimeTakenDuringLerp;
			Vector3 forwardTangent = _flagellant.GetForwardTangent(_flagellant.transform.right, _flagellant.EnemyFloorChecker().EnemyFloorCollisionNormal);
			if (!_flagellantMotionLerper.IsLerping)
			{
				_flagellantMotionLerper.StartLerping(forwardTangent);
			}
		}
	}

	public override void CurrentWeaponAttack(DamageArea.DamageType damageType)
	{
		if (!(_flagellantAttack == null))
		{
			_flagellantAttack.CurrentWeaponAttack(damageType);
		}
	}

	public override void WeaponBlowUp(float weaponBlowUp)
	{
		weaponBlowUp = Mathf.Clamp01(weaponBlowUp);
		if (_flagellantAttack != null)
		{
			_flagellantAttack.IsWeaponBlowingUp = weaponBlowUp > 0f;
		}
	}

	public void PlayBasicAttack()
	{
		if (_flagellant.Status.IsVisibleOnCamera)
		{
			_flagellant.Audio.PlayBasicAttack();
		}
	}

	public void PlaySlashHit()
	{
		_flagellant.Audio.PlayAttackHit();
	}

	public void PlaySelfLash()
	{
		if (_flagellant.Status.IsVisibleOnCamera)
		{
			_flagellant.Audio.PlaySelfLash();
		}
	}

	public void PlayDeath()
	{
		_flagellant.Audio.PlayDeath();
	}

	public void PlayVaporizationDeath()
	{
		if (_flagellant.Status.IsVisibleOnCamera)
		{
			_flagellant.Audio.PlayVaporizationDeath();
		}
	}
}
