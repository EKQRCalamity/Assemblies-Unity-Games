using UnityEngine;

public class FlyingCowboyLevelBirdShrapnel : BasicProjectile
{
	[SerializeField]
	private SpriteRenderer trailARenderer;

	[SerializeField]
	private SpriteRenderer trailBRenderer;

	public override AbstractProjectile Create()
	{
		AbstractProjectile abstractProjectile = base.Create();
		abstractProjectile.animator.Update(0f);
		abstractProjectile.animator.Play(0, 0, Random.Range(0f, 1f));
		abstractProjectile.animator.Update(0f);
		abstractProjectile.animator.RoundFrame();
		abstractProjectile.GetComponent<SpriteRenderer>().flipY = Rand.Bool();
		return abstractProjectile;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		AudioManager.Play("sfx_dlc_cowgirl_p1_dynamitehitplayer");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p1_dynamitehitplayer");
	}

	private void animationEvent_LoopMiddleReached()
	{
		trailBRenderer.enabled = true;
		trailBRenderer.flipY = Rand.Bool();
	}

	private void animationEvent_LoopEndReached()
	{
		trailARenderer.flipY = Rand.Bool();
	}
}
