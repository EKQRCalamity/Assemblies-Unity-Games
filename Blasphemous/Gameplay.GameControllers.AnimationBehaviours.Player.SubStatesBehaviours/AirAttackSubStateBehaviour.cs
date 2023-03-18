using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.SubStatesBehaviours;

public class AirAttackSubStateBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_penitent.GrabLadder.EnableClimbLadderAbility(enable: false);
		_penitent.AnimatorInyector.RaiseAttackEvent();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.GrabLadder.EnableClimbLadderAbility();
		_penitent.PenitentAttackAnimations.CloseAttackWindow();
		_penitent.PenitentAttack.ClearHitEntityList();
	}
}
