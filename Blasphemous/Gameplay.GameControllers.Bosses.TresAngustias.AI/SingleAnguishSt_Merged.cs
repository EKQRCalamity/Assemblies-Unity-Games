using Maikel.StatelessFSM;

namespace Gameplay.GameControllers.Bosses.TresAngustias.AI;

public class SingleAnguishSt_Merged : State<SingleAnguishBehaviour>
{
	public override void Enter(SingleAnguishBehaviour owner)
	{
		owner.ActivateSprite(activate: false);
		owner.ActivateSteering(enabled: false);
		owner.ActivateWeapon(activate: false);
	}

	public override void Execute(SingleAnguishBehaviour owner)
	{
	}

	public override void Exit(SingleAnguishBehaviour owner)
	{
		owner.ActivateSprite(activate: true);
		owner.ActivateWeapon(activate: true);
	}
}
