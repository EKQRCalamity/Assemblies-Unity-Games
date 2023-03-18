using System;
using System.Collections;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Inventory;
using Framework.Util;
using Gameplay.UI;
using Gameplay.UI.Widgets;
using UnityEngine;

namespace Framework.Managers;

public class DemakeManager : GameSystem
{
	public List<int> purgeRewardsByIndex = new List<int> { 100, 200, 500, 1000, 2000, 3000 };

	public const string DEMAKE_DISTRICT = "D25";

	private const string FIRST_DEMAKE_SCENE = "D25Z01S01";

	public const string INTRO_DEMAKE_ID = "DEMAKE_INTRO";

	public const string INTRO_DEMAKE_EVENT = "event:/Background Layer/DemakePressStartScreen";

	public const string ARCADE_MUSIC_EVENT = "event:/SFX/DEMAKE/ArcadeMusic";

	public const string SKIN2_FLAG = "DEMAKE_GBSKIN";

	private const int DEMAKE_SLOT = 8;

	private int prevSlot;

	private GameModeManager.GAME_MODES prevMode;

	private string prevLevel;

	private bool oldShakeValue;

	private float prevPurge;

	private float prevLife;

	private float prevFervour;

	private float purgeToGrant;

	private bool grantSkin1;

	private bool grantSkin2;

	public Core.SimpleEvent OnDemakeLevelCompletion;

	public override void Initialize()
	{
		GameModeManager gameModeManager = Core.GameModeManager;
		gameModeManager.OnExitDemakeMode = (Core.SimpleEvent)Delegate.Combine(gameModeManager.OnExitDemakeMode, new Core.SimpleEvent(OnExitDemakeMode));
		GameModeManager gameModeManager2 = Core.GameModeManager;
		gameModeManager2.OnEnterDemakeMode = (Core.SimpleEvent)Delegate.Combine(gameModeManager2.OnEnterDemakeMode, new Core.SimpleEvent(OnEnterDemakeMode));
	}

	private void OnEnterDemakeMode()
	{
		UIController.instance.CanOpenInventory = false;
	}

	private void OnExitDemakeMode()
	{
		UIController.instance.CanOpenInventory = true;
	}

