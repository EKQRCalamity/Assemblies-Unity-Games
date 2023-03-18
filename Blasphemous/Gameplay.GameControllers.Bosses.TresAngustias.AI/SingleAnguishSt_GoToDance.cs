using Maikel.StatelessFSM;

namespace Gameplay.GameControllers.Bosses.TresAngustias.AI;

public class SingleAnguishSt_GoToDance : State<SingleAnguishBehaviour>
{
	public override void Enter(SingleAnguishBehaviour owner)
	{
		owner.ActivateSteering(enabled: true);
		owner.ActivateGhostMode(enabled: true);
	}

	public override void Execute(SingleAnguishBehaviour owner)
	{
		owner.UpdateGoToDancePointState();
		if (owner.IsCloseToTargetPoint(0.25f))
		{
			owner.ChangeToDance();
		}
	}

	public override void Exit(SingleAnguishBehaviour owner)
	{
		owner.ActivateSteering(enabled: false);
		owner.ActivateGhostMode(enabled: false);
	}
}
