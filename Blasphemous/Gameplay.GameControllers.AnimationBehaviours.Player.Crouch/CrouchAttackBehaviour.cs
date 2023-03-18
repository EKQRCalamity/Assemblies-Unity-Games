using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Crouch;

public class CrouchAttackBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	[Tooltip("The damage amount done by the penitent in this attack")]
	public int damageAmount;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_penitent.CurrentOutputDamage = damageAmount;
		_penitent.AnimatorInyector.RaiseAttackEvent();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = ((!(_penitent.PenitentAttack.AttackSpeed > 1f)) ? 1f : _penitent.PenitentAttack.AttackSpeed);
		if (!_penitent.IsCrouchAttacking)
		{
			_penitent.IsCrouchAttacking = true;
		}
		if (!_penitent.PlatformCharacterInput.IsAttacking)
		{
			_penitent.PlatformCharacterInput.IsAttacking = true;
		}
		if (stateInfo.normalizedTime >= 0.35f && stateInfo.normalizedTime <= 0.7f)
		{
			if (!_penitent.EntityAttack.IsWeaponBlowingUp)
			{
				_penitent.EntityAttack.IsWeaponBlowingUp = true;
			}
		}
		else if (_penitent.EntityAttack.IsWeaponBlowingUp)
		{
			_penitent.EntityAttack.IsWeaponBlowingUp = false;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent.IsCrouchAttacking)
		{
			_penitent.IsCrouchAttacking = !_penitent.IsCrouchAttacking;
		}
		if (_penitent.PlatformCharacterInput.IsAttacking)
		{
			_penitent.PlatformCharacterInput.IsAttacking = false;
		}
		animator.speed = 1f;
		_penitent.EntityAttack.IsWeaponBlowingUp = false;
	}
}
