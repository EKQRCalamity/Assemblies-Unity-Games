using System.Collections;
using UnityEngine;

public class BoatmanEnabler : MapLevelDependentObstacle
{
	private bool forceBoatmanUnlocking;

	protected override void Start()
	{
		if (DLCManager.DLCEnabled())
		{
			StartCoroutine(check_cr());
		}
	}

	private IEnumerator check_cr()
	{
		if (forceBoatmanUnlocking || PlayerData.Data.CurrentMap == Scenes.scene_map_world_DLC)
		{
			OnConditionAlreadyMet();
		}
		else if (!PlayerData.Data.hasUnlockedBoatman)
		{
			if (PlayerData.Data.hasUnlockedFirstSuper)
			{
				PlayerData.Data.shouldShowBoatmanTooltip = true;
				while (!MapEventNotification.Current.showing)
				{
					yield return null;
				}
				while (MapEventNotification.Current.showing)
				{
					yield return null;
				}
				StartCoroutine(showAppear_cr());
			}
			else if (PlayerData.Data.GetLevelData(Levels.Mausoleum).completed)
			{
				while (AbstractEquipUI.Current.CurrentState == AbstractEquipUI.ActiveState.Inactive)
				{
					yield return null;
				}
				while (AbstractEquipUI.Current.CurrentState == AbstractEquipUI.ActiveState.Active)
				{
					yield return null;
				}
				StartCoroutine(showAppear_cr());
			}
		}
		else
		{
			OnConditionAlreadyMet();
		}
	}

	private IEnumerator showAppear_cr()
	{
		Map.Current.CurrentState = Map.State.Event;
		MapEventNotification.Current.showing = true;
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: false, promptBeforeJoin: false);
		CupheadMapCamera cupheadMapCamera = Object.FindObjectOfType<CupheadMapCamera>();
		Vector3 cameraStartPos = cupheadMapCamera.transform.position;
		if (panCamera)
		{
			yield return cupheadMapCamera.MoveToPosition(base.CameraPosition, 0.5f, 0.9f);
		}
		MapMeetCondition();
		while (base.CurrentState != State.Complete)
		{
			yield return null;
		}
		yield return CupheadTime.WaitForSeconds(this, 0.25f);
		if (panCamera)
		{
			cupheadMapCamera.MoveToPosition(cameraStartPos, 0.75f, 1f);
		}
		Map.Current.CurrentState = Map.State.Ready;
		MapEventNotification.Current.showing = false;
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: true, promptBeforeJoin: true);
	}
}
