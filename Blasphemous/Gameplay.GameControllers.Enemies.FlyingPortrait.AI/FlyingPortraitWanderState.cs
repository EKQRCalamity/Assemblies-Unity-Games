using Framework.FrameworkCore;
using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Enemies.FlyingPortrait.AI;

public class FlyingPortraitWanderState : State
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
		_flyingPortrait.DamageByContact = false;
	}

	public override void Update()
	{
		base.Update();
		if (_flyingPortrait.Behaviour.IsAwake)
		{
			_flyingPortrait.Behaviour.Wander();
			if (_flyingPortrait.MotionChecker.HitsBlock || _flyingPortrait.MotionChecker.HitsPatrolBlock)
			{
				EntityOrientation orientation = ((_flyingPortrait.Status.Orientation == EntityOrientation.Right) ? EntityOrientation.Left : EntityOrientation.Right);
				_flyingPortrait.SetOrientation(orientation);
			}
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}
