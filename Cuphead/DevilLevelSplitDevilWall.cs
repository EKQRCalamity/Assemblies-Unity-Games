using UnityEngine;

public class DevilLevelSplitDevilWall : AbstractProjectile
{
	private float xVelocity;

	private DevilLevelSplitDevil devil;

	private const float Y_POS = 30f;

	public DevilLevelSplitDevilWall Create(float xPos, float xVelocity, float distance, DevilLevelSplitDevil devil)
	{
		DevilLevelSplitDevilWall devilLevelSplitDevilWall = base.Create(new Vector2(xPos, 30f)) as DevilLevelSplitDevilWall;
		devilLevelSplitDevilWall.xVelocity = xVelocity;
		devilLevelSplitDevilWall.DestroyDistance = distance;
		devilLevelSplitDevilWall.devil = devil;
		devilLevelSplitDevilWall.UpdateColor();
		CupheadLevelCamera.Current.StartShake(4f);
		return devilLevelSplitDevilWall;
	}

	protected override void Update()
	{
		base.Update();
		if (!base.dead)
		{
			if (devil == null)
			{
				Die();
				return;
			}
			base.transform.AddPosition(xVelocity * (float)CupheadTime.Delta);
			base.transform.SetScale(Random.Range(0.9f, 1f));
			UpdateColor();
		}
	}

	private void UpdateColor()
	{
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected override void Die()
	{
		base.Die();
		Object.Destroy(base.gameObject);
	}

	protected override void OnDestroy()
	{
		if (Object.FindObjectsOfType<DevilLevelSplitDevilWall>().Length <= 1)
		{
			CupheadLevelCamera.Current.EndShake(0.5f);
		}
		base.OnDestroy();
	}
}
