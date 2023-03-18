using Gameplay.GameControllers.Enemies.PontiffHusk;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.PontiffHusk;

public class PontiffHuskDashingBehaviour : StateMachineBehaviour
{
	public float MaxDashTime = 2f;

	private float time;

	private PontiffHuskMelee _pontiffHusk;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_pontiffHusk == null)
		{
			_pontiffHusk = animator.GetComponentInParent<PontiffHuskMelee>();
		}
		time = 0f;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		time += Time.deltaTime;
		if (time > MaxDashTime)
		{
			_pontiffHusk.Behaviour.AnimatorInyector.StopAttack();
			_pontiffHusk.Behaviour.Disappear(1f);
			time = 0f;
		}
	}
}
