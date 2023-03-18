using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.Inventory;

public class PrayerUseVfx : ObjectEffect_Stat
{
	[BoxGroup("VFX", true, false, 0)]
	public GameObject Vfx;

	protected override void OnAwake()
	{
		base.OnAwake();
		if (Vfx != null)
		{
			PoolManager.Instance.CreatePool(Vfx, 1);
		}
	}

	protected override bool OnApplyEffect()
	{
		InstantiateEffect();
		return base.OnApplyEffect();
	}

	private void InstantiateEffect()
	{
		if (!(Vfx == null))
		{
			Vector3 position = Core.Logic.Penitent.transform.position;
			PoolManager.Instance.ReuseObject(Vfx, position, Quaternion.identity);
		}
	}
}
