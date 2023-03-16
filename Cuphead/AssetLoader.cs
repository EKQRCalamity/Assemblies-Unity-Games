using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AssetLoader<T> : MonoBehaviour where T : class
{
	private class AssetContainer<U>
	{
		public U asset;

		public AssetLoaderOption assetOption;

		public AssetContainer(U asset, AssetLoaderOption assetOption)
		{
			this.asset = asset;
			this.assetOption = assetOption;
		}
	}

	private class LoadOperation
	{
		public Coroutine coroutine;

		public List<Action<T>> completionHandlers = new List<Action<T>>();
	}

	protected static AssetLoader<T> Instance;

	[SerializeField]
	private RuntimeSceneAssetDatabase sceneAssetDatabase;

	[SerializeField]
	private AssetLocationDatabase assetLocationDatabase;

	private Dictionary<string, LoadOperation> loadOperations = new Dictionary<string, LoadOperation>();

	private Dictionary<string, AssetContainer<T>> loadedAssets = new Dictionary<string, AssetContainer<T>>();

	private bool _persistentAssetsLoaded;

	public static bool persistentAssetsLoaded
	{
		get
		{
			return Instance._persistentAssetsLoaded;
		}
		private set
		{
			Instance._persistentAssetsLoaded = value;
		}
	}

	private void Awake()
	{
		if (Instance != null)
		{
			throw new Exception("More than one instance found");
		}
		Instance = this;
	}

	private void Start()
	{
		StartCoroutine(loadPersistentAssets());
	}

	private IEnumerator loadPersistentAssets()
	{
		if (sceneAssetDatabase != null)
		{
			foreach (string assetName in sceneAssetDatabase.persistentAssets)
			{
				yield return loadAssetFromAssetBundle(assetName, AssetLoaderOption.PersistInCache(), null);
			}
		}
		persistentAssetsLoaded = true;
	}

	public static string[] GetPreloadAssetNames(string sceneName)
	{
		if (!Instance.sceneAssetDatabase.sceneAssetMappings.TryGetValue(sceneName, out var value))
		{
			return new string[0];
		}
		return value;
	}

	public static Coroutine LoadAsset(string assetName, AssetLoaderOption option)
	{
		return Instance.loadAssetFromAssetBundle(assetName, option, null);
	}

	public static T LoadAssetSynchronous(string assetName, AssetLoaderOption option)
	{
		return Instance.loadAssetFromAssetBundleSynchronous(assetName, option);
	}

	protected abstract Coroutine loadAsset(string assetName, Action<T> completionHandler);

	protected abstract T loadAssetSynchronous(string assetName);

	public static Coroutine LoadPersistentAssetsDLC()
	{
		return Instance.loadPersistentAssetsDLC();
	}

	private Coroutine loadPersistentAssetsDLC()
	{
		return StartCoroutine(loadPersistentAssetsDLC_cr());
	}

	private IEnumerator loadPersistentAssetsDLC_cr()
	{
		foreach (string assetName in sceneAssetDatabase.persistentAssetsDLC)
		{
			yield return loadAssetFromAssetBundle(assetName, AssetLoaderOption.PersistInCache(), null);
		}
	}

	public static T GetCachedAsset(string assetName)
	{
		if (Instance.tryGetAsset(assetName, out var asset))
		{
			return asset;
		}
		throw new Exception("Asset not cached: " + assetName);
	}

	public static void UnloadAssets(params string[] persistentTagsToUnload)
	{
		List<string> list = new List<string>(Instance.loadedAssets.Keys);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			AssetContainer<T> assetContainer = Instance.loadedAssets[list[num]];
			if ((assetContainer.assetOption.type & AssetLoaderOption.Type.PersistInCache) != 0)
			{
				list.RemoveAt(num);
			}
			else if ((assetContainer.assetOption.type & AssetLoaderOption.Type.PersistInCacheTagged) != 0 && Array.IndexOf(persistentTagsToUnload, (string)assetContainer.assetOption.context) < 0)
			{
				list.RemoveAt(num);
			}
			else if ((assetContainer.assetOption.type & AssetLoaderOption.Type.DontDestroyOnUnload) == 0)
			{
				Instance.destroyAsset(assetContainer.asset);
			}
		}
		foreach (string item in list)
		{
			Instance.loadedAssets.Remove(item);
		}
	}

	protected abstract void destroyAsset(T asset);

	private void cacheAsset(string assetName, AssetContainer<T> container)
	{
		loadedAssets.Add(assetName, container);
	}

	protected bool tryGetAsset(string assetName, out T asset)
	{
		asset = (T)null;
		if (loadedAssets.TryGetValue(assetName, out var value))
		{
			asset = value.asset;
			return true;
		}
		return false;
	}

	protected Coroutine loadAssetFromAssetBundle(string assetName, AssetLoaderOption option, Action<T> completionAction)
	{
		if (tryGetAsset(assetName, out var asset2))
		{
			completionAction?.Invoke(asset2);
			return null;
		}
		if (!DLCManager.DLCEnabled() && assetLocationDatabase.dlcAssets.Contains(assetName))
		{
			completionAction?.Invoke((T)null);
			return null;
		}
		if (!loadOperations.TryGetValue(assetName, out var value))
		{
			value = new LoadOperation();
			loadOperations.Add(assetName, value);
			value.coroutine = loadAsset(assetName, delegate(T asset)
			{
				cacheAsset(assetName, new AssetContainer<T>(asset, option));
				LoadOperation loadOperation = loadOperations[assetName];
				foreach (Action<T> completionHandler in loadOperation.completionHandlers)
				{
					completionHandler?.Invoke(asset);
				}
				loadOperations.Remove(assetName);
			});
		}
		value.completionHandlers.Add(completionAction);
		return value.coroutine;
	}

	protected T loadAssetFromAssetBundleSynchronous(string assetName, AssetLoaderOption option)
	{
		if (tryGetAsset(assetName, out var asset))
		{
			return asset;
		}
		T val = loadAssetSynchronous(assetName);
		cacheAsset(assetName, new AssetContainer<T>(val, option));
		return val;
	}

	public static bool IsDLCAsset(string assetName)
	{
		return Instance.assetLocationDatabase.dlcAssets.Contains(assetName);
	}

	public static List<string> DEBUG_GetLoadedAssets()
	{
		List<string> list = new List<string>(Instance.loadedAssets.Count);
		foreach (KeyValuePair<string, AssetContainer<T>> loadedAsset in Instance.loadedAssets)
		{
			AssetContainer<T> value = loadedAsset.Value;
			string text = $"{loadedAsset.Key} ({value.assetOption.type.ToString()})";
			if ((value.assetOption.type & AssetLoaderOption.Type.PersistInCacheTagged) != 0)
			{
				text += $" [Tag={value.assetOption.context}]";
			}
			list.Add(text);
		}
		return list;
	}
}
