using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class CupheadStartScene : AbstractMonoBehaviour
{
	private bool settingsDataLoaded;

	protected override void Awake()
	{
		Application.targetFrameRate = 60;
		Cuphead.Init(lightInit: true);
	}

	private void Start()
	{
		StartCoroutine(start_cr());
	}

	private IEnumerator start_cr()
	{
		yield return null;
		yield return null;
		AssetLoader<Texture2D[]>.LoadAssetSynchronous("screen_fx", AssetLoaderOption.DontDestroyOnUnload());
		Object.FindObjectOfType<ChromaticAberrationFilmGrain>().Initialize(AssetLoader<Texture2D[]>.GetCachedAsset("screen_fx"));
		if (PlatformHelper.ForceAdditionalHeapMemory)
		{
			HeapAllocator.Allocate(100);
			yield return null;
			yield return null;
		}
		if (PlatformHelper.PreloadSettingsData)
		{
			OnlineManager.Instance.Init();
			SettingsData.LoadFromCloud(OnSettingsDataLoaded);
			while (!settingsDataLoaded)
			{
				yield return null;
			}
		}
		StartScreen.InitialLoadData startScreenLoadData = new StartScreen.InitialLoadData();
		PlatformHandlingTitleScreenOverride titleScreenOverride2 = new PlatformHandlingTitleScreenOverride(startScreenLoadData);
		yield return StartCoroutine(titleScreenOverride2.GetTitleScreenOverrideStatus_cr(this));
		StartScreen.initialLoadData = startScreenLoadData;
		titleScreenOverride2 = null;
		Coroutine[] fontCoroutines = FontLoader.Initialize();
		Coroutine[] array = fontCoroutines;
		for (int i = 0; i < array.Length; i++)
		{
			yield return array[i];
		}
		while (AssetBundleLoader.loadCounter > 0 || !AssetLoader<SpriteAtlas>.persistentAssetsLoaded || !AssetLoader<AudioClip>.persistentAssetsLoaded || !AssetLoader<Texture2D[]>.persistentAssetsLoaded)
		{
			yield return null;
		}
		yield return null;
		Cuphead.Init();
		yield return new WaitForSeconds(0.1f);
		DLCManager.RefreshDLC();
		yield return null;
		yield return null;
		Coroutine[] coroutines = DLCManager.LoadPersistentAssets();
		if (coroutines != null)
		{
			Coroutine[] array2 = coroutines;
			for (int j = 0; j < array2.Length; j++)
			{
				yield return array2[j];
			}
			yield return null;
			yield return null;
		}
		string titleSceneName = "scene_title";
		string[] preloadAtlases = AssetLoader<SpriteAtlas>.GetPreloadAssetNames(titleSceneName);
		string[] array3 = preloadAtlases;
		foreach (string atlas in array3)
		{
			yield return AssetLoader<SpriteAtlas>.LoadAsset(atlas, AssetLoaderOption.None());
		}
		string[] preloadMusic = AssetLoader<AudioClip>.GetPreloadAssetNames(titleSceneName);
		string[] array4 = preloadMusic;
		foreach (string clip in array4)
		{
			yield return AssetLoader<AudioClip>.LoadAsset(clip, AssetLoaderOption.None());
		}
		yield return null;
		yield return null;
		SceneManager.LoadSceneAsync(1);
	}

	private void OnSettingsDataLoaded(bool success)
	{
		if (!success)
		{
			SettingsData.LoadFromCloud(OnSettingsDataLoaded);
		}
		else
		{
			settingsDataLoaded = true;
		}
	}
}
