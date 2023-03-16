using System;
using UnityEngine;

public class MusicLoader : AssetLoader<AudioClip>
{
	protected override Coroutine loadAsset(string assetName, Action<AudioClip> completionHandler)
	{
		return AssetBundleLoader.LoadMusic(assetName, completionHandler);
	}

	protected override AudioClip loadAssetSynchronous(string assetName)
	{
		throw new NotImplementedException();
	}

	protected override void destroyAsset(AudioClip asset)
	{
		UnityEngine.Object.Destroy(asset);
	}
}
