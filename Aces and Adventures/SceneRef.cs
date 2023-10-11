using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "ScriptableObject/SceneRef")]
public class SceneRef : ScriptableObject, IEquatable<SceneRef>, IComparable<SceneRef>
{
	private const string RESOURCE_PATH = "SceneRefs/";

	private static readonly string UI_PATH = "SceneRefs/UI/";

	private static readonly string UI_SUBSCENE_PATH = UI_PATH + "SubScene/";

	public static readonly string UI_CREATION_PATH = UI_PATH + "Creation/";

	public static readonly string UI_CREATION_SCENE_PATH = "UI/Creation/";

	private const string RESOURCE_POST = " Ref";

	private const string DONT_DESTROY_ON_LOAD = "DontDestroyOnLoad";

	private static SceneRef[] _ScenesInBuild;

	private static HashSet<SceneRef> _ScenesBeingLoaded;

	private static Dictionary<Type, SceneRef> _DataRefCreationScenesByType;

	private static List<SceneRef> _CreationSceneRefs;

	private static List<ADataRefPref> _CreationBreadCrumbs;

	private static SceneRef _Launch;

	private static SceneRef _Game;

	private static SceneRef _GameUI;

	private static SceneRef _GameGraph;

	private static SceneRef _MainMenu;

	private static SceneRef _CampaignEnd;

	private static SceneRef _CharmCollection;

	private static SceneRef _CharmCreation;

	private static SceneRef _AbilityCreation;

	private static SceneRef _GameCreation;

	private static SceneRef _ContentManager;

	private static SceneRef _PreviousActiveSceneRef;

	private static readonly string[] _CreationSplit = new string[1] { "Creation" };

	[SerializeField]
	[HideInInspector]
	private string _path;

	[SerializeField]
	private bool _isSubScene;

	[SerializeField]
	private string _nameOverride;

	[SerializeField]
	private bool _hideInSceneSelect;

	[SerializeField]
	[EnumFlags]
	private SceneRefVisibility _visibility = (SceneRefVisibility)(-1);

	private int? _index;

	private string _sceneName;

	private string _directory;

	private static SceneRef[] ScenesInBuild => _ScenesInBuild ?? (_ScenesInBuild = new SceneRef[SceneManager.sceneCountInBuildSettings]);

	private static HashSet<SceneRef> ScenesBeingLoaded => _ScenesBeingLoaded ?? (_ScenesBeingLoaded = new HashSet<SceneRef>());

	public static Dictionary<Type, SceneRef> DataRefCreationScenesByType
	{
		get
		{
			if (_DataRefCreationScenesByType == null)
			{
				_DataRefCreationScenesByType = ReflectionUtil.GetTypesWhichImplementInterface<IDataContent>().ToDictionary((Type t) => t, (Type t) => Resources.Load<SceneRef>(UI_CREATION_PATH + t.Name.RemoveFromEnd(4).FriendlyFromCamelOrPascalCase() + " Creation Ref")).RemoveValues((SceneRef s) => !s);
			}
			return _DataRefCreationScenesByType;
		}
	}

	public static List<SceneRef> CreationSceneRefs => _CreationSceneRefs ?? (_CreationSceneRefs = new List<SceneRef>(DataRefCreationScenesByType.Values.OrderBy((SceneRef s) => s.sceneName)).AddReturnList(ContentManager).AddReturnListIf(CharmCreation, Application.isEditor));

	public static List<ADataRefPref> CreationBreadCrumbs => _CreationBreadCrumbs ?? (_CreationBreadCrumbs = new List<ADataRefPref>());

	public static SceneRef Launch
	{
		get
		{
			if (!_Launch)
			{
				return _Launch = Resources.Load<SceneRef>("SceneRefs/Launch Ref");
			}
			return _Launch;
		}
	}

	public static SceneRef Game
	{
		get
		{
			if (!_Game)
			{
				return _Game = Resources.Load<SceneRef>("SceneRefs/GameStateManager Ref");
			}
			return _Game;
		}
	}

	public static SceneRef GameUI
	{
		get
		{
			if (!_GameUI)
			{
				return _GameUI = Resources.Load<SceneRef>(UI_SUBSCENE_PATH + "Game ScreenSpaceUI Ref");
			}
			return _GameUI;
		}
	}

	public static SceneRef GameGraph
	{
		get
		{
			if (!_GameGraph)
			{
				return _GameGraph = Resources.Load<SceneRef>("SceneRefs/Game Graph Ref");
			}
			return _GameGraph;
		}
	}

	public static SceneRef MainMenu
	{
		get
		{
			if (!_MainMenu)
			{
				return _MainMenu = Resources.Load<SceneRef>("SceneRefs/Main Menu Ref");
			}
			return _MainMenu;
		}
	}

