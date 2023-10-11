using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameStepInstantiateAsync : GameStep
{
	private AssetReference _assetReference;

	private Action<GameObject> _onAssetInstantiated;

	private Action _onBeginInstantiate;

	private bool _releaseAfterInstantiate;

	public GameStepInstantiateAsync(AssetReference assetReference, Action<GameObject> onAssetInstantiated = null, Action onBeginInstantiate = null, bool releaseAfterInstantiate = false)
	{
		_assetReference = assetReference;
		_onAssetInstantiated = onAssetInstantiated;
		_onBeginInstantiate = onBeginInstantiate;
		_releaseAfterInstantiate = releaseAfterInstantiate;
	}

	protected override IEnumerator Update()
	{
		_onBeginInstantiate?.Invoke();
		AsyncOperationHandle<GameObject> instantiateAsync = Addressables.InstantiateAsync(_assetReference.RuntimeKey);
		while (!instantiateAsync.IsDone)
		{
			yield return null;
		}
		_onAssetInstantiated?.Invoke(instantiateAsync.Result);
		if (_releaseAfterInstantiate)
		{
			Addressables.ReleaseInstance(instantiateAsync.Result);
		}
	}
}
