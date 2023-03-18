using Framework.Managers;
using Gameplay.GameControllers.Effects.Player.Protection;
using UnityEngine;

public class AlliedCherubPrayer : MonoBehaviour
{
	public GameObject AlliedCherubSystem;

	private GameObject _instantiatedAlliedCherubSystem;

	private void Start()
	{
		if ((bool)AlliedCherubSystem)
		{
			PoolManager.Instance.CreatePool(AlliedCherubSystem, 1);
		}
	}

	public void InstantiateCherubs()
	{
		if (!(AlliedCherubSystem == null))
		{
			Vector3 position = Core.Logic.Penitent.transform.position;
			_instantiatedAlliedCherubSystem = PoolManager.Instance.ReuseObject(AlliedCherubSystem, position, Quaternion.identity).GameObject;
			AlliedCherubSystem componentInChildren = _instantiatedAlliedCherubSystem.GetComponentInChildren<AlliedCherubSystem>();
			if ((bool)componentInChildren && !componentInChildren.IsEnable)
			{
				componentInChildren.DeployCherubs();
				componentInChildren.OnCherubsDepleted += OnCherubsDepleted;
			}
		}
	}

	private void OnCherubsDepleted(AlliedCherubSystem obj)
	{
		obj.OnCherubsDepleted -= OnCherubsDepleted;
		Core.Logic.Penitent.PrayerCast.ForcePrayerEnd();
	}

	public void DisposeCherubs()
	{
		if (!(_instantiatedAlliedCherubSystem == null))
		{
			AlliedCherubSystem componentInChildren = _instantiatedAlliedCherubSystem.GetComponentInChildren<AlliedCherubSystem>();
			if ((bool)componentInChildren)
			{
				componentInChildren.DisposeSystem();
			}
		}
	}
}
