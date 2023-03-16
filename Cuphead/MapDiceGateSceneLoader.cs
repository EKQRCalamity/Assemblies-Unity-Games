using UnityEngine;

public class MapDiceGateSceneLoader : AbstractMapInteractiveEntity
{
	[SerializeField]
	private Scenes nextWorld;

	private readonly Scenes diceGate = Scenes.scene_level_dice_gate;

	[SerializeField]
	private bool askDifficulty;

	protected override void Activate(MapPlayerController player)
	{
		base.Activate(player);
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: false, promptBeforeJoin: false);
		AudioManager.Play("world_map_level_difficulty_appear");
		Map.Current.OnLoadLevel();
		SetPlayerReturnPos();
		if (nextWorld == Scenes.scene_map_world_1)
		{
			MapBasicStartUI.Current.level = "MapWorld_1";
		}
		else if (nextWorld == Scenes.scene_map_world_2)
		{
			MapBasicStartUI.Current.level = ((!PlayerData.Data.GetMapData(Scenes.scene_map_world_2).sessionStarted) ? "DieHouse" : "MapWorld_2");
		}
		else if (nextWorld == Scenes.scene_map_world_3)
		{
			MapBasicStartUI.Current.level = ((!PlayerData.Data.GetMapData(Scenes.scene_map_world_2).sessionStarted) ? "DieHouse" : "MapWorld_3");
		}
		else if (nextWorld == Scenes.scene_map_world_4)
		{
			MapBasicStartUI.Current.level = "Inkwell";
		}
		MapBasicStartUI.Current.In(player);
		MapBasicStartUI.Current.OnLoadLevelEvent += OnLoadLevel;
		MapBasicStartUI.Current.OnBackEvent += OnBack;
	}

	private void OnLoadLevel()
	{
		AudioManager.HandleSnapshot(AudioManager.Snapshots.Paused.ToString(), 0.5f);
		AudioNoiseHandler.Instance.BoingSound();
		CheckSceneToLoad();
	}

	private void CheckSceneToLoad()
	{
		if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_1)
		{
			if (PlayerData.Data.GetMapData(Scenes.scene_map_world_2).sessionStarted)
			{
				PlayerData.Data.GetMapData(Scenes.scene_map_world_2).enteringFrom = PlayerData.MapData.EntryMethod.DiceHouseLeft;
				SceneLoader.LoadScene(nextWorld, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
			}
			else
			{
				SceneLoader.LoadScene(diceGate, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
			}
		}
		else if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_2)
		{
			if (PlayerData.Data.GetMapData(Scenes.scene_map_world_3).sessionStarted)
			{
				PlayerData.Data.GetMapData(Scenes.scene_map_world_3).enteringFrom = PlayerData.MapData.EntryMethod.DiceHouseLeft;
				SceneLoader.LoadScene(nextWorld, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
			}
			else
			{
				SceneLoader.LoadScene(diceGate, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
			}
		}
	}

	private void OnBack()
	{
		ReCheck();
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: true, promptBeforeJoin: true);
		MapBasicStartUI.Current.OnLoadLevelEvent -= OnLoadLevel;
		MapBasicStartUI.Current.OnBackEvent -= OnBack;
	}

	protected override void Reset()
	{
		base.Reset();
		dialogueProperties = new AbstractUIInteractionDialogue.Properties("ENTER <sprite=0>");
	}
}
