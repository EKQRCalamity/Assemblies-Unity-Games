using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Menu;

public class MapMenuBehaviour : StateMachineBehaviour
{
	private bool _inputAlreadyBlocked;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		_inputAlreadyBlocked = Core.Input.HasBlocker("PLAYER_LOGIC");
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Core.Input.SetBlocker("PLAYER_LOGIC", _inputAlreadyBlocked);
	}
}
