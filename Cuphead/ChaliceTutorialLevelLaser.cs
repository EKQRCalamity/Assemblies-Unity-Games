using UnityEngine;

public class ChaliceTutorialLevelLaser : AbstractCollidableObject
{
	[SerializeField]
	private ChaliceTutorialLevel level;

	[SerializeField]
	private ChaliceTutorialLevelParryable parryable;

	[SerializeField]
	private Collider2D coll;

	[SerializeField]
	private Animator hitAnimator;

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		level.resetParryables = true;
		AudioManager.Play("sfx_rip_fail");
		hitAnimator.transform.position = new Vector3(base.transform.position.x + coll.bounds.size.x / 2f, hit.transform.position.y + 100f);
		hitAnimator.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0, 360));
		hitAnimator.Play("Hit");
	}

	private void Enabled()
	{
		base.animator.SetBool("On", value: true);
		coll.enabled = true;
	}

	private void Disabled()
	{
		base.animator.SetBool("On", value: false);
		coll.enabled = false;
	}

	private void Update()
	{
		if (!parryable.isDeactivated)
		{
			Enabled();
		}
		else
		{
			Disabled();
		}
	}

	private void AniEvent_SFX_Open()
	{
		AudioManager.Play("sfx_rip_open");
	}
}
