using UnityEngine;

public class MapSceneLoader : AbstractMapInteractiveEntity
{
	[SerializeField]
	protected Scenes scene;

	[SerializeField]
	private bool askDifficulty;

	protected override void Activate(MapPlayerController player)
	{
		base.Activate(player);
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: false, promptBeforeJoin: false);
		AudioManager.Play("world_map_level_difficulty_appear");
		SetPlayerReturnPos();
		if (askDifficulty)
		{
			if (scene == Scenes.scene_cutscene_kingdice)
			{
				MapDifficultySelectStartUI.Current.level = "DicePalaceMain";
			}
			MapDifficultySelectStartUI.Current.In(player);
			MapDifficultySelectStartUI.Current.OnLoadLevelEvent += OnLoadLevel;
			MapDifficultySelectStartUI.Current.OnBackEvent += OnBack;
			return;
		}
		if (scene == Scenes.scene_map_world_1)
		{
			PlayerData.Data.GetMapData(Scenes.scene_map_world_1).enteringFrom = PlayerData.MapData.EntryMethod.DiceHouseRight;
			MapBasicStartUI.Current.level = "MapWorld_1";
		}
		else if (scene == Scenes.scene_map_world_2)
		{
			PlayerData.Data.GetMapData(Scenes.scene_map_world_2).enteringFrom = PlayerData.MapData.EntryMethod.DiceHouseRight;
			MapBasicStartUI.Current.level = "MapWorld_2";
		}
		else if (scene == Scenes.scene_map_world_3)
		{
			if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_4)
			{
				MapBasicStartUI.Current.level = "KingDiceToWorld3WorldMap";
			}
			else
			{
				MapBasicStartUI.Current.level = "MapWorld_3";
			}
		}
		else if (scene == Scenes.scene_map_world_4)
		{
			MapBasicStartUI.Current.level = "Inkwell";
		}
		else if (scene == Scenes.scene_cutscene_kingdice)
		{
			MapBasicStartUI.Current.level = "KingDice";
		}
		MapBasicStartUI.Current.In(player);
		MapBasicStartUI.Current.OnLoadLevelEvent += OnLoadLevel;
		MapBasicStartUI.Current.OnBackEvent += OnBack;
	}

	private void OnLoadLevel()
	{
		AudioManager.HandleSnapshot(AudioManager.Snapshots.Paused.ToString(), 0.5f);
		AudioNoiseHandler.Instance.BoingSound();
		LoadScene();
	}

	protected virtual void LoadScene()
	{
		MapDifficultySelectStartUI.Current.OnLoadLevelEvent -= OnLoadLevel;
		MapDifficultySelectStartUI.Current.OnBackEvent -= OnBack;
		MapBasicStartUI.Current.OnLoadLevelEvent -= OnLoadLevel;
		MapBasicStartUI.Current.OnBackEvent -= OnBack;
		SceneLoader.LoadScene(scene, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
	}

	private void OnBack()
	{
		ReCheck();
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: true, promptBeforeJoin: true);
		MapDifficultySelectStartUI.Current.OnLoadLevelEvent -= OnLoadLevel;
		MapDifficultySelectStartUI.Current.OnBackEvent -= OnBack;
		MapBasicStartUI.Current.OnLoadLevelEvent -= OnLoadLevel;
		MapBasicStartUI.Current.OnBackEvent -= OnBack;
	}

	protected override void Reset()
	{
		base.Reset();
		dialogueProperties = new AbstractUIInteractionDialogue.Properties("ENTER <sprite=0>");
	}
}
