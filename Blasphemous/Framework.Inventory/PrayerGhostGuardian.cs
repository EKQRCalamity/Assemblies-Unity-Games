using Framework.Managers;
using Gameplay.GameControllers.Entities.Guardian;
using UnityEngine;

namespace Framework.Inventory;

public class PrayerGhostGuardian : ObjectEffect
{
	public GameObject GhostGuardianPrefab;

	private GameObject GhostGuardianInstantiation;

	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
	}

	protected override bool OnApplyEffect()
	{
		InstantiateGuardian();
		return true;
	}

	protected override void OnRemoveEffect()
	{
		base.OnRemoveEffect();
		DisposeGuardian();
	}

	public void InstantiateGuardian()
	{
		if (GhostGuardianPrefab != null && GhostGuardianInstantiation == null)
		{
			Vector3 position = Core.Logic.Penitent.transform.position;
			GhostGuardianInstantiation = Object.Instantiate(GhostGuardianPrefab, position, Quaternion.identity);
		}
		else if (GhostGuardianInstantiation != null)
		{
			GhostGuardianInstantiation.SetActive(value: true);
			GhostGuardianInstantiation.transform.position = Core.Logic.Penitent.transform.position;
		}
	}

	public void DisposeGuardian()
	{
		if (!(GhostGuardianInstantiation == null))
		{
			GuardianPrayer componentInChildren = GhostGuardianInstantiation.GetComponentInChildren<GuardianPrayer>();
			componentInChildren.Behaviour.VanishFlag = true;
		}
	}
}
