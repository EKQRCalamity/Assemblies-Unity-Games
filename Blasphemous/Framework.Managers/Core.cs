using System;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Tools.Level;
using UnityEngine;

namespace Framework.Managers;

[DefaultExecutionOrder(-2)]
public class Core : Singleton<Core>
{
	public delegate void SimpleEvent();

	public delegate void SimpleEventParam(object param);

	public delegate void GenericEvent(UnityEngine.Object param);

	public delegate void ObjectEvent(GameObject param);

	public delegate void StringEvent(string param);

	public delegate void EntityEvent(Entity entity);

	public delegate void InteractableEvent(Interactable entity);

	public delegate void RegionEvent(Region entity);

	public bool debugMode;

	public static bool ready;

	public static bool preinit;

	private List<GameSystem> systems = new List<GameSystem>();

	public static LogicManager Logic { get; private set; }

	public static UIManager UI { get; private set; }

	public static InputManager Input { get; private set; }

	public static FMODAudioManager Audio { get; private set; }

	public static ScreenManager Screen { get; private set; }

	public static InventoryManager InventoryManager { get; private set; }

	public static SpawnManager SpawnManager { get; private set; }

	public static PersistentManager Persistence { get; private set; }

	public static LocalizationManager Localization { get; private set; }

	public static EventManager Events { get; private set; }

	public static MetricManager Metrics { get; private set; }

	public static DialogManager Dialog { get; private set; }

	public static Cinematics Cinematics { get; private set; }

	public static ExceptionHandler ExceptionHandler { get; private set; }

	public static SkillManager SkillManager { get; private set; }

	public static MapManager MapManager { get; private set; }

	public static NewMapManager NewMapManager { get; private set; }

	public static LevelManager LevelManager { get; private set; }

	public static CamerasManager CamerasManager { get; private set; }

	public static GuiltManager GuiltManager { get; private set; }

	public static DLCManager DLCManager { get; private set; }

	public static AchievementsManager AchievementsManager { get; private set; }

	public static ColorPaletteManager ColorPaletteManager { get; private set; }

	public static TutorialManager TutorialManager { get; private set; }

	public static ControlRemapManager ControlRemapManager { get; private set; }

	public static GameModeManager GameModeManager { get; private set; }

	public static PatchNotesManager PatchNotesManager { get; private set; }

	public static PenitenceManager PenitenceManager { get; private set; }

	public static SharedCommands SharedCommands { get; private set; }

	public static AlmsManager Alms { get; private set; }

	public static BossRushManager BossRushManager { get; private set; }

	public static DemakeManager DemakeManager { get; private set; }

	public void PreInit()
	{
		if (!ready)
		{
			Log.Raw("============================================== (Initialization: Framework)");
			ReadParameters();
			EventManager system = (Events = new EventManager());
			AddSystem(system);
			MetricManager system2 = (Metrics = new MetricManager());
			AddSystem(system2);
			LogicManager system3 = (Logic = new LogicManager());
			AddSystem(system3);
			LevelManager system4 = (LevelManager = new LevelManager());
			AddSystem(system4);
			UIManager system5 = (UI = new UIManager());
			AddSystem(system5);
			InputManager system6 = (Input = new InputManager());
			AddSystem(system6);
			FMODAudioManager system7 = (Audio = new FMODAudioManager());
			AddSystem(system7);
			PersistentManager system8 = (Persistence = new PersistentManager());
			AddSystem(system8);
			ScreenManager system9 = (Screen = new ScreenManager());
			AddSystem(system9);
			InventoryManager system10 = (InventoryManager = new InventoryManager());
			AddSystem(system10);
			SpawnManager system11 = (SpawnManager = new SpawnManager());
			AddSystem(system11);
			LocalizationManager system12 = (Localization = new LocalizationManager());
			AddSystem(system12);
			DialogManager system13 = (Dialog = new DialogManager());
			AddSystem(system13);
			Cinematics system14 = (Cinematics = new Cinematics());
			AddSystem(system14);
			ExceptionHandler system15 = (ExceptionHandler = new ExceptionHandler());
			AddSystem(system15);
			SkillManager system16 = (SkillManager = new SkillManager());
			AddSystem(system16);
			MapManager system17 = (MapManager = new MapManager());
			AddSystem(system17);
			NewMapManager system18 = (NewMapManager = new NewMapManager());
			AddSystem(system18);
			CamerasManager system19 = (CamerasManager = new CamerasManager());
			AddSystem(system19);
			GuiltManager system20 = (GuiltManager = new GuiltManager());
			AddSystem(system20);
			DLCManager system21 = (DLCManager = new DLCManager());
			AddSystem(system21);
			AchievementsManager system22 = (AchievementsManager = new AchievementsManager());
			AddSystem(system22);
			ColorPaletteManager system23 = (ColorPaletteManager = new ColorPaletteManager());
			AddSystem(system23);
			TutorialManager system24 = (TutorialManager = new TutorialManager());
			AddSystem(system24);
			ControlRemapManager system25 = (ControlRemapManager = new ControlRemapManager());
			AddSystem(system25);
			GameModeManager system26 = (GameModeManager = new GameModeManager());
			AddSystem(system26);
			PatchNotesManager system27 = (PatchNotesManager = new PatchNotesManager());
			AddSystem(system27);
			PenitenceManager system28 = (PenitenceManager = new PenitenceManager());
			AddSystem(system28);
			SharedCommands system29 = (SharedCommands = new SharedCommands());
			AddSystem(system29);
			AlmsManager system30 = (Alms = new AlmsManager());
			AddSystem(system30);
			BossRushManager system31 = (BossRushManager = new BossRushManager());
			AddSystem(system31);
			DemakeManager system32 = (DemakeManager = new DemakeManager());
			AddSystem(system32);
			preinit = true;
		}
	}

	public void Initialize()
	{
		if (!ready)
		{
			if (!preinit)
			{
				PreInit();
			}
			for (int i = 0; i < systems.Count; i++)
			{
				systems[i].AllPreInitialized();
			}
			for (int j = 0; j < systems.Count; j++)
			{
				systems[j].AllInitialized();
			}
			Log.Raw("============================================== (Framework Ready)");
			ready = true;
		}
	}

	public void SetDebug(string system, bool value)
	{
		foreach (GameSystem system2 in systems)
		{
			if (system2.GetType().Name.ToUpper() == system.ToUpper())
			{
				system2.ShowDebug = value;
			}
		}
	}

	public List<string> GetSystemsId()
	{
		return systems.ConvertAll((GameSystem x) => x.GetType().Name);
	}

	private void AddSystem(GameSystem system)
	{
		systems.Add(system);
		system.Initialize();
	}

	private void ReadParameters()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i] == "-console")
			{
				debugMode = true;
			}
		}
	}

	private void Awake()
	{
		for (int i = 0; i < systems.Count; i++)
		{
			systems[i].Awake();
		}
	}

	private void Start()
	{
		for (int i = 0; i < systems.Count; i++)
		{
			systems[i].Start();
		}
	}

	private void Update()
	{
		if (ready)
		{
			for (int i = 0; i < systems.Count; i++)
			{
				systems[i].Update();
			}
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < systems.Count; i++)
		{
			systems[i].Dispose();
		}
	}

	private void OnGUI()
	{
		for (int i = 0; i < systems.Count; i++)
		{
			if (systems[i].ShowDebug)
			{
				systems[i].OnGUI();
			}
		}
	}
}
