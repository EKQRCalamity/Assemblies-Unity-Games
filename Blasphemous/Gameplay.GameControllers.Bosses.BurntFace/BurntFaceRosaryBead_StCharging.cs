using Gameplay.GameControllers.Bosses.BurntFace.Rosary;
using Maikel.StatelessFSM;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BurntFace;

public class BurntFaceRosaryBead_StCharging : State<BurntFaceRosaryBead>
{
	public override void Enter(BurntFaceRosaryBead owner)
	{
		Debug.Log("BEAD ENTERING CHARGING");
		owner.ResetChargeCounter();
	}

	public override void Execute(BurntFaceRosaryBead owner)
	{
		owner.UpdateChargeCounter();
		if (owner.IsCharged())
		{
			switch (owner.currentType)
			{
			case BurntFaceRosaryBead.ROSARY_BEAD_TYPE.BEAM:
				owner.ChangeToBeam();
				break;
			case BurntFaceRosaryBead.ROSARY_BEAD_TYPE.PROJECTILE:
				owner.ChangeToProjectile();
				break;
			}
		}
	}

	public override void Exit(BurntFaceRosaryBead owner)
	{
	}
}