	private void OnBeforeLevelLoad(Level oldLevel, Level newLevel)
	{
		if (newLevel.LevelName.StartsWith("D25") && !Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE))
		{
			Core.GameModeManager.ChangeMode(GameModeManager.GAME_MODES.DEMAKE);
			Core.PenitenceManager.UseStocksOfHealth = true;
			prevMode = GameModeManager.GAME_MODES.DEMAKE;
		}
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		if (string.IsNullOrEmpty(prevLevel))
		{
			prevLevel = Core.LevelManager.currentLevel.LevelName;
		}
	}

	public override void Update()
	{
		base.Update();
	}

	public override void Dispose()
	{
	}

	public void StartDemakeRun()
	{
		Singleton<Core>.Instance.StartCoroutine(LoadDemakeFadeWhiteCourrutine());
	}

	public void DemakeLevelVictory()
	{
		if (OnDemakeLevelCompletion != null)
		{
			OnDemakeLevelCompletion();
		}
	}

	public void EndDemakeRun(bool completed, int numSpecialItems)
	{
		Singleton<Core>.Instance.StartCoroutine(LoadArcadeFadeCourrutine(completed, numSpecialItems));
	}

	private void OnArcadeLevelLoad(Level newLevel, Level oldLevel)
	{
		LevelManager.OnLevelLoaded -= OnArcadeLevelLoad;
		Singleton<Core>.Instance.StartCoroutine(WaitBeforeRewards());
	}

	private IEnumerator WaitBeforeRewards()
	{
		Core.Logic.Penitent.Stats.Purge.Current = prevPurge;
		prevPurge = 0f;
		Core.Logic.Penitent.Stats.Life.Current = prevLife;
		prevLife = 0f;
		Core.Logic.Penitent.Stats.Fervour.Current = prevFervour;
		prevFervour = 0f;
		yield return new WaitForSecondsRealtime(1f);
		if (grantSkin1)
		{
			grantSkin1 = false;
			Core.ColorPaletteManager.UnlockDemake1ColorPalette();
			yield return new WaitForSecondsRealtime(0.05f);
			yield return new WaitUntil(() => !UIController.instance.IsUnlockActive());
		}
		if (grantSkin2)
		{
			grantSkin2 = false;
			Core.ColorPaletteManager.UnlockDemake2ColorPalette();
			yield return new WaitForSecondsRealtime(0.05f);
			yield return new WaitUntil(() => !UIController.instance.IsUnlockActive());
		}
		if (purgeToGrant > 0f)
		{
			Core.Logic.Penitent.Stats.Purge.Current += purgeToGrant;
			purgeToGrant = 0f;
			if (!grantSkin1)
			{
				BaseInventoryObject baseObjectOrTears = Core.InventoryManager.GetBaseObjectOrTears("QI78", InventoryManager.ItemType.Quest);
				UIController.instance.ShowObjectPopUp(UIController.PopupItemAction.GetObejct, baseObjectOrTears.caption, baseObjectOrTears.picture, InventoryManager.ItemType.Quest, 3f, blockPlayer: true);
			}
		}
	}

	private IEnumerator LoadDemakeFadeWhiteCourrutine()
	{
		yield return FadeWidget.instance.FadeCoroutine(new Color(0f, 0f, 0f, 0f), Color.white, 2f, toBlack: true, null);
		UIController.instance.HideIntroDemakeWidget();
		prevSlot = PersistentManager.GetAutomaticSlot();
		prevLevel = Core.LevelManager.currentLevel.LevelName;
		prevMode = Core.GameModeManager.GetCurrentGameMode();
		prevPurge = Core.Logic.Penitent.Stats.Purge.Current;
		prevLife = Core.Logic.Penitent.Stats.Life.Current;
		prevFervour = Core.Logic.Penitent.Stats.Fervour.Current;
		Core.SpawnManager.SetCurrentToCustomSpawnData(IsMiriam: false);
		PersistentManager.SetAutomaticSlot(8);
		Core.GameModeManager.ChangeMode(GameModeManager.GAME_MODES.DEMAKE);
		Core.Persistence.SaveGame(fullSave: false);
		Core.Logic.Penitent.Stats.Life.SetPermanentBonus(0f);
		Core.Logic.Penitent.Stats.Life.SetToCurrentMax();
		Core.Logic.Penitent.Stats.Flask.SetToCurrentMax();
		Core.Logic.Penitent.Stats.Purge.Current = 0f;
		Core.Logic.Penitent.Stats.Fervour.Current = 0f;
		Core.Logic.Penitent.Stats.Strength.ResetBonus();
		Core.Logic.Penitent.Stats.Strength.ResetUpgrades();
		Core.PenitenceManager.DeactivateCurrentPenitence();
		Core.PenitenceManager.UseStocksOfHealth = true;
		Core.InventoryManager.RemoveEquipableObjects();
		oldShakeValue = Core.Logic.CameraManager.ProCamera2DShake.enabled;
		Core.Logic.CameraManager.ProCamera2DShake.enabled = false;
		Core.SpawnManager.PrepareForSpawnFromEditor();
		Core.LevelManager.ChangeLevel("D25Z01S01", startFromEditor: false, useFade: false, forceDeactivate: false, Color.white);
	}

	private IEnumerator LoadArcadeFadeCourrutine(bool completed, int numSpecialItems)
	{
		if (completed)
		{
			yield return FadeWidget.instance.FadeCoroutine(new Color(0f, 0f, 0f, 0f), Color.white, 2f, toBlack: true, null);
		}
		else
		{
			yield return FadeWidget.instance.FadeCoroutine(Color.black, Color.white, 2f, toBlack: true, null);
		}
		purgeToGrant = Core.Logic.Penitent.Stats.Purge.Current;
		grantSkin2 = Core.Events.GetFlag("DEMAKE_GBSKIN");
		PersistentManager.SetAutomaticSlot(prevSlot);
		Core.Persistence.LoadGameWithOutRespawn(prevSlot);
		Core.GameModeManager.ChangeMode(prevMode);
		Core.Logic.CameraManager.ProCamera2DShake.enabled = oldShakeValue;
		Core.Persistence.SaveGame(fullSave: false);
		LevelManager.OnLevelLoaded += OnArcadeLevelLoad;
		if (completed)
		{
			if (numSpecialItems == purgeRewardsByIndex.Count - 1 && !Core.ColorPaletteManager.IsColorPaletteUnlocked("PENITENT_DEMAKE"))
			{
				grantSkin1 = true;
				purgeToGrant = 0f;
			}
			else
			{
				purgeToGrant += purgeRewardsByIndex[numSpecialItems];
			}
		}
		Core.SpawnManager.SpawnFromCustom(usefade: true, Color.white);
	}
}
