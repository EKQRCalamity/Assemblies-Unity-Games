using UnityEngine;

public class DevilLevelSplitDevilProjectile : BasicProjectile
{
	private DevilLevelSplitDevil devil;

	public DevilLevelSplitDevilProjectile Create(Vector2 position, float rotation, float speed, DevilLevelSplitDevil devil)
	{
		DevilLevelSplitDevilProjectile devilLevelSplitDevilProjectile = base.Create(position, rotation, speed) as DevilLevelSplitDevilProjectile;
		devilLevelSplitDevilProjectile.devil = devil;
		return devilLevelSplitDevilProjectile;
	}

	protected override void Update()
	{
		base.Update();
		if (!base.dead)
		{
			if (devil == null)
			{
				Die();
			}
			else
			{
				UpdateColor();
			}
		}
	}

	private void UpdateColor()
	{
	}

	protected override void Die()
	{
		base.Die();
		Object.Destroy(base.gameObject);
	}
}
