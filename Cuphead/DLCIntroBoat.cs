using UnityEngine;

public class DLCIntroBoat : AbstractPausableComponent
{
	[SerializeField]
	private Animator boatmanAnimator;

	[SerializeField]
	private MinMax speed;

	private float curSpeed;

	private void FixedUpdate()
	{
		curSpeed = Mathf.Lerp(speed.GetFloatAt(1f - boatmanAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f), speed.GetFloatAt((1.1f - boatmanAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f) % 1f), 0.5f);
		base.transform.position += Vector3.right * curSpeed * CupheadTime.FixedDelta;
	}
}
