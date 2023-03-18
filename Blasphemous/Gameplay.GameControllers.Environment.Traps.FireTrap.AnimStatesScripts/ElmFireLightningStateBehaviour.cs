using UnityEngine;

namespace Gameplay.GameControllers.Environment.Traps.FireTrap.AnimStatesScripts;

public class ElmFireLightningStateBehaviour : StateMachineBehaviour
{
	private TileableBeamLauncher _beamLauncher;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (!_beamLauncher)
		{
			_beamLauncher = animator.GetComponentInParent<TileableBeamLauncher>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if ((bool)_beamLauncher)
		{
			_beamLauncher.gameObject.SetActive(value: false);
		}
	}
}
