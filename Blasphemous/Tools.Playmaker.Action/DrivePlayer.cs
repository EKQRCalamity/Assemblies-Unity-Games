using Framework.FrameworkCore;
using Framework.Managers;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Tools.PlayMaker.Action;

public class DrivePlayer : FsmStateAction
{
	public FsmGameObject Destination;

	public EntityOrientation FinalPlayerOrientation;

	public override void OnEnter()
	{
		base.OnEnter();
		if (Destination.Value == null)
		{
			Core.Logic.Penitent.SetOrientation(FinalPlayerOrientation);
			return;
		}
		Vector3 position = Destination.Value.transform.position;
		Core.Logic.Penitent.DrivePlayer.OnStopMotion += OnStopMotion;
		Core.Logic.Penitent.DrivePlayer.MoveToPosition(position, FinalPlayerOrientation);
	}

	private void OnStopMotion()
	{
		Core.Logic.Penitent.DrivePlayer.OnStopMotion -= OnStopMotion;
		Finish();
	}
}
