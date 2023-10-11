using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class GameManager : MonoBehaviour
{
	[SerializeField]
	protected Light _mainLight;

	[SerializeField]
	protected Light _spotLight;

	[Header("States")]
	public AdventureStateView adventureState;

	public DeckStateView deckState;

	public LevelStateView levelUpState;

	[Header("Establishing Shot")]
	public PlayableDirector startToEstablish;

	public PlayableDirector establishShotToGame;

	public PlayableDirector establishShotToDeck;

	public PlayableDirector establishShotToLevelUp;

	[Header("Virtual Cameras")]
	public CinemachineVirtualCamera startCamera;

	public CinemachineVirtualCamera establishCamera;

	public CinemachineVirtualCamera adventureCamera;

	public CinemachineVirtualCamera deckCamera;

	public CinemachineVirtualCamera levelUpCamera;

	[Header("Camera Tracks")]
	public CinemachineSmoothPath adventureTrack;

	public CinemachineSmoothPath deckTrack;

	public CinemachineSmoothPath levelUpTrack;

	[Header("Camera Look At")]
	public Transform cameraLookAt;

	public Transform adventureLookAt;

	public Transform deckLookAt;

	public Transform levelUpLookAt;

	[Header("Addressable")]
	public AssetReference cosmeticScene;

	public AssetReference deckCreationView;

	public GameObject deckCreationViewBlueprint;

	[Header("Events")]
	public BoolEvent onDeckCreationEnabledChange;

	[Header("Talismans")]
	public GameObject warriorTalisman;

	public GameObject rogueTalisman;

	public GameObject mageTalisman;

	public GameObject hunterTalisman;

	public GameObject enchantressTalisman;

	private GameStepStack _stack;

	private List<CinemachineVirtualCameraBase> _virtualCameras;

	private Dictionary<AssetReference, AsyncOperationHandle<SceneInstance>> _loadedSceneHandleMap = new Dictionary<AssetReference, AsyncOperationHandle<SceneInstance>>();

	public static GameManager Instance { get; private set; }

	public GameStepStack stack => _stack ?? (_stack = new GameStepStack().Register());

	public Light mainLight => _mainLight ?? (_mainLight = (from l in Object.FindObjectsOfType<Light>()
		where l.type == LightType.Directional
		select l).MaxBy((Light l) => l.intensity));

	public Light spotLight => _spotLight;

	public List<CinemachineVirtualCameraBase> virtualCameras => _virtualCameras ?? (_virtualCameras = new List<CinemachineVirtualCameraBase>(_GetVirtualCameras()));

	public GameObject this[PlayerClass c] => c switch
	{
		PlayerClass.Warrior => warriorTalisman, 
		PlayerClass.Rogue => rogueTalisman, 
		PlayerClass.Mage => mageTalisman, 
		PlayerClass.Hunter => hunterTalisman, 
		_ => enchantressTalisman, 
	};

	private IEnumerable<CinemachineVirtualCameraBase> _GetVirtualCameras()
	{
		if ((bool)startCamera)
		{
			yield return startCamera;
		}
		if ((bool)establishCamera)
		{
			yield return establishCamera;
		}
		if ((bool)adventureCamera)
		{
			yield return adventureCamera;
		}
		if ((bool)deckCamera)
		{
			yield return deckCamera;
		}
		if ((bool)levelUpCamera)
		{
			yield return levelUpCamera;
		}
	}

	private void _PushLoadGameStateSteps(GameState gameState)
	{
		Debug.Log("Pushing Load GameState");
		stack.Push(new GameStepGroupTransitionToAdventureTable());
		stack.Push(new GameStepGenericSimple(delegate
		{
			GameStateView.CreateAdventureView().state = gameState;
		}));
		stack.Push(new GameStepGenericSimple(gameState.PushOnLoadGameSteps));
		stack.Push(new GameStepWaitFrame());
		stack.Push(new GameStepGenericSimple(delegate
		{
			SplashScreenView.Instance?.FinishDelayed(1f);
		}));
	}

	private void _PushStartupSteps()
	{
		Debug.Log("Pushing Startup");
		stack.Push(new GameStepLoadSceneAsync(cosmeticScene));
		stack.Push(new GameStepGeneric
		{
			onStart = GameState.LoadStartupResources
		});
		stack.Push(new GameStepSetupEnvironmentRendering());
		stack.Push(new GameStepWaitFrame(7));
		stack.Push(new GameStepGeneric
		{
			onStart = delegate
			{
				SplashScreenView.Instance?.Finish();
			}
		});
		stack.Push(new GameStepGroupSetupEnvironmentCosmetics
		{
			music = false
		});
		stack.Push(new GameStepGeneric
		{
			onStart = delegate
			{
				if (!ProfileManager.progress.openingPlayed)
				{
					stack.Push(new GameStepOpeningSequence());
				}
				stack.Push(new GameStepGroupSetupEnvironmentCosmetics());
				stack.Push(new GameStepEstablishShot());
			}
		});
	}

	private async void Awake()
	{
		ProfileManager.progress.games.read.CheckForRewardUnlocks();
		await AchievementData.RegisterAll();
		await Steam.Stats.UnlockAchievements(ProfileManager.progress.GetAchievements());
	}

	private void OnEnable()
	{
		Instance = this;
	}

	private void Start()
	{
		stack.Push(new GameStepInstantiate(deckCreationViewBlueprint, null, null, releaseAfterInstantiate: true));
		GameState gameState = ProfileManager.progress.LoadActiveRun();
		if (gameState != null)
		{
			_PushLoadGameStateSteps(gameState);
		}
		else
		{
			_PushStartupSteps();
		}
	}

	private void Update()
	{
		stack.Update();
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	public void SetActiveVirtualCamera(CinemachineVirtualCameraBase virtualCamera)
	{
		virtualCamera.gameObject.SetActive(value: true);
		foreach (CinemachineVirtualCameraBase virtualCamera2 in virtualCameras)
		{
			if (virtualCamera2 != virtualCamera)
			{
				virtualCamera2.gameObject.SetActive(value: false);
			}
		}
		Camera.main.GetComponent<CinemachineBrain>().RefreshEnabled();
	}

	public void AddLoadedSceneHandle(AssetReference assetReference, AsyncOperationHandle<SceneInstance> sceneInstanceHandle)
	{
		_loadedSceneHandleMap[assetReference] = sceneInstanceHandle;
	}

	public AsyncOperationHandle<SceneInstance> RemoveLoadedSceneHandle(AssetReference assetReference)
	{
		AsyncOperationHandle<SceneInstance> valueOrDefault = _loadedSceneHandleMap.GetValueOrDefault(assetReference);
		_loadedSceneHandleMap.Remove(assetReference);
		return valueOrDefault;
	}

	public bool IsAddressableSceneLoaded(AssetReference assetReference)
	{
		return _loadedSceneHandleMap?.ContainsKey(assetReference) ?? false;
	}
}
