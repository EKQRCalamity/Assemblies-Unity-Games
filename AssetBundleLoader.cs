using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.U2D;

public class AssetBundleLoader : MonoBehaviour
{
	private enum AssetBundleLocation
	{
		StreamingAssets,
		DLC
	}

	private class AssetBundleContainer
	{
		public AssetBundle assetBundle;

		public AssetBundleLocation location;

		public AssetBundleContainer(AssetBundle assetBundle, AssetBundleLocation location)
		{
			this.assetBundle = assetBundle;
			this.location = location;
		}
	}

	public static readonly string AssetBundlePrefixSpriteAtlas = "atlas_";

	public static readonly string AssetBundlePrefixMusic = "music_";

	public static readonly string AssetBundlePrefixFont = "font_";

	public static readonly string AssetBundlePrefixTMPFont = "tmpfont_";

	public static readonly string AssetBundlePrefixTexture = "tex_";

	private static AssetBundleLoader Instance;

	[SerializeField]
	private AssetLocationDatabase atlasLocationDatabase;

	[SerializeField]
	private AssetLocationDatabase musicLocationDatabase;

	private Dictionary<string, AssetBundleContainer> loadedBundles = new Dictionary<string, AssetBundleContainer>();

	public static int loadCounter { get; private set; }

	private void Awake()
	{
		if (Instance != null)
		{
			throw new Exception("Should only be one instance");
		}
		Instance = this;
	}

	public static void UnloadAssetBundles()
	{
		foreach (KeyValuePair<string, AssetBundleContainer> loadedBundle in Instance.loadedBundles)
		{
			loadedBundle.Value.assetBundle.Unload(unloadAllLoadedObjects: false);
		}
		Instance.loadedBundles.Clear();
	}

	public static Coroutine LoadSpriteAtlas(string atlasName, Action<SpriteAtlas> completionHandler)
	{
		AssetBundleLocation location = AssetBundleLocation.StreamingAssets;
		if (Instance.atlasLocationDatabase.dlcAssets.Contains(atlasName))
		{
			location = AssetBundleLocation.DLC;
		}
		string spriteAtlasBundleName = GetSpriteAtlasBundleName(atlasName);
		return Instance.StartCoroutine(Instance.loadAsset(spriteAtlasBundleName, location, atlasName, completionHandler));
	}

	public static Coroutine LoadMusic(string audioClipName, Action<AudioClip> completionHandler)
	{
		AssetBundleLocation location = AssetBundleLocation.StreamingAssets;
		if (Instance.musicLocationDatabase.dlcAssets.Contains(audioClipName))
		{
			location = AssetBundleLocation.DLC;
		}
		string musicBundleName = GetMusicBundleName(audioClipName);
		return Instance.StartCoroutine(Instance.loadAsset(musicBundleName, location, audioClipName, completionHandler));
	}

	public static Coroutine LoadFont(string bundleName, string assetName, Action<Font> completionHandler)
	{
		AssetBundleLocation location = AssetBundleLocation.StreamingAssets;
		bundleName = AssetBundlePrefixFont + bundleName.ToLowerInvariant();
		return Instance.StartCoroutine(Instance.loadAsset(bundleName, location, assetName, completionHandler));
	}

	public static Coroutine LoadTMPFont(string bundleName, Action<UnityEngine.Object[]> completionHandler)
	{
		AssetBundleLocation location = AssetBundleLocation.StreamingAssets;
		bundleName = AssetBundlePrefixTMPFont + bundleName.ToLowerInvariant();
		return Instance.StartCoroutine(Instance.loadAllAssets(bundleName, location, completionHandler));
	}

	public static Coroutine LoadTextures(string bundleName, Action<Texture2D[]> completionHandler)
	{
		AssetBundleLocation location = AssetBundleLocation.StreamingAssets;
		bundleName = AssetBundlePrefixTexture + bundleName.ToLowerInvariant();
		return Instance.StartCoroutine(Instance.loadAllAssets(bundleName, location, completionHandler));
	}

	public static Texture2D[] LoadTexturesSynchronous(string bundleName)
	{
		AssetBundleLocation location = AssetBundleLocation.StreamingAssets;
		bundleName = AssetBundlePrefixTexture + bundleName.ToLowerInvariant();
		return Instance.loadAllAssetsSynchronous<Texture2D>(bundleName, location);
	}

