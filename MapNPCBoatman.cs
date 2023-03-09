using UnityEngine;

public class MapNPCBoatman : AbstractMonoBehaviour
{
	private const int DIALOGUER_BOATMAN_STATE = 22;

	private const int W1 = 0;

	private const int W2 = 1;

	private const int W3 = 2;

	private const int WDLC = 3;

	[SerializeField]
	private MinMax blinkRange = new MinMax(2.5f, 4.5f);

	private float blinkTimer;

	private bool selectionMade;

	private void Start()
	{
		AddDialoguerEvents();
		Dialoguer.SetGlobalFloat(22, PlayerData.Data.GetMapData(Scenes.scene_map_world_DLC).sessionStarted ? 1 : 0);
		if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_1)
		{
			GetComponent<SpriteRenderer>().sortingOrder = 1000;
		}
		PlayerData.Data.hasUnlockedBoatman = true;
		PlayerData.SaveCurrentFile();
	}

	private void OnDestroy()
	{
		RemoveDialoguerEvents();
	}

	public void AddDialoguerEvents()
	{
		Dialoguer.events.onMessageEvent += OnDialoguerMessageEvent;
		Dialoguer.events.onStarted += OnDialoguerStart;
		Dialoguer.events.onEnded += OnDialoguerEnd;
	}

	public void RemoveDialoguerEvents()
	{
		Dialoguer.events.onMessageEvent -= OnDialoguerMessageEvent;
		Dialoguer.events.onStarted -= OnDialoguerStart;
		Dialoguer.events.onEnded -= OnDialoguerEnd;
	}

	private void SetOptions()
	{
		SpeechBubble instance = SpeechBubble.Instance;
		switch (PlayerData.Data.CurrentMap)
		{
		case Scenes.scene_map_world_1:
			instance.HideOptionByIndex(0);
			break;
		case Scenes.scene_map_world_2:
			instance.HideOptionByIndex(1);
			break;
		case Scenes.scene_map_world_3:
			instance.HideOptionByIndex(2);
			break;
		case Scenes.scene_map_world_DLC:
			instance.HideOptionByIndex(3);
			break;
		}
		if (!PlayerData.Data.GetMapData(Scenes.scene_map_world_2).sessionStarted)
		{
			instance.HideOptionByIndex(1);
		}
		if (!PlayerData.Data.GetMapData(Scenes.scene_map_world_3).sessionStarted)
		{
			instance.HideOptionByIndex(2);
		}
	}

	private void SelectWorld(string metadata)
	{
		if (selectionMade)
		{
			return;
		}
		Parser.IntTryParse(metadata, out var result);
		if (result <= -1)
		{
			return;
		}
		GetComponent<MapDialogueInteraction>().enabled = false;
		selectionMade = true;
		AudioManager.Play("sfx_worldmap_boattravel_accept");
		switch (result)
		{
		case 3:
			if (PlayerData.Data.GetMapData(Scenes.scene_map_world_DLC).sessionStarted)
			{
				SceneLoader.LoadScene(Scenes.scene_map_world_DLC, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
				PlayerData.Data.GetMapData(Scenes.scene_map_world_DLC).enteringFrom = PlayerData.MapData.EntryMethod.Boatman;
				break;
			}
			PlayerData.Data.Gift(PlayerId.PlayerOne, Charm.charm_chalice);
			PlayerData.Data.Gift(PlayerId.PlayerTwo, Charm.charm_chalice);
			PlayerData.Data.shouldShowChaliceTooltip = true;
			Cutscene.Load(Scenes.scene_level_kitchen, Scenes.scene_cutscene_dlc_intro, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
			PlayerData.Data.GetMapData(Scenes.scene_map_world_DLC).enteringFrom = PlayerData.MapData.EntryMethod.None;
			PlayerData.Data.CurrentMap = Scenes.scene_map_world_DLC;
			break;
		case 0:
			PlayerData.Data.GetMapData(Scenes.scene_map_world_1).enteringFrom = PlayerData.MapData.EntryMethod.Boatman;
			SceneLoader.LoadScene(Scenes.scene_map_world_1, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
			break;
		case 1:
			PlayerData.Data.GetMapData(Scenes.scene_map_world_2).enteringFrom = PlayerData.MapData.EntryMethod.Boatman;
			SceneLoader.LoadScene(Scenes.scene_map_world_2, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
			break;
		case 2:
			PlayerData.Data.GetMapData(Scenes.scene_map_world_3).enteringFrom = PlayerData.MapData.EntryMethod.Boatman;
			SceneLoader.LoadScene(Scenes.scene_map_world_3, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
			break;
		}
	}

	private void Update()
	{
		blinkTimer -= CupheadTime.Delta;
		if (blinkTimer < 0f)
		{
			blinkTimer = blinkRange.RandomFloat();
			base.animator.SetTrigger("Blink");
		}
	}

	private void OnDialoguerStart()
	{
		base.animator.SetBool("Talk", value: true);
	}

	private void OnDialoguerEnd()
	{
		base.animator.SetBool("Talk", value: false);
	}

	private void OnDialoguerMessageEvent(string message, string metadata)
	{
		if (message == "BoatmanSetOptions")
		{
			SetOptions();
		}
		if (message == "BoatmanSelection")
		{
			SelectWorld(metadata);
		}
	}
}
