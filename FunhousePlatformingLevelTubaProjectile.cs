public class FunhousePlatformingLevelTubaProjectile : BasicProjectile
{
	protected override bool DestroyedAfterLeavingScreen => true;

	protected override float DestroyLifetime => 0f;

	protected override void Awake()
	{
		base.Awake();
		DestroyDistance = 0f;
	}
}
