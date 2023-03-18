using Framework.Managers;
using Gameplay.GameControllers.Penitent.Spawn;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Effects.Player;

public class CherubRespawnAnimationBehaviour : StateMachineBehaviour
{
	private CherubRespawn _cherubRespawn;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_cherubRespawn == null)
		{
			_cherubRespawn = animator.GetComponent<CherubRespawn>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_cherubRespawn.Dispose();
	}
}
