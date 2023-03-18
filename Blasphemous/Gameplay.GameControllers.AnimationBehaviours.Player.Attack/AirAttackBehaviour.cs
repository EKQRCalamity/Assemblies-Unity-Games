using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Attack;

public class AirAttackBehaviour : StateMachineBehaviour
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
		animator.SetBool("AIR_ATTACKING", value: true);
		_penitent.OnAirAttackBehaviour_OnEnter(this);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.normalizedTime >= 0.6f)
		{
			_penitent.EntityAttack.IsWeaponBlowingUp = true;
		}
		else
		{
			_penitent.EntityAttack.IsWeaponBlowingUp = false;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.EntityAttack.IsWeaponBlowingUp = false;
		_penitent.CurrentOutputDamage = 0;
		animator.SetBool("AIR_ATTACKING", value: false);
	}
}
