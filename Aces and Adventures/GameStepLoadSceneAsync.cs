using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class GameStepLoadSceneAsync : GameStep
{
	private AssetReference _sceneReference;

	public GameStepLoadSceneAsync(AssetReference sceneReference)
	{
		_sceneReference = sceneReference;
	}

	protected override IEnumerator Update()
	{
		AsyncOperationHandle<SceneInstance> load = Addressables.LoadSceneAsync(_sceneReference.RuntimeKey, LoadSceneMode.Additive, activateOnLoad: false);
		while (!load.IsDone)
		{
			yield return null;
		}
		AsyncOperation activate = load.Result.ActivateAsync();
		while (!activate.isDone)
		{
			yield return null;
		}
		base.manager.AddLoadedSceneHandle(_sceneReference, load);
	}
}
