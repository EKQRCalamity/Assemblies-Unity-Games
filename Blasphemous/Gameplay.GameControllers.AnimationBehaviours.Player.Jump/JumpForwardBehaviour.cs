using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Jump;

public class JumpForwardBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_penitent.DamageArea.IsFallingForwardResized = false;
	}
}
