using Maikel.StatelessFSM;

namespace Gameplay.GameControllers.Bosses.TresAngustias.AI;

public class SingleAnguishSt_GoToMergePoint : State<SingleAnguishBehaviour>
{
	public override void Enter(SingleAnguishBehaviour owner)
	{
		owner.ActivateSteering(enabled: true);
		owner.ActivateGhostMode(enabled: true);
	}

	public override void Execute(SingleAnguishBehaviour owner)
	{
		owner.UpdateGoToTargetPoint();
		if (owner.IsCloseToTargetPoint(0.05f))
		{
			owner.NotifyMaster();
		}
	}

	public override void Exit(SingleAnguishBehaviour owner)
	{
		owner.ActivateSteering(enabled: false);
		owner.ActivateGhostMode(enabled: false);
	}
}
