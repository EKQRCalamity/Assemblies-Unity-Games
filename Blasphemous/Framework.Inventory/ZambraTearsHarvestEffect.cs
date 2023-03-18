using System.Collections;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Prayers;
using UnityEngine;

namespace Framework.Inventory;

public class ZambraTearsHarvestEffect : ObjectEffect
{
	public float Duration = 2f;

	private ZambraTearsHarvestPrayer TearsHarvestPrayer { get; set; }

	private bool IsRunningEffect { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Penitent penitent)
	{
		TearsHarvestPrayer = penitent.GetComponentInChildren<ZambraTearsHarvestPrayer>();
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}

	protected override bool OnApplyEffect()
	{
		if (!IsRunningEffect && (bool)TearsHarvestPrayer)
		{
			Debug.Log("ZAMBRA Effect Applied");
			IsRunningEffect = true;
			TearsHarvestPrayer.EnableEffect();
			StartCoroutine(DisableCoroutine(Duration));
		}
		return true;
	}

	private IEnumerator DisableCoroutine(float delay)
	{
		yield return new WaitForSeconds(delay);
		IsRunningEffect = false;
		if ((bool)TearsHarvestPrayer)
		{
			TearsHarvestPrayer.DisableEffect();
		}
		Debug.Log("ZAMBRA Effect disable");
	}

	protected override void OnRemoveEffect()
	{
		base.OnRemoveEffect();
	}
}
