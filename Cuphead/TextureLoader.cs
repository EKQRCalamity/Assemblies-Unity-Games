using System;
using UnityEngine;

public class TextureLoader : AssetLoader<Texture2D[]>
{
	protected override Coroutine loadAsset(string assetName, Action<Texture2D[]> completionHandler)
	{
		return AssetBundleLoader.LoadTextures(assetName, completionHandler);
	}

	protected override Texture2D[] loadAssetSynchronous(string assetName)
	{
		return AssetBundleLoader.LoadTexturesSynchronous(assetName);
	}

	protected override void destroyAsset(Texture2D[] asset)
	{
		for (int i = 0; i < asset.Length; i++)
		{
			UnityEngine.Object.Destroy(asset[i]);
		}
	}
}
