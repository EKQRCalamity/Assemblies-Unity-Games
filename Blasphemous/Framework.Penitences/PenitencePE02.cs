using Framework.FrameworkCore;
using Framework.Managers;

namespace Framework.Penitences;

public class PenitencePE02 : IPenitence
{
	private const string id = "PE02";

	public const float PenitenceRegenFactor = 1f;

	public string Id => "PE02";

	public bool Completed { get; set; }

	public bool Abandoned { get; set; }

	public void Activate()
	{
		Core.PenitenceManager.UseStocksOfHealth = true;
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		Core.PenitenceManager.AddFlasksPassiveHealthRegen(1f);
	}

	public void Deactivate()
	{
		Core.PenitenceManager.UseStocksOfHealth = false;
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		Core.PenitenceManager.AddFlasksPassiveHealthRegen(-1f);
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		Core.Logic.EnemySpawner.RespawnDeadEnemies();
	}
}
