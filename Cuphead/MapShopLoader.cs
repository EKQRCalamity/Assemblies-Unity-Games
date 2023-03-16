using UnityEngine;

public class MapShopLoader : AbstractMapInteractiveEntity
{
	[SerializeField]
	private bool isDLCShop;

	protected override void Activate(MapPlayerController player)
	{
		if (!AbstractMapInteractiveEntity.HasPopupOpened)
		{
			AbstractMapInteractiveEntity.HasPopupOpened = true;
			base.Activate(player);
			PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: false, promptBeforeJoin: false);
			AudioManager.Play("world_map_level_difficulty_appear");
			Map.Current.OnLoadShop();
			PlayerData.Data.CurrentMapData.playerOnePosition = (Vector2)base.transform.position + returnPositions.playerOne;
			PlayerData.Data.CurrentMapData.playerTwoPosition = (Vector2)base.transform.position + returnPositions.playerTwo;
			if (!PlayerManager.Multiplayer)
			{
				PlayerData.Data.CurrentMapData.playerOnePosition = (Vector2)base.transform.position + returnPositions.singlePlayer;
			}
			MapBasicStartUI.Current.level = "Shop";
			MapBasicStartUI.Current.In(player);
			MapBasicStartUI.Current.OnLoadLevelEvent += OnLoadLevel;
			MapBasicStartUI.Current.OnBackEvent += OnBack;
		}
	}

	private void OnLoadLevel()
	{
		AbstractMapInteractiveEntity.HasPopupOpened = false;
		AudioNoiseHandler.Instance.BoingSound();
		SceneLoader.LoadScene((!isDLCShop) ? Scenes.scene_shop : Scenes.scene_shop_DLC, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris, SceneLoader.Icon.None);
	}

	private void OnBack()
	{
		AbstractMapInteractiveEntity.HasPopupOpened = false;
		ReCheck();
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: true, promptBeforeJoin: true);
		MapBasicStartUI.Current.OnLoadLevelEvent -= OnLoadLevel;
		MapBasicStartUI.Current.OnBackEvent -= OnBack;
	}
}