	public static SceneRef CampaignEnd
	{
		get
		{
			if (!_CampaignEnd)
			{
				return _CampaignEnd = Resources.Load<SceneRef>("SceneRefs/Campaign End Ref");
			}
			return _CampaignEnd;
		}
	}

	public static SceneRef CharmCollection
	{
		get
		{
			if (!_CharmCollection)
			{
				return _CharmCollection = Resources.Load<SceneRef>(UI_CREATION_PATH + "Charm Collection Ref");
			}
			return _CharmCollection;
		}
	}

	public static SceneRef CharmCreation
	{
		get
		{
			if (!_CharmCreation)
			{
				return _CharmCreation = Resources.Load<SceneRef>(UI_CREATION_PATH + "Charm Creation Ref");
			}
			return _CharmCreation;
		}
	}

	public static SceneRef AbilityCreation
	{
		get
		{
			if (!_AbilityCreation)
			{
				return _AbilityCreation = Resources.Load<SceneRef>(UI_CREATION_PATH + "Ability Creation Ref");
			}
			return _AbilityCreation;
		}
	}

	public static SceneRef GameCreation
	{
		get
		{
			if (!_GameCreation)
			{
				return _GameCreation = Resources.Load<SceneRef>(UI_CREATION_PATH + "Game Creation Ref");
			}
			return _GameCreation;
		}
	}

	public static SceneRef ContentManager
	{
		get
		{
			if (!_ContentManager)
			{
				return _ContentManager = Resources.Load<SceneRef>(UI_CREATION_PATH + "Content Manager Ref");
			}
			return _ContentManager;
		}
	}

	public static SceneRef ActiveScene => SceneManager.GetActiveScene();

	public string path => _path;

	public string sceneName
	{
		get
		{
			if (!_nameOverride.IsNullOrEmpty())
			{
				return _nameOverride;
			}
			if (!_sceneName.IsNullOrEmpty())
			{
				return _sceneName;
			}
			return _sceneName = Path.GetFileNameWithoutExtension(path);
		}
	}

	public string directory
	{
		get
		{
			if (!_directory.IsNullOrEmpty())
			{
				return _directory;
			}
			return _directory = Path.GetDirectoryName(path);
		}
	}

	public int buildIndex
	{
		get
		{
			return _index ?? (buildIndex = SceneUtility.GetBuildIndexByScenePath(path));
		}
		private set
		{
			if (SetPropertyUtility.SetStruct(ref _index, value) && buildIndex >= 0 && buildIndex < ScenesInBuild.Length)
			{
				ScenesInBuild[buildIndex] = this;
			}
		}
	}

	public bool isSubScene => _isSubScene;

	public bool hideInSceneSelect
	{
		get
		{
			if (!_hideInSceneSelect)
			{
				return !_visibility.Visible();
			}
			return true;
		}
	}

	public bool isActive
	{
		get
		{
			if (((Scene)this).IsValid())
			{
				return ((Scene)this).isLoaded;
			}
			return false;
		}
	}

	public bool isLoading => ScenesBeingLoaded.Contains(this);

	public static event Action<SceneRef, LoadSceneMode> OnSceneBeginLoad;

	public static event Action<Scene, LoadSceneMode> OnSceneLoaded;

	public static event Action<Scene> OnSceneBeginUnload;

	public static event Action<Scene> OnSceneUnloaded;

	public static event Action<SceneRef, SceneRef> OnActiveSceneChanged;

	public static event Action<SceneRef, SceneRef> OnSceneTransitionBegin;

	public static event Action<SceneRef, SceneRef> OnSceneTransition;

	public static event Action<SceneRef, SceneRef> OnSceneTransitionFinish;

	public static void AddCreationBreadCrumb(ADataRefPref dataRefPref)
	{
		if (dataRefPref != null)
		{
			CreationBreadCrumbs.Add(ProtoUtil.Clone(dataRefPref));
		}
	}

	public static void RemoveCreationBreadCrumb(ADataRefPref dataRefPref)
	{
		CreationBreadCrumbs.Remove(dataRefPref);
	}

	public static void ClearCreationBreadCrumbs()
	{
		_CreationBreadCrumbs = null;
		UIGeneratorType.RemovePersistedData((Type type) => type.ImplementsInterface(typeof(IDataContent)));
		WebRequestTextureCache.ClearCache();
		Steam.Ugc.ClearAuthorFileCache();
		Steam.Friends.ClearNameCache();
		Steam.Ugc.ClearUserVoteCache();
	}

