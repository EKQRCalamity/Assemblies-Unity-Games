using UnityEngine;

public class SnowCultLevelSplitShotBulletShattered : BasicProjectile
{
	[SerializeField]
	private Effect trailFX;

	[SerializeField]
	private float fxDelay = 0.3f;

	private float fxTimer;

	public override BasicProjectile Create(Vector2 position, float rotation, float speed)
	{
		SnowCultLevelSplitShotBulletShattered snowCultLevelSplitShotBulletShattered = base.Create(position, rotation, speed) as SnowCultLevelSplitShotBulletShattered;
		snowCultLevelSplitShotBulletShattered.animator.Play((!Rand.Bool()) ? "MoonB" : "MoonA");
		snowCultLevelSplitShotBulletShattered.fxTimer = 0f;
		return snowCultLevelSplitShotBulletShattered;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		fxTimer += CupheadTime.FixedDelta;
		if (fxTimer > fxDelay)
		{
			fxTimer -= fxDelay;
			trailFX.Create(base.transform.position);
		}
	}
}
