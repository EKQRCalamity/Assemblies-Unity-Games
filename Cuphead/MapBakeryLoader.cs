using UnityEngine;

public class MapBakeryLoader : AbstractMapInteractiveEntity
{
	private MapPlayerController player1;

	private MapPlayerController player2;

	private bool p1InTrigger;

	private bool p2InTrigger;

	private bool loadKitchen;

	private void Start()
	{
		if (PlayerData.Data.shouldShowChaliceTooltip)
		{
			MapEventNotification.Current.ShowTooltipEvent(TooltipEvent.Chalice);
			PlayerData.Data.shouldShowChaliceTooltip = false;
		}
	}

	protected override void Activate(MapPlayerController player)
	{
		if (!AbstractMapInteractiveEntity.HasPopupOpened)
		{
			AbstractMapInteractiveEntity.HasPopupOpened = true;
			base.Activate(player);
			PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: false, promptBeforeJoin: false);
			AudioManager.Play("world_map_level_difficulty_appear");
			PlayerData.Data.CurrentMapData.playerOnePosition = (Vector2)base.transform.position + returnPositions.playerOne;
			PlayerData.Data.CurrentMapData.playerTwoPosition = (Vector2)base.transform.position + returnPositions.playerTwo;
			if (!PlayerManager.Multiplayer)
			{
				PlayerData.Data.CurrentMapData.playerOnePosition = (Vector2)base.transform.position + returnPositions.singlePlayer;
			}
			if (PlayerData.Data.GetLevelData(Levels.Saltbaker).played && !HoldingLAndR())
			{
				MapDifficultySelectStartUI.Current.level = "Saltbaker";
				MapDifficultySelectStartUI.Current.In(player);
				MapDifficultySelectStartUI.Current.OnLoadLevelEvent += OnLoadLevel;
				MapDifficultySelectStartUI.Current.OnBackEvent += OnBack;
				loadKitchen = false;
			}
			else
			{
				MapBasicStartUI.Current.level = "BakeryWorldMap";
				MapBasicStartUI.Current.In(player);
				MapBasicStartUI.Current.OnLoadLevelEvent += OnLoadLevel;
				MapBasicStartUI.Current.OnBackEvent += OnBack;
				loadKitchen = true;
			}
		}
	}

	private bool HoldingLAndR()
	{
		if ((bool)Map.Current.players[0] && Map.Current.players[0].input.actions.GetButton(11) && Map.Current.players[0].input.actions.GetButton(12))
		{
			return true;
		}
		if ((bool)Map.Current.players[1] && Map.Current.players[1].input.actions.GetButton(11) && Map.Current.players[1].input.actions.GetButton(12))
		{
			return true;
		}
		return false;
	}

	private void OnLoadLevel()
	{
		AbstractMapInteractiveEntity.HasPopupOpened = false;
		AudioNoiseHandler.Instance.BoingSound();
		if (loadKitchen)
		{
			SceneLoader.LoadScene(Scenes.scene_level_kitchen, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris, SceneLoader.Icon.None);
		}
		else
		{
			SceneLoader.LoadScene(Scenes.scene_level_saltbaker, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
		}
	}

	private void OnBack()
	{
		AbstractMapInteractiveEntity.HasPopupOpened = false;
		ReCheck();
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: true, promptBeforeJoin: true);
		MapDifficultySelectStartUI.Current.OnLoadLevelEvent -= OnLoadLevel;
		MapDifficultySelectStartUI.Current.OnBackEvent -= OnBack;
		MapConfirmStartUI.Current.OnLoadLevelEvent -= OnLoadLevel;
		MapConfirmStartUI.Current.OnBackEvent -= OnBack;
		MapBasicStartUI.Current.OnLoadLevelEvent -= OnLoadLevel;
		MapBasicStartUI.Current.OnBackEvent -= OnBack;
	}
}
