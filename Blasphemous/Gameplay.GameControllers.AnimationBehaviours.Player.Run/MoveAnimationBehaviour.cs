using Framework.Managers;
using Gameplay.GameControllers.Effects.Player.Dust;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Run;

public class MoveAnimationBehaviour : StateMachineBehaviour
{
	public StepDust.StepDustType stepDustType;

	private StepDustSpawner _stepDustSpawner;

	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = 1f;
		animator.SetBool("IS_CLIMBING_LADDER", value: false);
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_penitent.IsGrabbingLadder = false;
		if (_stepDustSpawner == null)
		{
			_stepDustSpawner = _penitent.StepDustSpawner;
		}
		_stepDustSpawner.CurrentStepDustSpawn = stepDustType;
	}
}