	public static SceneRef GetCreationBackSceneRef(bool removeFromBreadCrumbs)
	{
		if (CreationBreadCrumbs.Count <= 0)
		{
			_ = MainMenu;
		}
		else
		{
			_ = DataRefCreationScenesByType[CreationBreadCrumbs.LastRef().dataType];
		}
		if (removeFromBreadCrumbs && CreationBreadCrumbs.Count > 0)
		{
			CreationBreadCrumbs.RemoveLast();
		}
		return MainMenu;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeMethodLoad()
	{
		SceneManager.sceneLoaded += _SignalOnSceneLoad;
		SceneManager.sceneUnloaded += _SignalSceneUnload;
		SceneManager.activeSceneChanged += _SignalOnActiveSceneChanged;
		OnSceneBeginLoad += _TrackScenesBeingLoadedBegin;
		OnSceneLoaded += _TrackScenesBeingLoadedEnd;
		Resources.LoadAll<SceneRef>("SceneRefs/");
	}

	private static void _SignalOnSceneBeginLoad(SceneRef sceneBeginningToLoad, LoadSceneMode loadMode)
	{
		if (SceneRef.OnSceneBeginLoad != null)
		{
			SceneRef.OnSceneBeginLoad(sceneBeginningToLoad, loadMode);
		}
	}

	private static void _SignalOnSceneLoad(Scene loadedScene, LoadSceneMode loadMode)
	{
		if (SceneRef.OnSceneLoaded != null)
		{
			SceneRef.OnSceneLoaded(loadedScene, loadMode);
		}
	}

	private static void _SignalOnSceneBeginUnload(Scene sceneBeginningToUnload)
	{
		if (SceneRef.OnSceneBeginUnload != null)
		{
			SceneRef.OnSceneBeginUnload(sceneBeginningToUnload);
		}
	}

	private static void _SignalSceneUnload(Scene unloadedScene)
	{
		if (SceneRef.OnSceneUnloaded != null)
		{
			SceneRef.OnSceneUnloaded(unloadedScene);
		}
	}

	private static void _SignalOnActiveSceneChanged(Scene previousActiveScene, Scene activeScene)
	{
		if (SceneRef.OnActiveSceneChanged != null)
		{
			SceneRef.OnActiveSceneChanged(_PreviousActiveSceneRef ? _PreviousActiveSceneRef : ((SceneRef)activeScene), activeScene);
		}
		_PreviousActiveSceneRef = activeScene;
	}

	private static void _SignalOnBeginSceneTransition(SceneRef currentActiveScene, SceneRef nextActiveScene)
	{
		if (SceneRef.OnSceneTransitionBegin != null)
		{
			SceneRef.OnSceneTransitionBegin(currentActiveScene, nextActiveScene);
		}
		Action<Scene> onSceneUnloaded = null;
		onSceneUnloaded = delegate(Scene scene)
		{
			if (!(scene != currentActiveScene))
			{
				if (SceneRef.OnSceneTransition != null)
				{
					SceneRef.OnSceneTransition(currentActiveScene, nextActiveScene);
				}
				OnSceneUnloaded -= onSceneUnloaded;
			}
		};
		OnSceneUnloaded += onSceneUnloaded;
		Action<Scene, LoadSceneMode> onSceneLoaded = null;
		onSceneLoaded = delegate(Scene scene, LoadSceneMode mode)
		{
			if (!(scene != nextActiveScene))
			{
				if (SceneRef.OnSceneTransitionFinish != null)
				{
					SceneRef.OnSceneTransitionFinish(currentActiveScene, nextActiveScene);
				}
				OnSceneLoaded -= onSceneLoaded;
			}
		};
		OnSceneLoaded += onSceneLoaded;
	}

	private static IEnumerable<Scene> _GetScenesThatWouldBeUnloadedBySingleSceneLoad()
	{
		return from scene in GetActiveScenes()
			where scene.name != "DontDestroyOnLoad"
			select scene;
	}

	private static void _TrackScenesBeingLoadedBegin(SceneRef sceneBeginningToLoad, LoadSceneMode loadMode)
	{
		ScenesBeingLoaded.Add(sceneBeginningToLoad);
	}

	private static void _TrackScenesBeingLoadedEnd(Scene loadedScene, LoadSceneMode loadMode)
	{
		ScenesBeingLoaded.Remove(loadedScene);
	}

	private static SceneRef _GetFromBuildIndex(int buildIndex)
	{
		if (buildIndex >= 0 && buildIndex <= ScenesInBuild.Length - 1)
		{
			if (!ScenesInBuild[buildIndex])
			{
				return ScenesInBuild[buildIndex] = ScriptableObject.CreateInstance<SceneRef>()._SetData(buildIndex);
			}
			return ScenesInBuild[buildIndex];
		}
		return null;
	}

	public static IEnumerable<Scene> GetActiveScenes()
	{
		int activeSceneCount = SceneManager.sceneCount;
		for (int x = 0; x < activeSceneCount; x++)
		{
			yield return SceneManager.GetSceneAt(x);
		}
	}

	public static IEnumerable<SceneRef> GetAllScenesInBuild(bool includeSubScenes = true)
	{
		for (int x = 0; x < ScenesInBuild.Length; x++)
		{
			if (includeSubScenes || !_GetFromBuildIndex(x).isSubScene)
			{
				yield return _GetFromBuildIndex(x);
			}
		}
	}

	public static IEnumerable<SceneRef> GetAllScenesInDirectory(string directory, bool includeSubScenes = true, bool includeSubDirectories = true)
	{
		foreach (SceneRef item in GetAllScenesInBuild(includeSubScenes))
		{
			if (item.directory.Contains(directory) && (includeSubDirectories || item.directory == directory))
			{
				yield return item;
			}
		}
	}

	public static void DisableEventSystemUntilSceneTransition()
	{
		Action<SceneRef, SceneRef> onSceneTransition = null;
		object disableRequest = new object();
		InputManager.SetEventSystemEnabled(disableRequest, enabled: false);
		onSceneTransition = delegate
		{
			InputManager.SetEventSystemEnabled(disableRequest, enabled: true);
			OnSceneTransitionFinish -= onSceneTransition;
		};
		OnSceneTransitionFinish += onSceneTransition;
	}

	public static string RemoveCreationFromSceneName(SceneRef sceneRef)
	{
		return sceneRef.sceneName.Split(_CreationSplit, StringSplitOptions.None)[0];
	}

	private void OnEnable()
	{
		_ = buildIndex;
	}

	private void _ProcessLoad(LoadSceneMode loadMode)
	{
		if (loadMode == LoadSceneMode.Single)
		{
			foreach (Scene item in _GetScenesThatWouldBeUnloadedBySingleSceneLoad())
			{
				_SignalOnSceneBeginUnload(item);
			}
			_SignalOnBeginSceneTransition(SceneManager.GetActiveScene(), this);
		}
		_SignalOnSceneBeginLoad(this, loadMode);
	}

	private void _ProcessUnload()
	{
		_SignalOnSceneBeginUnload(this);
	}

	private IEnumerator _WaitUntilActive()
	{
		while (!isActive)
		{
			yield return null;
		}
	}

	private SceneRef _SetData(int sceneBuildIndex)
	{
		_path = SceneUtility.GetScenePathByBuildIndex(sceneBuildIndex);
		buildIndex = sceneBuildIndex;
		return this;
	}

	public IEnumerator LoadAsync(LoadSceneMode loadMode)
	{
		if (isLoading)
		{
			return _WaitUntilActive();
		}
		_ProcessLoad(loadMode);
		return SceneManager.LoadSceneAsync(buildIndex, loadMode).WaitForCompletion();
	}

	public AsyncOperation Unload()
	{
		_ProcessUnload();
		return SceneManager.UnloadSceneAsync(buildIndex);
	}

	public static bool operator ==(SceneRef a, SceneRef b)
	{
		if ((bool)a && (bool)b)
		{
			return a.buildIndex == b.buildIndex;
		}
		return false;
	}

	public static bool operator !=(SceneRef a, SceneRef b)
	{
		return !(a == b);
	}

	public static bool operator ==(SceneRef sceneRef, Scene scene)
	{
		if ((bool)sceneRef)
		{
			return sceneRef.buildIndex == scene.buildIndex;
		}
		return false;
	}

	public static bool operator !=(SceneRef sceneRef, Scene scene)
	{
		return !(sceneRef == scene);
	}

	public static bool operator ==(Scene scene, SceneRef sceneRef)
	{
		return sceneRef == scene;
	}

	public static bool operator !=(Scene scene, SceneRef sceneRef)
	{
		return !(sceneRef == scene);
	}

	public static implicit operator Scene(SceneRef sceneRef)
	{
		return SceneManager.GetSceneByBuildIndex(sceneRef.buildIndex);
	}

	public static implicit operator SceneRef(Scene scene)
	{
		return _GetFromBuildIndex(scene.buildIndex);
	}

	public override string ToString()
	{
		return sceneName;
	}

	public bool Equals(SceneRef other)
	{
		return this == other;
	}

	public int CompareTo(SceneRef other)
	{
		return buildIndex - other.buildIndex;
	}

	public override bool Equals(object other)
	{
		if (other is SceneRef)
		{
			return ((SceneRef)other).buildIndex == buildIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return buildIndex;
	}
}
