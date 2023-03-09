using UnityEngine;

public class OldManLevelLobberProjectile : BasicProjectile
{
	protected override void OnCollision(GameObject hit, CollisionPhase phase)
	{
		base.OnCollision(hit, phase);
		if (!hit.GetComponent<LevelPlatform>())
		{
			return;
		}
		Die();
		AbstractPlayerController[] componentsInChildren = hit.GetComponentsInChildren<AbstractPlayerController>();
		foreach (AbstractPlayerController abstractPlayerController in componentsInChildren)
		{
			if (!(abstractPlayerController == null))
			{
				abstractPlayerController.transform.parent = null;
			}
		}
		hit.SetActive(value: false);
	}

	protected override void Die()
	{
		base.Die();
		GetComponent<SpriteRenderer>().enabled = false;
	}
}
