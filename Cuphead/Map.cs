using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Map : AbstractMonoBehaviour
{
	public enum State
	{
		Starting,
		Ready,
		Event,
		Exiting,
		Graveyard
	}

	[Serializable]
	public class Camera
	{
		public bool moveX = true;

		public bool moveY = true;

		public CupheadBounds bounds = new CupheadBounds(-6.4f, 6.4f, 3.6f, -3.6f);
	}

	public MapResources MapResources;

	[SerializeField]
	private Camera cameraProperties;

	[Space(10f)]
	[SerializeField]
	private AbstractMapInteractiveEntity firstNode;

	[SerializeField]
	private AbstractMapInteractiveEntity[] entryPoints;

	private MapUI ui;

	private Scenes scene;

	private CupheadMapCamera cupheadMapCamera;

	public Levels level;

	public List<CoinPositionAndID> LevelCoinsIDs = new List<CoinPositionAndID>();

	protected int currentMusic;

	public static Map Current { get; private set; }

	public State CurrentState { get; set; }

	public MapPlayerController[] players { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		Current = this;
		Cuphead.Init();
		Level.ResetBossesHub();
		Level.IsGraveyard = false;
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: false, promptBeforeJoin: false);
		PlayerManager.OnPlayerJoinedEvent += OnPlayerJoined;
		PlayerManager.OnPlayerLeaveEvent += OnPlayerLeave;
		scene = EnumUtils.Parse<Scenes>(SceneManager.GetActiveScene().name);
		PlayerData.Data.CurrentMap = scene;
		CreateUI();
		CreatePlayers();
		ui.Init(players);
		cupheadMapCamera = UnityEngine.Object.FindObjectOfType<CupheadMapCamera>();
		cupheadMapCamera.Init(cameraProperties);
		CupheadTime.SetAll(1f);
		SceneLoader.OnLoaderCompleteEvent += SelectMusic;
	}

	private void Start()
	{
		if (PlatformHelper.ManuallyRefreshDLCAvailability)
		{
			DLCManager.CheckInstallationStatusChanged();
		}
		AudioManager.PlayLoop(string.Empty);
		StartCoroutine(start_cr());
	}

	private void OnDestroy()
	{
		SceneLoader.OnLoaderCompleteEvent -= SelectMusic;
		PlayerManager.OnPlayerJoinedEvent -= OnPlayerJoined;
		PlayerManager.OnPlayerLeaveEvent -= OnPlayerLeave;
		MapResources = null;
		cameraProperties = null;
		firstNode = null;
		entryPoints = null;
		ui = null;
		cupheadMapCamera = null;
		players = null;
		if (Current == this)
		{
			Current = null;
		}
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.DrawLine(new Vector3(cameraProperties.bounds.left, cameraProperties.bounds.top), new Vector3(cameraProperties.bounds.left, cameraProperties.bounds.bottom));
		Gizmos.DrawLine(new Vector3(cameraProperties.bounds.right, cameraProperties.bounds.top), new Vector3(cameraProperties.bounds.right, cameraProperties.bounds.bottom));
		Gizmos.DrawLine(new Vector3(cameraProperties.bounds.right, cameraProperties.bounds.top), new Vector3(cameraProperties.bounds.left, cameraProperties.bounds.top));
		Gizmos.DrawLine(new Vector3(cameraProperties.bounds.right, cameraProperties.bounds.bottom), new Vector3(cameraProperties.bounds.left, cameraProperties.bounds.bottom));
	}

	private void CreateUI()
	{
		ui = UnityEngine.Object.FindObjectOfType<MapUI>();
		if (ui == null)
		{
			ui = MapUI.Create();
		}
	}

	private void CreatePlayers()
	{
		if (!PlayerData.Data.CurrentMapData.sessionStarted)
		{
			PlayerData.Data.CurrentMapData.sessionStarted = true;
			PlayerData.Data.CurrentMapData.playerOnePosition = (Vector2)firstNode.transform.position + firstNode.returnPositions.playerOne;
			PlayerData.Data.CurrentMapData.playerTwoPosition = (Vector2)firstNode.transform.position + firstNode.returnPositions.playerTwo;
			if (!PlayerManager.Multiplayer)
			{
				PlayerData.Data.CurrentMapData.playerOnePosition = (Vector2)firstNode.transform.position + firstNode.returnPositions.singlePlayer;
			}
		}
		else if (PlayerData.Data.CurrentMapData.enteringFrom != 0)
		{
			entryPoints[(int)PlayerData.Data.CurrentMapData.enteringFrom].SetPlayerReturnPos();
			PlayerData.Data.CurrentMapData.enteringFrom = PlayerData.MapData.EntryMethod.None;
		}
		PlayerData.SaveCurrentFile();
		MapPlayerPose pose = MapPlayerPose.Default;
		if (Level.Won && Level.PreviousLevel != Levels.Saltbaker)
		{
			pose = MapPlayerPose.Won;
		}
		players = new MapPlayerController[2];
		players[0] = MapPlayerController.Create(PlayerId.PlayerOne, new MapPlayerController.InitObject(PlayerData.Data.CurrentMapData.playerOnePosition, pose));
		if (PlayerManager.Multiplayer)
		{
			players[1] = MapPlayerController.Create(PlayerId.PlayerTwo, new MapPlayerController.InitObject(PlayerData.Data.CurrentMapData.playerTwoPosition, pose));
		}
	}

	protected virtual void OnPlayerJoined(PlayerId playerId)
	{
		if (playerId == PlayerId.PlayerTwo)
		{
			Vector3 position = players[0].transform.position;
			Vector3 vector = position + new Vector3(0.05f, 0.05f, 0f);
			LayerMask layerMask = -257;
			for (int i = 0; i < 10; i++)
			{
				float num = 36 * -i + 150;
				Vector2 vector2 = new Vector2(Mathf.Cos((float)Math.PI / 180f * num), Mathf.Sin((float)Math.PI / 180f * num));
				if (!(Physics2D.CircleCast(position, 0.2f, vector2, 0.7f, layerMask.value).collider != null))
				{
					vector = position + (Vector3)(vector2 * 0.7f);
					break;
				}
			}
			players[1] = MapPlayerController.Create(PlayerId.PlayerTwo, new MapPlayerController.InitObject(vector, MapPlayerPose.Joined));
			players[1].animationController.spriteRenderer.sortingOrder = players[0].animationController.spriteRenderer.sortingOrder;
			LevelNewPlayerGUI.Current.Init();
			SetRichPresence();
		}
		CheckMusic(isRecheck: true);
	}

	protected virtual void OnPlayerLeave(PlayerId playerId)
	{
		if (playerId == PlayerId.PlayerTwo)
		{
			players[1].OnLeave();
		}
		CheckMusic(isRecheck: true);
	}

	protected virtual void SelectMusic()
	{
		currentMusic = -1;
		CheckMusic(isRecheck: false);
	}

	public void OnCloseEquipMenu()
	{
		CheckMusic(isRecheck: true);
	}

	public void OnNPCChangeMusic()
	{
		CheckMusic(isRecheck: true);
	}

	protected virtual void CheckMusic(bool isRecheck)
	{
		int num = currentMusic;
		PlayerData.PlayerLoadouts.PlayerLoadout playerLoadout = PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerOne);
		PlayerData.PlayerLoadouts.PlayerLoadout playerLoadout2 = PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerTwo);
		num = (((playerLoadout.charm != Charm.charm_curse || CharmCurse.CalculateLevel(PlayerId.PlayerOne) <= -1) && (!PlayerManager.Multiplayer || playerLoadout2.charm != Charm.charm_curse || CharmCurse.CalculateLevel(PlayerId.PlayerTwo) <= -1)) ? ((!PlayerData.Data.pianoAudioEnabled) ? (-1) : 0) : (((playerLoadout.charm != Charm.charm_curse || !CharmCurse.IsMaxLevel(PlayerId.PlayerOne)) && (!PlayerManager.Multiplayer || playerLoadout2.charm != Charm.charm_curse || !CharmCurse.IsMaxLevel(PlayerId.PlayerTwo))) ? ((!PlayerData.Data.pianoAudioEnabled) ? 1 : 3) : ((!PlayerData.Data.pianoAudioEnabled) ? 2 : 4)));
		if (!isRecheck || num != currentMusic)
		{
			currentMusic = num;
			if (currentMusic == -1)
			{
				AudioManager.PlayBGM();
			}
			else
			{
				AudioManager.StartBGMAlternate(currentMusic);
			}
		}
	}

	public void OnLoadLevel()
	{
	}

	public void OnLoadShop()
	{
	}

	private IEnumerator start_cr()
	{
		SetRichPresence();
		Level.ResetBossesHub();
		if (Level.Won && Level.PreviousLevel != Levels.Saltbaker)
		{
			yield return CupheadTime.WaitForSeconds(this, 1.5f);
			bool longPlayerAnimation4 = true;
			bool cameraMoved = false;
			Vector3 cameraStartPos = cupheadMapCamera.transform.position;
			if (AbstractMapLevelDependentEntity.RegisteredEntities != null)
			{
				while (AbstractMapLevelDependentEntity.RegisteredEntities.Count > 0)
				{
					yield return null;
					CurrentState = State.Event;
					AbstractMapLevelDependentEntity entity = AbstractMapLevelDependentEntity.RegisteredEntities[0];
					foreach (AbstractMapLevelDependentEntity registeredEntity in AbstractMapLevelDependentEntity.RegisteredEntities)
					{
						if (!registeredEntity.panCamera)
						{
							entity = registeredEntity;
							break;
						}
						if (!(registeredEntity == entity))
						{
							float num = Vector2.Distance(cupheadMapCamera.transform.position, registeredEntity.CameraPosition);
							if (num < Vector2.Distance(cupheadMapCamera.transform.position, entity.CameraPosition))
							{
								entity = registeredEntity;
							}
						}
					}
					AbstractMapLevelDependentEntity.RegisteredEntities.Remove(entity);
					if (entity.panCamera)
					{
						yield return cupheadMapCamera.MoveToPosition(entity.CameraPosition, 0.5f, 0.9f);
						cameraMoved = true;
					}
					entity.MapMeetCondition();
					while (entity.CurrentState != AbstractMapLevelDependentEntity.State.Complete)
					{
						yield return null;
					}
					yield return CupheadTime.WaitForSeconds(this, 0.25f);
					longPlayerAnimation4 = false;
				}
				if (cameraMoved)
				{
					cupheadMapCamera.MoveToPosition(cameraStartPos, 0.75f, 1f);
				}
			}
			if (!PlayerManager.playerWasChalice[0] && (!PlayerManager.Multiplayer || !PlayerManager.playerWasChalice[1]))
			{
				yield return CupheadTime.WaitForSeconds(this, (!longPlayerAnimation4) ? 1f : 2.5f);
				players[0].OnWinComplete();
				if (PlayerManager.Multiplayer)
				{
					players[1].OnWinComplete();
				}
			}
			else
			{
				if (PlayerManager.playerWasChalice[0])
				{
					players[0].OnWinComplete();
				}
				if (PlayerManager.Multiplayer && PlayerManager.playerWasChalice[1])
				{
					players[1].OnWinComplete();
				}
				yield return CupheadTime.WaitForSeconds(this, 1f);
				if (!PlayerManager.playerWasChalice[0])
				{
					players[0].OnWinComplete();
				}
				if (PlayerManager.Multiplayer && !PlayerManager.playerWasChalice[1])
				{
					players[1].OnWinComplete();
				}
			}
			if (!Level.PreviouslyWon || Level.PreviousDifficulty < Level.Mode.Normal || Level.PreviousLevel == Levels.Mausoleum)
			{
				if (!Level.IsDicePalace && !Level.IsDicePalaceMain && Level.PreviousLevel != Levels.Devil && Level.PreviousLevel != Levels.DicePalaceMain && Level.PreviousLevel != Levels.Mausoleum && Level.PreviousLevelType == Level.Type.Battle)
				{
					if (Array.IndexOf(Level.worldDLCBossLevels, Level.PreviousLevel) >= 0)
					{
						if (Level.Difficulty == Level.Mode.Easy && !PlayerData.Data.hasBeatenAnyDLCBossOnEasy && !PlayerData.Data.CheckLevelsHaveMinDifficulty(Level.worldDLCBossLevels, Level.Mode.Normal))
						{
							MapEventNotification.Current.ShowTooltipEvent(TooltipEvent.SimpleIngredient);
							PlayerData.Data.hasBeatenAnyDLCBossOnEasy = true;
							PlayerData.SaveCurrentFile();
						}
					}
					else if (Level.Difficulty == Level.Mode.Easy && !PlayerData.Data.hasBeatenAnyBossOnEasy && (!PlayerData.Data.CheckLevelsHaveMinDifficulty(Level.world1BossLevels, Level.Mode.Normal) || !PlayerData.Data.CheckLevelsHaveMinDifficulty(Level.world2BossLevels, Level.Mode.Normal) || !PlayerData.Data.CheckLevelsHaveMinDifficulty(Level.world3BossLevels, Level.Mode.Normal)))
					{
						MapEventNotification.Current.ShowTooltipEvent(TooltipEvent.KingDice);
						PlayerData.Data.hasBeatenAnyBossOnEasy = true;
						PlayerData.SaveCurrentFile();
					}
					if (Level.Difficulty >= Level.Mode.Normal)
					{
						if (Level.PreviousLevel == Levels.Airplane)
						{
							MapEventNotification.Current.ShowEvent(MapEventNotification.Type.AirplaneIngredient);
							while (MapEventNotification.Current.showing)
							{
								yield return null;
							}
							longPlayerAnimation4 = false;
							yield return CupheadTime.WaitForSeconds(this, 0.25f);
						}
						else if (Level.PreviousLevel == Levels.RumRunners)
						{
							MapEventNotification.Current.ShowEvent(MapEventNotification.Type.RumIngredient);
							while (MapEventNotification.Current.showing)
							{
								yield return null;
							}
							longPlayerAnimation4 = false;
							yield return CupheadTime.WaitForSeconds(this, 0.25f);
						}
						else if (Level.PreviousLevel == Levels.OldMan)
						{
							MapEventNotification.Current.ShowEvent(MapEventNotification.Type.OldManIngredient);
							while (MapEventNotification.Current.showing)
							{
								yield return null;
							}
							longPlayerAnimation4 = false;
							yield return CupheadTime.WaitForSeconds(this, 0.25f);
						}
						else if (Level.PreviousLevel == Levels.SnowCult)
						{
							MapEventNotification.Current.ShowEvent(MapEventNotification.Type.SnowIngredient);
							while (MapEventNotification.Current.showing)
							{
								yield return null;
							}
							longPlayerAnimation4 = false;
							yield return CupheadTime.WaitForSeconds(this, 0.25f);
						}
						else if (Level.PreviousLevel == Levels.FlyingCowboy)
						{
							MapEventNotification.Current.ShowEvent(MapEventNotification.Type.CowboyIngredient);
							while (MapEventNotification.Current.showing)
							{
								yield return null;
							}
							longPlayerAnimation4 = false;
							yield return CupheadTime.WaitForSeconds(this, 0.25f);
						}
						else if (Level.PreviousLevel == Levels.Graveyard)
						{
							GameObject.Find("GhostDetective").GetComponent<MapNPCGraveyardGhost>().TalkAfterPlayerGotCharm();
						}
						else if (Level.PreviousLevel != Levels.Saltbaker)
						{
							MapEventNotification.Current.ShowEvent(MapEventNotification.Type.SoulContract);
							while (MapEventNotification.Current.showing)
							{
								yield return null;
							}
							longPlayerAnimation4 = false;
							yield return CupheadTime.WaitForSeconds(this, 0.25f);
						}
					}
					if (PlayerData.Data.CheckLevelsHaveMinDifficulty(Level.worldDLCBossLevels, Level.Mode.Normal) && Array.IndexOf(Level.worldDLCBossLevels, Level.PreviousLevel) >= 0)
					{
						MapEventNotification.Current.ShowTooltipEvent(TooltipEvent.BackToKitchen);
					}
					int bossCounter = 0;
					for (int i = 0; i < Level.chaliceLevels.Length; i++)
					{
						if (PlayerData.Data.GetLevelData(Level.chaliceLevels[i]).completedAsChaliceP1)
						{
							bossCounter++;
						}
					}
				}
				else if (Level.SuperUnlocked)
				{
					MapEventNotification.Current.ShowEvent(MapEventNotification.Type.Super);
					if (!PlayerData.Data.hasUnlockedFirstSuper)
					{
						MapEventNotification.Current.ShowTooltipEvent(TooltipEvent.Mausoleum);
						PlayerData.Data.hasUnlockedFirstSuper = true;
						PlayerData.SaveCurrentFile();
					}
					longPlayerAnimation4 = false;
				}
			}
			while ((bool)MapEventNotification.Current && MapEventNotification.Current.showing)
			{
				yield return null;
			}
		}
		if (DLCManager.showAvailabilityPrompt)
		{
			yield return CupheadTime.WaitForSeconds(this, (!Level.Won) ? 1.5f : 0.5f);
			DLCManager.ResetAvailabilityPrompt();
			MapEventNotification.Current.ShowEvent(MapEventNotification.Type.DLCAvailable);
		}
		CurrentState = State.Ready;
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: true, promptBeforeJoin: true);
		InterruptingPrompt.SetCanInterrupt(canInterrupt: true);
		Level.ResetPreviousLevelInfo();
	}

	private void SetRichPresence()
	{
		OnlineManager.Instance.Interface.SetStat(PlayerId.Any, "WorldMap", SceneLoader.SceneName);
		OnlineManager.Instance.Interface.SetRichPresence(PlayerId.Any, "Exploring", active: true);
	}
}
