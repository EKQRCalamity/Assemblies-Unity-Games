using System.Collections;
using UnityEngine;

public class RumRunnersLevelSnoutTongue : ParrySwitch
{
	private CollisionChild collisionChild;

	public event CollisionChild.OnCollisionHandler OnPlayerCollision;

	private void OnEnable()
	{
		GetComponent<CollisionChild>().OnPlayerCollision += onPlayerCollision;
	}

	private void OnDisable()
	{
		GetComponent<CollisionChild>().OnPlayerCollision -= onPlayerCollision;
	}

	protected override void Awake()
	{
		base.Awake();
		base.gameObject.tag = "Enemy";
		collisionChild = GetComponent<CollisionChild>();
	}

	public override void OnParryPrePause(AbstractPlayerController player)
	{
		if ((bool)parrySpark)
		{
			parrySpark.Create(player.transform.position);
		}
		FirePrePauseEvent();
		player.stats.ParryOneQuarter();
	}

	public override void OnParryPostPause(AbstractPlayerController player)
	{
		base.OnParryPostPause(player);
		base.IsParryable = false;
		collisionChild.enabled = false;
		StartCoroutine(parryCooldown_cr());
	}

	private IEnumerator parryCooldown_cr()
	{
		float t = 0f;
		while (t < coolDown)
		{
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		base.IsParryable = true;
		collisionChild.enabled = true;
	}

	private void onPlayerCollision(GameObject hit, CollisionPhase phase)
	{
		if (base.IsParryable && this.OnPlayerCollision != null)
		{
			this.OnPlayerCollision(hit, phase);
		}
	}
}
