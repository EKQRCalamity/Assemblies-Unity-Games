using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Prayers;

public class ZambraTearsHarvestPrayer : MonoBehaviour
{
	public GameObject ZambraEffectPrefab;

	private GameObject zambraEffect;

	private void Start()
	{
		CreatePoolEffect();
	}

	public void EnableEffect()
	{
		InstantiateEffect();
	}

	public void DisableEffect()
	{
		DisposeEffect();
	}

	private void CreatePoolEffect()
	{
		if ((bool)ZambraEffectPrefab)
		{
			PoolManager.Instance.CreatePool(ZambraEffectPrefab, 1);
		}
	}

	private void InstantiateEffect()
	{
		if ((bool)ZambraEffectPrefab)
		{
			Vector3 position = Core.Logic.Penitent.GetPosition();
			zambraEffect = PoolManager.Instance.ReuseObject(ZambraEffectPrefab, position, Quaternion.identity).GameObject;
		}
	}

	private void DisposeEffect()
	{
		if ((bool)zambraEffect)
		{
			TearHarvestEffect componentInChildren = zambraEffect.GetComponentInChildren<TearHarvestEffect>();
			if ((bool)componentInChildren)
			{
				componentInChildren.Dispose();
			}
		}
	}
}
