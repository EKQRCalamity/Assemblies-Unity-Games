using System;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BejeweledSaint;

public class BsHolderManager : MonoBehaviour
{
	public Core.SimpleEvent OnBossCollapse;

	public int CollapsedHoldersAmount;

	public int holdersToFall = 3;

	public BejeweledSaintHolder[] Holders;

	private void Start()
	{
		BejeweledSaintHolder.OnHolderCollapse = (Core.SimpleEvent)Delegate.Combine(BejeweledSaintHolder.OnHolderCollapse, new Core.SimpleEvent(OnHolderCollapse));
	}

	private void OnHolderCollapse()
	{
		CollapsedHoldersAmount++;
		if (CollapsedHoldersAmount >= holdersToFall)
		{
			CollapsedHoldersAmount = 0;
			EnableHoldersDamageArea(enableDamageArea: false);
			if (OnBossCollapse != null)
			{
				OnBossCollapse();
			}
		}
	}

	public void SortDamageable()
	{
	}

	public void SortRealHolder()
	{
	}

	public void SetDefaultLocalPositions()
	{
		BejeweledSaintHolder[] holders = Holders;
		foreach (BejeweledSaintHolder bejeweledSaintHolder in holders)
		{
			bejeweledSaintHolder.SetDefaultLocalPosition();
		}
	}

	public void HealHolders()
	{
		BejeweledSaintHolder[] holders = Holders;
		foreach (BejeweledSaintHolder bejeweledSaintHolder in holders)
		{
			bejeweledSaintHolder.Heal();
		}
	}

	public void EnableHoldersDamageArea(bool enableDamageArea)
	{
		BejeweledSaintHolder[] holders = Holders;
		foreach (BejeweledSaintHolder bejeweledSaintHolder in holders)
		{
			bejeweledSaintHolder.EnableDamageArea(enableDamageArea);
		}
	}

	private void OnDestroy()
	{
		BejeweledSaintHolder.OnHolderCollapse = (Core.SimpleEvent)Delegate.Remove(BejeweledSaintHolder.OnHolderCollapse, new Core.SimpleEvent(OnHolderCollapse));
	}
}
