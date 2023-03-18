using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Gameplay.UI;
using I2.Loc;

namespace Tools.Items;

public class ChaliceEffect : ObjectEffect
{
	public List<string> EnemiesNames;

	public string CurrentChaliceId;

	public const string FLAGNAME_PUZZLE_COMPLETED = "D01Z05S23_CHALICEPUZZLE";

	private const string FIRST_CHALICE_ID = "QI75";

	private const string SECOND_CHALICE_ID = "QI76";

	private const string THIRD_CHALICE_ID = "QI77";

	public static bool ShouldUnfillChalice;

	protected override bool OnApplyEffect()
	{
		ClearEvents();
		SuscribeToEvents();
		return base.OnApplyEffect();
	}

	private void ClearEvents()
	{
		SpawnManager.OnTeleport -= OnTeleport;
		SpawnManager.OnTeleportPrieDieu -= OnTeleportPrieDieu;
		Entity.Death -= OnEntityDead;
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		Core.Events.OnFlagChanged -= OnFlagChanged;
	}

	private void SuscribeToEvents()
	{
		SpawnManager.OnTeleport += OnTeleport;
		SpawnManager.OnTeleportPrieDieu += OnTeleportPrieDieu;
		Entity.Death += OnEntityDead;
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		Core.Events.OnFlagChanged += OnFlagChanged;
	}

	private void OnFlagChanged(string flag, bool flagactive)
	{
		if (flag == "D01Z05S23_CHALICEPUZZLE" && flagactive)
		{
			ClearEvents();
		}
	}

	protected override void OnRemoveEffect()
	{
		ClearEvents();
		base.OnRemoveEffect();
	}

	private void OnLevelLoaded(Framework.FrameworkCore.Level oldLevel, Framework.FrameworkCore.Level newLevel)
	{
		if ((newLevel == null || newLevel.LevelName.StartsWith("Main")) && (oldLevel == null || !oldLevel.LevelName.StartsWith("Main")))
		{
			ClearEvents();
		}
		else if (ShouldUnfillChalice)
		{
			ShouldUnfillChalice = false;
			ClearEnemiesFlags();
			ClearEvents();
			UnfillChalice();
		}
	}

	private void OnTeleport(string spawnId)
	{
		bool flag = false;
		foreach (TeleportDestination allUIActiveTeleport in Core.SpawnManager.GetAllUIActiveTeleports())
		{
			if (allUIActiveTeleport.teleportName.Equals(spawnId))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			LevelManager.OnLevelLoaded += OnLevelLoadedTeleport;
		}
	}

	private void OnTeleportPrieDieu(string spawnId)
	{
		LevelManager.OnLevelLoaded += OnLevelLoadedTeleport;
	}

	private void OnLevelLoadedTeleport(Framework.FrameworkCore.Level oldLevel, Framework.FrameworkCore.Level newLevel)
	{
		LevelManager.OnLevelLoaded -= OnLevelLoadedTeleport;
		if (!newLevel.LevelName.StartsWith("D19Z01") && !newLevel.LevelName.StartsWith("D13Z01") && !newLevel.LevelName.StartsWith("D18Z01") && !oldLevel.LevelName.StartsWith("D19Z01") && !oldLevel.LevelName.StartsWith("D13Z01") && !oldLevel.LevelName.StartsWith("D18Z01"))
		{
			ClearEnemiesFlags();
			ClearEvents();
			UnfillChalice();
		}
	}

	private void OnEntityDead(Entity entity)
	{
		Enemy enemy = entity as Enemy;
		if ((bool)enemy)
		{
			CheckAndFillChalice(enemy);
			return;
		}
		Penitent penitent = entity as Penitent;
		if ((bool)penitent && !Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE) && !Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.BOSS_RUSH))
		{
			ClearEnemiesFlags();
			ClearEvents();
			UnfillChalice();
		}
	}

	private void ClearEnemiesFlags()
	{
		foreach (string enemiesName in EnemiesNames)
		{
			Core.Events.SetFlag(enemiesName.ToUpper() + "_CHALICE_DEAD", b: false);
		}
	}

	private void UnfillChalice()
	{
		switch (CurrentChaliceId)
		{
		case "QI75":
			break;
		case "QI76":
			Core.InventoryManager.RemoveQuestItem(Core.InventoryManager.GetQuestItem("QI76"));
			Core.InventoryManager.AddQuestItem(Core.InventoryManager.GetQuestItem("QI75"));
			UIController.instance.ShowPopUp(ScriptLocalization.UI_Inventory.TEXT_QI76_OR_QI77_UNFILLS, string.Empty, 5f, blockPlayer: false);
			break;
		case "QI77":
			Core.InventoryManager.RemoveQuestItem(Core.InventoryManager.GetQuestItem("QI77"));
			Core.InventoryManager.AddQuestItem(Core.InventoryManager.GetQuestItem("QI75"));
			UIController.instance.ShowPopUp(ScriptLocalization.UI_Inventory.TEXT_QI76_OR_QI77_UNFILLS, string.Empty, 5f, blockPlayer: false);
			break;
		}
	}

	private void CheckAndFillChalice(Enemy enemy)
	{
		foreach (string enemiesName in EnemiesNames)
		{
			if (!enemy.name.StartsWith(enemiesName))
			{
				continue;
			}
			if (Core.Events.GetFlag(enemiesName.ToUpper() + "_CHALICE_DEAD"))
			{
				break;
			}
			Core.Events.SetFlag(enemiesName.ToUpper() + "_CHALICE_DEAD", b: true);
			switch (CurrentChaliceId)
			{
			case "QI75":
				ClearEvents();
				Core.InventoryManager.RemoveQuestItem(Core.InventoryManager.GetQuestItem("QI75"));
				Core.InventoryManager.AddQuestItem(Core.InventoryManager.GetQuestItem("QI76"));
				UIController.instance.ShowPopUp(ScriptLocalization.UI_Inventory.TEXT_QI75_FILLS, string.Empty, 5f, blockPlayer: false);
				break;
			case "QI76":
			{
				bool flag = true;
				foreach (string enemiesName2 in EnemiesNames)
				{
					if (!Core.Events.GetFlag(enemiesName2.ToUpper() + "_CHALICE_DEAD"))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					ClearEvents();
					Core.InventoryManager.RemoveQuestItem(Core.InventoryManager.GetQuestItem("QI76"));
					Core.InventoryManager.AddQuestItem(Core.InventoryManager.GetQuestItem("QI77"));
					UIController.instance.ShowPopUp(ScriptLocalization.UI_Inventory.TEXT_QI76_FILLS, string.Empty, 5f, blockPlayer: false);
				}
				else
				{
					UIController.instance.ShowPopUp(ScriptLocalization.UI_Inventory.TEXT_QI75_FILLS, string.Empty, 5f, blockPlayer: false);
				}
				break;
			}
			case "QI77":
				break;
			}
			break;
		}
	}
}
