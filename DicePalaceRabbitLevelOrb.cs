using UnityEngine;

public class DicePalaceRabbitLevelOrb : AbstractProjectile
{
	protected override void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
		base.Update();
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	public void SetAsGold(bool isGold)
	{
		if (isGold)
		{
			base.animator.SetTrigger("Gold");
		}
		else
		{
			base.animator.SetTrigger("Blue");
		}
	}
}
