using System.Collections;

public class DevilLevelHandProjectile : BasicProjectile
{
	protected override void Start()
	{
		base.Start();
		StartCoroutine(animation_cr());
	}

	private IEnumerator animation_cr()
	{
		move = false;
		yield return base.animator.WaitForAnimationToEnd(this, "Projectile");
		move = true;
	}
}
