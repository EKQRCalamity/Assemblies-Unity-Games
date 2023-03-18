using Framework.Managers;
using Gameplay.GameControllers.Effects.Player.Sparks;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Effects.Player;

public class SparkBehaviour : StateMachineBehaviour
{
	private SwordSpark _swordSpark;

	private SwordSparkSpawner _swordSparkSpawner;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_swordSparkSpawner == null)
		{
			_swordSparkSpawner = Core.Logic.Penitent.SwordSparkSpawner;
		}
		if (_swordSpark == null)
		{
			_swordSpark = animator.GetComponent<SwordSpark>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_swordSparkSpawner == null)
		{
			_swordSparkSpawner = Core.Logic.Penitent.SwordSparkSpawner;
		}
		if (_swordSpark != null)
		{
			_swordSparkSpawner.StoreSwordSpark(_swordSpark);
		}
	}
}
