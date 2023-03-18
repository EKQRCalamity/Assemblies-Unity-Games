using Framework.Managers;
using Gameplay.GameControllers.Effects.Player.Protection;
using UnityEngine;

public class ShieldSystemPrayer : MonoBehaviour
{
	public GameObject ShieldSystem;

	private GameObject _instantiatedShield;

	private void Start()
	{
		if ((bool)ShieldSystem)
		{
			PoolManager.Instance.CreatePool(ShieldSystem, 1);
		}
	}

	public void InstantiateShield()
	{
		if (!(ShieldSystem == null))
		{
			Vector3 position = Core.Logic.Penitent.transform.position;
			_instantiatedShield = PoolManager.Instance.ReuseObject(ShieldSystem, position, Quaternion.identity).GameObject;
			PenitentShieldSystem componentInChildren = _instantiatedShield.GetComponentInChildren<PenitentShieldSystem>();
			if ((bool)componentInChildren)
			{
				componentInChildren.SetShieldsOwner(Core.Logic.Penitent);
			}
		}
	}

	public void DisposeShield()
	{
		if (!(_instantiatedShield == null))
		{
			PenitentShieldSystem componentInChildren = _instantiatedShield.GetComponentInChildren<PenitentShieldSystem>();
			if ((bool)componentInChildren)
			{
				componentInChildren.DisposeShield(1f);
			}
		}
	}
}