	private IEnumerator loadAssetBundle(string assetBundleName, AssetBundleLocation location)
	{
		loadCounter++;
		string path3 = getBasePath(location);
		path3 = Path.Combine(path3, "AssetBundles");
		path3 = Path.Combine(path3, assetBundleName);
		AssetBundle assetBundle;
		if (location == AssetBundleLocation.DLC && DLCManager.UsesAlternateBundleLoadingMechanism())
		{
			DLCManager.AssetBundleLoadWaitInstruction waitInstruction = DLCManager.LoadAssetBundle(path3);
			yield return waitInstruction;
			assetBundle = waitInstruction.assetBundle;
		}
		else
		{
			AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path3);
			yield return request;
			assetBundle = request.assetBundle;
		}
		loadedBundles.Add(assetBundleName, new AssetBundleContainer(assetBundle, location));
		loadCounter--;
	}

	private AssetBundleContainer loadAssetBundleSynchronous(string assetBundleName, AssetBundleLocation location)
	{
		string basePath = getBasePath(location);
		basePath = Path.Combine(basePath, "AssetBundles");
		basePath = Path.Combine(basePath, assetBundleName);
		AssetBundle assetBundle = AssetBundle.LoadFromFile(basePath);
		AssetBundleContainer assetBundleContainer = new AssetBundleContainer(assetBundle, location);
		loadedBundles.Add(assetBundleName, assetBundleContainer);
		return assetBundleContainer;
	}

	private IEnumerator loadAsset<T>(string assetBundleName, AssetBundleLocation location, string assetName, Action<T> completionHandler) where T : UnityEngine.Object
	{
		loadCounter++;
		if (!loadedBundles.TryGetValue(assetBundleName, out var assetBundleContainer))
		{
			yield return StartCoroutine(loadAssetBundle(assetBundleName, location));
			assetBundleContainer = loadedBundles[assetBundleName];
		}
		AssetBundleRequest assetRequest = assetBundleContainer.assetBundle.LoadAssetAsync<T>(assetName);
		yield return assetRequest;
		completionHandler(assetRequest.asset as T);
		if (assetBundleContainer.location == AssetBundleLocation.DLC && DLCManager.UnloadBundlesImmediately() && typeof(T) == typeof(SpriteAtlas))
		{
			loadedBundles.Remove(assetBundleContainer.assetBundle.name);
			assetBundleContainer.assetBundle.Unload(unloadAllLoadedObjects: false);
		}
		loadCounter--;
	}

	private IEnumerator loadAllAssets<T>(string assetBundleName, AssetBundleLocation location, Action<T[]> completionHandler) where T : UnityEngine.Object
	{
		loadCounter++;
		if (!loadedBundles.TryGetValue(assetBundleName, out var assetBundleContainer))
		{
			yield return StartCoroutine(loadAssetBundle(assetBundleName, location));
			assetBundleContainer = loadedBundles[assetBundleName];
		}
		AssetBundleRequest assetRequest = assetBundleContainer.assetBundle.LoadAllAssetsAsync<T>();
		yield return assetRequest;
		UnityEngine.Object[] allAssets = assetRequest.allAssets;
		T[] castAssets = new T[allAssets.Length];
		for (int i = 0; i < allAssets.Length; i++)
		{
			castAssets[i] = (T)allAssets[i];
		}
		completionHandler(castAssets);
		loadCounter--;
	}

	private T[] loadAllAssetsSynchronous<T>(string assetBundleName, AssetBundleLocation location) where T : UnityEngine.Object
	{
		if (!loadedBundles.TryGetValue(assetBundleName, out var value))
		{
			value = loadAssetBundleSynchronous(assetBundleName, location);
		}
		return value.assetBundle.LoadAllAssets<T>();
	}

	private static string getBasePath(AssetBundleLocation location)
	{
		if (location == AssetBundleLocation.DLC)
		{
			return DLCManager.AssetBundlePath();
		}
		return Application.streamingAssetsPath;
	}

	public static string GetSpriteAtlasBundleName(string atlasName)
	{
		return AssetBundlePrefixSpriteAtlas + atlasName.ToLowerInvariant();
	}

	public static string GetMusicBundleName(string audioClipName)
	{
		return AssetBundlePrefixMusic + audioClipName.ToLowerInvariant();
	}

	public static List<string> DEBUG_LoadedAssetBundles()
	{
		return new List<string>(Instance.loadedBundles.Keys);
	}
}
