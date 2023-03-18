using Gameplay.GameControllers.Bosses.BurntFace.Rosary;
using Maikel.StatelessFSM;

namespace Gameplay.GameControllers.Bosses.BurntFace;

public class BurntFaceRosaryBead_StDestroyed : State<BurntFaceRosaryBead>
{
	public override void Enter(BurntFaceRosaryBead owner)
	{
		owner.SetAnimatorDestroyed(dest: true);
		owner.OnDestroyed();
	}

	public override void Execute(BurntFaceRosaryBead owner)
	{
	}

	public override void Exit(BurntFaceRosaryBead owner)
	{
		owner.SetAnimatorDestroyed(dest: false);
	}
}
