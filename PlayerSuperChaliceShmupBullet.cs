public class PlayerSuperChaliceShmupBullet : BasicProjectile
{
	public float lifetimeMax = 20f;

	protected override float DestroyLifetime => lifetimeMax;
}
