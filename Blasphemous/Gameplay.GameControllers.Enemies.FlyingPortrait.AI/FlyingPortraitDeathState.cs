using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Enemies.FlyingPortrait.AI;

public class FlyingPortraitDeathState : State
{
	private FlyingPortrait _flyingPortrait;

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		_flyingPortrait = machine.GetComponent<FlyingPortrait>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		if (_flyingPortrait.Status.Dead)
		{
			_flyingPortrait.AnimatorInjector.Death();
		}
	}
}
