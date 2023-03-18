using Framework.FrameworkCore;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BossFight;

public class BossFightMetrics : MonoBehaviour
{
	private void Start()
	{
	}

	public void StartBossFight()
	{
		PlayerPrefs.GetInt("BOSS_TRIES", 1);
		int @int = PlayerPrefs.GetInt("BOSS_TRIES");
		Log.Trace("Metrics", "Boss fight Nº" + @int + " started.");
	}

	public void EndBossFight()
	{
		int @int = PlayerPrefs.GetInt("BOSS_TRIES", 1);
		Core.Metrics.CustomEvent("NORMAL_BOSS_KILLED", string.Empty, @int);
		Log.Trace("Metrics", "Boss fight Nº " + @int + " was successful.");
		PlayerPrefs.DeleteKey("BOSS_TRIES");
	}
}
