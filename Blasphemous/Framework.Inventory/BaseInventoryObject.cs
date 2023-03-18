using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Tools.Items;
using Tools.Level.Actionables;
using UnityEngine;

namespace Framework.Inventory;

public abstract class BaseInventoryObject : MonoBehaviour, ILocalizable
{
	public string id = string.Empty;

	public string caption = string.Empty;

	[TextArea(3, 10)]
	public string description = string.Empty;

	[TextArea(6, 10)]
	public string lore = string.Empty;

	public Sprite picture;

	public bool carryonstart;

	public bool preserveInNewGamePlus;

	private const int TOTAL_NUMBER_OF_PRAYERS_FOR_AC16 = 13;

	private const int TOTAL_NUMBER_OF_RELIC_FOR_AC17 = 7;

	private const int TOTAL_NUMBER_OF_BEADS_FOR_AC18 = 30;

	public const int TOTAL_NUMBER_OF_COLLECTIBLE_FOR_AC19 = 44;

	private const int TOTAL_NUMBER_OF_SWORD_FOR_AC20 = 9;

	public bool IsOwned { get; private set; }

	public virtual bool IsEquipable()
	{
		return false;
	}

	public virtual bool AskForPercentageCompletition()
	{
		return false;
	}

	public virtual bool AddPercentageCompletition()
	{
		return false;
	}

	public virtual bool IsDecipherable()
	{
		return false;
	}

	public virtual bool IsDeciphered()
	{
		return true;
	}

	public virtual bool HasLore()
	{
		return true;
	}

	public abstract InventoryManager.ItemType GetItemType();

	public string GetInternalId()
	{
		return GetItemType().ToString() + "__" + id;
	}

	public void Add()
	{
		IsOwned = true;
		SendMessage("OnAddInventoryObject", SendMessageOptions.DontRequireReceiver);
		string text = id + "_OWNED";
		if (AddPercentageCompletition() && !Core.Events.GetFlag(text))
		{
			Core.Events.SetFlag(text, b: true, preserveInNewGamePlus);
			switch (GetItemType())
			{
			case InventoryManager.ItemType.Relic:
				Core.AchievementsManager.Achievements["AC17"].AddProgressSafeTo99(14.285714f);
				break;
			case InventoryManager.ItemType.Prayer:
				Core.AchievementsManager.Achievements["AC16"].AddProgressSafeTo99(7.6923075f);
				break;
			case InventoryManager.ItemType.Bead:
				Core.AchievementsManager.Achievements["AC18"].AddProgressSafeTo99(3.3333333f);
				break;
			case InventoryManager.ItemType.Collectible:
				Core.AchievementsManager.Achievements["AC19"].AddProgressSafeTo99(2.2727273f);
				Core.AchievementsManager.CheckFlagsToGrantAC19();
				break;
			case InventoryManager.ItemType.Sword:
				Core.AchievementsManager.Achievements["AC20"].AddProgressSafeTo99(11.111111f);
				break;
			case InventoryManager.ItemType.Quest:
				break;
			}
		}
	}

	public void Remove()
	{
		IsOwned = false;
		SendMessage("OnRemoveInventoryObject", SendMessageOptions.DontRequireReceiver);
	}

	public void Reset()
	{
		IsOwned = false;
		SendMessage("OnResetInventoryObject", SendMessageOptions.DontRequireReceiver);
	}

	public void HitEnemy(Hit hit)
	{
		SendMessage("OnHitEnemy", hit, SendMessageOptions.DontRequireReceiver);
	}

	public void KillEnemy(Enemy e)
	{
		SendMessage("OnKillEnemy", e, SendMessageOptions.DontRequireReceiver);
	}

	public void HitReceived(Hit hit)
	{
		SendMessage("OnHitReceived", hit, SendMessageOptions.DontRequireReceiver);
	}

	public void PenitentHealthChanged(float life)
	{
		SendMessage("OnPenitentHealthChanged", life, SendMessageOptions.DontRequireReceiver);
	}

	public void BreakableBreak(BreakableObject breakable)
	{
		SendMessage("OnBreakBreakable", breakable, SendMessageOptions.DontRequireReceiver);
	}

	public void PenitentDead()
	{
		SendMessage("OnPenitentDead", SendMessageOptions.DontRequireReceiver);
	}

	public void NumberOfCurrentFlasksChanged(float newNumberOfFlasks)
	{
		SendMessage("OnNumberOfCurrentFlasksChanged", newNumberOfFlasks, SendMessageOptions.DontRequireReceiver);
	}

	public string GetBaseTranslationID()
	{
		return GetType().Name + "/" + id;
	}

	public bool WillBlockSwords()
	{
		bool result = false;
		ItemTemporalEffect[] components = GetComponents<ItemTemporalEffect>();
		foreach (ItemTemporalEffect itemTemporalEffect in components)
		{
			if (itemTemporalEffect.enabled && itemTemporalEffect.ContainsEffect(ItemTemporalEffect.PenitentEffects.DisableUnEquipSword))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public void RemoveAllObjectEffets()
	{
		ObjectEffect[] components = GetComponents<ObjectEffect>();
		foreach (ObjectEffect objectEffect in components)
		{
			if (objectEffect.IsApplied)
			{
				objectEffect.RemoveEffect(force: true);
			}
		}
	}
}
