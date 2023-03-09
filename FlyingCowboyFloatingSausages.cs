using UnityEngine;

public class FlyingCowboyFloatingSausages : Effect
{
	private const float OFFSET = 100f;

	private const float SPEED = 200f;

	public void SetAnimation(string name)
	{
		base.animator.Play(name);
	}

	private void FixedUpdate()
	{
		base.transform.position += Vector3.up * 200f * CupheadTime.FixedDelta;
		if (base.transform.position.y > 460f)
		{
			OnEffectComplete();
		}
	}
}
