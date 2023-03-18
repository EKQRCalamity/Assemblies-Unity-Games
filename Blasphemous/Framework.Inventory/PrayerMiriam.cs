using Framework.Managers;
using Gameplay.GameControllers.Entities.MiriamPortal;
using Gameplay.GameControllers.Entities.MiriamPortal.AI;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Framework.Inventory;

public class PrayerMiriam : ObjectEffect
{
	public float vOffset = 2f;

	public float hOffest = 2f;

	public GameObject PortalPrefab;

	private GameObject PortalInstantiation;

	private MiriamPortalPrayer MiriamPortal;

	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!Core.InventoryManager.IsPrayerEquipped("PR201"))
		{
			DisposePortal();
		}
	}

	protected override bool OnApplyEffect()
	{
		InstantiatePortal();
		return true;
	}

	protected override void OnRemoveEffect()
	{
		base.OnRemoveEffect();
		DisposePortal();
	}

	public void InstantiatePortal()
	{
		if (PortalPrefab != null && PortalInstantiation == null)
		{
			PortalInstantiation = Object.Instantiate(PortalPrefab, CalculateTargetPosition(), Quaternion.identity);
		}
		else if (PortalInstantiation != null)
		{
			if (PortalInstantiation.activeInHierarchy)
			{
				MiriamPortal.Behaviour.ReappearFlag = true;
				MiriamPortal.Behaviour.VanishFlag = false;
			}
			else
			{
				PortalInstantiation.SetActive(value: true);
				PortalInstantiation.transform.position = CalculateTargetPosition();
			}
		}
		if (MiriamPortal == null)
		{
			MiriamPortal = PortalInstantiation.GetComponentInChildren<MiriamPortalPrayer>();
		}
		MiriamPortal.Behaviour.SetState(MiriamPortalPrayerBehaviour.MiriamPortalState.Follow);
	}

	private Vector3 CalculateTargetPosition()
	{
		Penitent penitent = Core.Logic.Penitent;
		Vector3 result = penitent.transform.position + Vector3.up * vOffset;
		result.x += ((penitent.GetOrientation() != 0) ? hOffest : (0f - hOffest));
		return result;
	}

	public void DisposePortal()
	{
		if (!(PortalInstantiation == null))
		{
			if (MiriamPortal == null)
			{
				MiriamPortal = PortalInstantiation.GetComponentInChildren<MiriamPortalPrayer>();
			}
			if ((bool)MiriamPortal && (bool)MiriamPortal.Behaviour)
			{
				MiriamPortal.Behaviour.VanishFlag = true;
			}
		}
	}
}
