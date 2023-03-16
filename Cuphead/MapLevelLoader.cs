using System;
using UnityEngine;

public class MapLevelLoader : AbstractMapInteractiveEntity
{
	[SerializeField]
	private Levels level;

	[SerializeField]
	private bool askDifficulty;

	public Action OnBackCallback;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Activate(MapPlayerController player)
	{
		if (AbstractMapInteractiveEntity.HasPopupOpened)
		{
			return;
		}
		if (PlatformHelper.ManuallyRefreshDLCAvailability)
		{
			DLCManager.CheckInstallationStatusChanged();
			if (DLCManager.showAvailabilityPrompt)
			{
				DLCManager.ResetAvailabilityPrompt();
				MapEventNotification.Current.ShowEvent(MapEventNotification.Type.DLCAvailable);
				return;
			}
		}
		AbstractMapInteractiveEntity.HasPopupOpened = true;
		base.Activate(player);
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: false, promptBeforeJoin: false);
		AudioManager.Play("world_map_level_difficulty_appear");
		Map.Current.OnLoadLevel();
		PlayerData.Data.CurrentMapData.playerOnePosition = (Vector2)base.transform.position + returnPositions.playerOne;
		PlayerData.Data.CurrentMapData.playerTwoPosition = (Vector2)base.transform.position + returnPositions.playerTwo;
		if (!PlayerManager.Multiplayer)
		{
			PlayerData.Data.CurrentMapData.playerOnePosition = (Vector2)base.transform.position + returnPositions.singlePlayer;
		}
		if (askDifficulty)
		{
			MapDifficultySelectStartUI.Current.level = level.ToString();
			MapDifficultySelectStartUI.Current.In(player);
			MapDifficultySelectStartUI.Current.OnLoadLevelEvent += OnLoadLevel;
			MapDifficultySelectStartUI.Current.OnBackEvent += OnBack;
			return;
		}
		string text = level.ToString();
		if (text != "Mausoleum" && text != "Devil" && text != "ShmupTutorial" && text != "ChaliceTutorial" && text != "ChessCastle")
		{
			MapConfirmStartUI.Current.level = text;
			MapConfirmStartUI.Current.In(player);
			MapConfirmStartUI.Current.OnLoadLevelEvent += OnLoadLevel;
			MapConfirmStartUI.Current.OnBackEvent += OnBack;
			return;
		}
		if (text == "Mausoleum")
		{
			text = ((PlayerData.Data.CurrentMap == Scenes.scene_map_world_1) ? "Mausoleum_1" : ((PlayerData.Data.CurrentMap != Scenes.scene_map_world_2) ? "Mausoleum_3" : "Mausoleum_2"));
		}
		else if (text == "ChessCastle")
		{
			text = "KingOfGamesWorldMap";
		}
		MapBasicStartUI.Current.level = text;
		MapBasicStartUI.Current.In(player);
		MapBasicStartUI.Current.OnLoadLevelEvent += OnLoadLevel;
		MapBasicStartUI.Current.OnBackEvent += OnBack;
	}

	private void OnLoadLevel()
	{
		AbstractMapInteractiveEntity.HasPopupOpened = false;
		AudioManager.HandleSnapshot(AudioManager.Snapshots.Paused.ToString(), 0.5f);
		AudioNoiseHandler.Instance.BoingSound();
		if (level == Levels.Devil)
		{
			SceneLoader.LoadScene(Scenes.scene_cutscene_devil, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
		}
		else
		{
			SceneLoader.LoadLevel(level, SceneLoader.Transition.Iris);
		}
	}

	private void OnBack()
	{
		AbstractMapInteractiveEntity.HasPopupOpened = false;
		ReCheck();
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: true, promptBeforeJoin: true);
		if (askDifficulty)
		{
			MapDifficultySelectStartUI.Current.OnLoadLevelEvent -= OnLoadLevel;
			MapDifficultySelectStartUI.Current.OnBackEvent -= OnBack;
		}
		else
		{
			MapConfirmStartUI.Current.OnLoadLevelEvent -= OnLoadLevel;
			MapConfirmStartUI.Current.OnBackEvent -= OnBack;
			MapBasicStartUI.Current.OnLoadLevelEvent -= OnLoadLevel;
			MapBasicStartUI.Current.OnBackEvent -= OnBack;
		}
		if (OnBackCallback != null)
		{
			OnBackCallback();
			OnBackCallback = (Action)Delegate.Remove(OnBackCallback, OnBackCallback);
		}
	}

	protected override void Reset()
	{
		base.Reset();
		dialogueProperties = new AbstractUIInteractionDialogue.Properties("ENTER <sprite=0>");
	}
}
