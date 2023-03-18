using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Jumper;
using Gameplay.GameControllers.Enemies.Jumper.AI;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Jumper;

public class JumperReadyAnimationBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.Jumper.Jumper Jumper { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Jumper == null)
		{
			Jumper = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Jumper.Jumper>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		JumperBehaviour jumperBehaviour = (JumperBehaviour)Jumper.EnemyBehaviour;
		float horizontalInput = ((!(Jumper.Target.transform.position.x > Jumper.transform.position.x)) ? (-1f) : 1f);
		Jumper.Inputs.HorizontalInput = horizontalInput;
		Jumper.SetOrientation(GetJumpOrientation());
		jumperBehaviour.Jump();
	}

	private EntityOrientation GetJumpOrientation()
	{
		float horizontalInput = Jumper.Inputs.HorizontalInput;
		return (horizontalInput <= 0f) ? EntityOrientation.Left : EntityOrientation.Right;
	}
}
