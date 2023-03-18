using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffGiant.Animator;

public class PontiffGiantAnimationEvents : MonoBehaviour
{
	public PontiffGiantBehaviour behaviour;

	public void AnimationEvent_OnMaskClosed()
	{
		behaviour.OnMaskClosed();
	}

	public void AnimationEvent_OnMaskOpened()
	{
		behaviour.OnMaskOpened();
	}
}
