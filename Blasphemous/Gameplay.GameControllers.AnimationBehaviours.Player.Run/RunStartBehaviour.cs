using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Run;

public class RunStartBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_penitent.IsGrabbingLadder = false;
	}
}
