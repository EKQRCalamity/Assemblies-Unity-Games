using Gameplay.GameControllers.Enemies.BellCarrier;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.BellCarrier;

public class BellCarrierWallCrushBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.BellCarrier.BellCarrier _bellCarrier;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_bellCarrier == null)
		{
			_bellCarrier = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.BellCarrier.BellCarrier>();
		}
		float num = ((_bellCarrier.Status.Orientation != 0) ? 1f : (-1f));
		_bellCarrier.MotionLerper.StartLerping(Vector2.right * num);
		_bellCarrier.BellCarrierBehaviour.WallHit = true;
		animator.ResetTrigger("WALL_CRUSH");
	}
}
