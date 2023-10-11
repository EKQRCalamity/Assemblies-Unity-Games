using System;
using UnityEngine;

public class GameStepInstantiate : GameStep
{
	private GameObject _assetReference;

	private Action<GameObject> _onAssetInstantiated;

	private Action _onBeginInstantiate;

	private bool _releaseAfterInstantiate;

	public GameStepInstantiate(GameObject assetReference, Action<GameObject> onAssetInstantiated = null, Action onBeginInstantiate = null, bool releaseAfterInstantiate = false)
	{
		_assetReference = assetReference;
		_onAssetInstantiated = onAssetInstantiated;
		_onBeginInstantiate = onBeginInstantiate;
		_releaseAfterInstantiate = releaseAfterInstantiate;
	}

	public override void Start()
	{
		_onBeginInstantiate?.Invoke();
		GameObject obj = UnityEngine.Object.Instantiate(_assetReference);
		_onAssetInstantiated?.Invoke(obj);
		if (_releaseAfterInstantiate)
		{
			UnityEngine.Object.Destroy(obj);
		}
	}
}
