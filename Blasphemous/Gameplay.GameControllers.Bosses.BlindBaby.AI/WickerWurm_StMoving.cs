using Maikel.StatelessFSM;

namespace Gameplay.GameControllers.Bosses.BlindBaby.AI;

public class WickerWurm_StMoving : State<WickerWurmBehaviour>
{
	public override void Enter(WickerWurmBehaviour owner)
	{
		owner.SetMoving(moving: true);
	}

	public override void Execute(WickerWurmBehaviour owner)
	{
		owner.UpdateLookAtPath();
	}

	public override void Exit(WickerWurmBehaviour owner)
	{
		owner.SetMoving(moving: false);
	}
}
