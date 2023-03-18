using Maikel.StatelessFSM;

namespace Gameplay.GameControllers.Bosses.Amanecidas;

public class Amanecidas_StFloating : State<AmanecidasBehaviour>
{
	public override void Enter(AmanecidasBehaviour owner)
	{
		owner.ActivateFloating(active: true);
		owner.CheckCounterAttack();
	}

	public override void Execute(AmanecidasBehaviour owner)
	{
		owner.UpdateFloatingCounter();
	}

	public override void Exit(AmanecidasBehaviour owner)
	{
		owner.ActivateFloating(active: false);
		owner.SetDodge(active: false);
	}
}
