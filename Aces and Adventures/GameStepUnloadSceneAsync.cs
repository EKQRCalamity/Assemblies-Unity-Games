using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class GameStepUnloadSceneAsync : GameStep
{
	private AssetReference _sceneReference;

	public GameStepUnloadSceneAsync(AssetReference sceneReference)
	{
		_sceneReference = sceneReference;
	}

	protected override IEnumerator Update()
	{
		if (base.manager.IsAddressableSceneLoaded(_sceneReference))
		{
			AsyncOperationHandle<SceneInstance> unload = Addressables.UnloadSceneAsync(base.manager.RemoveLoadedSceneHandle(_sceneReference));
			while (!unload.IsDone)
			{
				yield return null;
			}
		}
	}
}
