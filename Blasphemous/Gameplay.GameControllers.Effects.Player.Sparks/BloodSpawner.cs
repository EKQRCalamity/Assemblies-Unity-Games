using Framework.FrameworkCore;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Sparks;

public class BloodSpawner : Trait
{
	public enum BLOOD_FX_TYPES
	{
		SMALL,
		MEDIUM,
		BIG
	}

	public BloodVFXTable bloodVFXTable;

	public GameObject GetBloodFX(BLOOD_FX_TYPES bloodType)
	{
		BloodFXTableElement randomElementOfType = bloodVFXTable.GetRandomElementOfType(bloodType);
		return PoolManager.Instance.ReuseObject(randomElementOfType.prefab, base.transform.position, base.transform.rotation).GameObject;
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		InitPool();
	}

	private void InitPool()
	{
		foreach (BloodFXTableElement bloodVFX in bloodVFXTable.bloodVFXList)
		{
			PoolManager.Instance.CreatePool(bloodVFX.prefab, bloodVFX.poolSize);
		}
	}
}
