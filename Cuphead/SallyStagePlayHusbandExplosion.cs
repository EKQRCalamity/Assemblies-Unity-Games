public class SallyStagePlayHusbandExplosion : LevelBossDeathExploder
{
	protected override void Start()
	{
		effectPrefab = Level.Current.LevelResources.levelBossDeathExplosion;
	}
}
