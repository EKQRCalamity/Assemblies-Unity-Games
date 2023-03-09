using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SpriteAtlasLoader : AssetLoader<SpriteAtlas>
{
	private Dictionary<string, List<Action<SpriteAtlas>>> deferredAtlastRequests = new Dictionary<string, List<Action<SpriteAtlas>>>();

	private void OnEnable()
	{
		SpriteAtlasManager.atlasRequested += atlasRequestedHandler;
	}

	private void OnDisable()
	{
		SpriteAtlasManager.atlasRequested -= atlasRequestedHandler;
	}

	protected override Coroutine loadAsset(string assetName, Action<SpriteAtlas> completionHandler)
	{
		Action<SpriteAtlas> completionHandler2 = delegate(SpriteAtlas atlas)
		{
			resolveDeferredRequests(assetName, atlas);
			completionHandler(atlas);
		};
		return AssetBundleLoader.LoadSpriteAtlas(assetName, completionHandler2);
	}

	protected override SpriteAtlas loadAssetSynchronous(string assetName)
	{
		throw new NotImplementedException();
	}

	protected override void destroyAsset(SpriteAtlas asset)
	{
		UnityEngine.Object.Destroy(asset);
	}

	private void addDeferredRequest(string assetName, Action<SpriteAtlas> action)
	{
		if (!deferredAtlastRequests.TryGetValue(assetName, out var value))
		{
			value = new List<Action<SpriteAtlas>>();
			deferredAtlastRequests.Add(assetName, value);
		}
		value.Add(action);
	}

	private void resolveDeferredRequests(string assetName, SpriteAtlas atlas)
	{
		if (atlas == null || !deferredAtlastRequests.TryGetValue(assetName, out var value))
		{
			return;
		}
		foreach (Action<SpriteAtlas> item in value)
		{
			item(atlas);
		}
		deferredAtlastRequests.Remove(assetName);
	}

	private void atlasRequestedHandler(string atlasTag, Action<SpriteAtlas> action)
	{
		Action<SpriteAtlas> completionAction = delegate(SpriteAtlas atlas)
		{
			if (atlas == null)
			{
				addDeferredRequest(atlasTag, action);
			}
			else
			{
				action(atlas);
			}
		};
		loadAssetFromAssetBundle(atlasTag, AssetLoaderOption.None(), completionAction);
	}
}
