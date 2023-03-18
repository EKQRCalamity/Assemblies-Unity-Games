using System;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.FrameworkCore.Attributes.Logic;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.UI.Widgets;
using UnityEngine;

namespace Framework.Util.CompletionCommandHelper;

[CreateAssetMenu(fileName = "Completion List", menuName = "Blasphemous/Completion List", order = 0)]
public class CompletionAssets : ScriptableObject
{
	public Prayer[] prayers;

	public Relic[] relics;

	public Sword[] swordHearts;

	public Framework.Inventory.CollectibleItem[] collectibles;

	public RosaryBead[] beads;

	public TeleportDestination[] teleports;

	public string[] bossDeadFlags;

	public string[] arenaFlags;

	public string endingAFlag = "D07Z01S03_ENDING_A";

	public string[] amanecidasFlags = new string[4] { "SANTOS_AMANECIDA_AXE_DEFEATED", "SANTOS_AMANECIDA_BOW_DEFEATED", "SANTOS_AMANECIDA_FACCATA_DEFEATED", "SANTOS_AMANECIDA_LANCE_DEFEATED" };

	public IEnumerable<string> UnlockBaseGame()
	{
		Core.GameModeManager.ChangeMode(GameModeManager.GAME_MODES.NEW_GAME);
		ClearNGPlusData();
		AddBaseObjects(relics);
		yield return "Added Relics";
		AddBaseObjects(beads);
		yield return "Added Beads";
		AddBaseObjects(prayers);
		yield return "Added Prayers";
		AddBaseObjects(swordHearts);
		yield return "Added Sword Hearts";
		AddBaseObjects(collectibles);
		yield return "Added Collectibles";
		AddTeleports();
		yield return "Added Teleports";
		RevealFullMap();
		yield return "Added Full Map";
		UpgradeStat(EntityStats.StatsTypes.Fervour, 6);
		yield return "Upgraded Fervour";
		UpgradeStat(EntityStats.StatsTypes.Life, 6);
		yield return "Upgraded Health";
		UpgradeStat(EntityStats.StatsTypes.MeaCulpa, 7);
		yield return "Upgraded Mea Culpa";
		SetFlag(endingAFlag);
		yield return "Set Ending A";
		SetFlags(bossDeadFlags);
		yield return "Added Dead Bosses";
		SetFlags(arenaFlags);
		yield return "Added Arenas";
	}

	private void ClearNGPlusData()
	{
		SetFlags(amanecidasFlags, v: false);
	}

	public IEnumerable<string> UnlockNGPlus()
	{
		Core.GameModeManager.ChangeMode(GameModeManager.GAME_MODES.NEW_GAME_PLUS);
		RevealFullMap();
		yield return "Revealed NG+ map";
		SetFlags(amanecidasFlags);
		yield return "Added Amanecidas bosses defeated";
		Core.PenitenceManager.MarkPenitenceAsCompleted("PE01");
		yield return "Added Penitence PE01";
		Core.PenitenceManager.MarkPenitenceAsCompleted("PE02");
		yield return "Added Penitence PE02";
		Core.PenitenceManager.MarkPenitenceAsCompleted("PE03");
		yield return "Added Penitence PE03";
	}

	private void SetFlags(IEnumerable<string> flagList, bool v = true)
	{
		foreach (string flag in flagList)
		{
			SetFlag(flag, v);
		}
	}

	private void SetFlag(string flag, bool v = true)
	{
		if (!string.IsNullOrEmpty(flag))
		{
			RunCommand(string.Format("flag {0} {1}", (!v) ? "clear" : "set", flag));
		}
	}

	private void RevealFullMap()
	{
		RunCommand("map reveal all");
	}

	private void AddTeleports()
	{
		TeleportDestination[] array = teleports;
		foreach (TeleportDestination teleportDestination in array)
		{
			RunCommand($"teleport unlock {teleportDestination.id}");
		}
	}

	private void RunCommand(string command, int times = 1)
	{
		for (int i = 0; i < times; i++)
		{
			ConsoleWidget.Instance.ProcessCommand(command);
		}
	}

	private static void AddBaseObjects(IEnumerable<BaseInventoryObject> objects)
	{
		foreach (BaseInventoryObject @object in objects)
		{
			if ((bool)@object)
			{
				if (!IsOwned(@object))
				{
					Core.InventoryManager.AddBaseObject(@object);
					ConsoleWidget.Instance.WriteFormat("Added object: {0}", @object.id);
				}
				else
				{
					ConsoleWidget.Instance.WriteFormat("Object {0} already owned, ignoring", @object.id);
				}
			}
		}
	}

	private static void UpgradeStat(EntityStats.StatsTypes statType, int amount)
	{
		Framework.FrameworkCore.Attributes.Logic.Attribute byType = Core.Logic.Penitent.Stats.GetByType(statType);
		byType.ResetUpgrades();
		for (int i = 0; i < amount; i++)
		{
			byType.Upgrade();
		}
	}

	private static bool IsOwned(BaseInventoryObject o)
	{
		bool flag = false;
		return o.GetItemType() switch
		{
			InventoryManager.ItemType.Bead => Core.InventoryManager.IsRosaryBeadOwned(o.id), 
			InventoryManager.ItemType.Collectible => Core.InventoryManager.IsCollectibleItemOwned(o.id), 
			InventoryManager.ItemType.Prayer => Core.InventoryManager.IsPrayerOwned(o.id), 
			InventoryManager.ItemType.Quest => Core.InventoryManager.IsQuestItemOwned(o.id), 
			InventoryManager.ItemType.Relic => Core.InventoryManager.IsRelicOwned(o.id), 
			InventoryManager.ItemType.Sword => Core.InventoryManager.IsSwordOwned(o.id), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
