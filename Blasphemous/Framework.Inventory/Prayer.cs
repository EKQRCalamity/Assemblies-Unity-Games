using Framework.Managers;
using UnityEngine;

namespace Framework.Inventory;

public class Prayer : EquipableInventoryObject
{
	public static class Id
	{
		public const string DistressingSaeta = "PR04";
	}

	public enum PrayerType
	{
		Laments,
		Thanksgiving,
		Hymn
	}

	public int decipherMax;

	public int fervourNeeded = 20;

	public PrayerType prayerType;

	public int CurrentDecipher { get; private set; }

	public float EffectTime { get; private set; }

	public Prayer()
	{
		ResetDecipher();
	}

	public override InventoryManager.ItemType GetItemType()
	{
		return InventoryManager.ItemType.Prayer;
	}

	public override bool IsDeciphered()
	{
		return true;
	}

	public override bool IsDecipherable()
	{
		return true;
	}

	public void Awake()
	{
		ObjectEffect[] components = GetComponents<ObjectEffect>();
		foreach (ObjectEffect objectEffect in components)
		{
			if (objectEffect.effectType == ObjectEffect.EffectType.OnUse && objectEffect.LimitTime && objectEffect.EffectTime > EffectTime)
			{
				EffectTime = objectEffect.EffectTime;
			}
		}
		Debug.Log("Prayer " + id + "  Time:" + EffectTime);
	}

	public void ResetDecipher()
	{
		CurrentDecipher = 0;
	}

	public void AddDecipher(int number = 1)
	{
		Core.Metrics.CustomEvent("DECIPHER_FERVOUR", string.Empty, number);
		CurrentDecipher += number;
		if (CurrentDecipher > decipherMax)
		{
			CurrentDecipher = decipherMax;
		}
	}
}
