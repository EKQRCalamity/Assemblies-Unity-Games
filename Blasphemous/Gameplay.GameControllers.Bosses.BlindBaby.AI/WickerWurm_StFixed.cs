using Maikel.StatelessFSM;

namespace Gameplay.GameControllers.Bosses.BlindBaby.AI;

public class WickerWurm_StFixed : State<WickerWurmBehaviour>
{
	public override void Enter(WickerWurmBehaviour owner)
	{
		owner.AffixBody(affix: true);
		owner.lookAtPlayer = true;
	}

	public override void Execute(WickerWurmBehaviour owner)
	{
	}

	public override void Exit(WickerWurmBehaviour owner)
	{
		owner.AffixBody(affix: false);
		owner.lookAtPlayer = false;
	}
}
