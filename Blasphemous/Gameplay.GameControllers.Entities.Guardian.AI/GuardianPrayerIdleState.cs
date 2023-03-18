using Framework.Managers;
using Gameplay.GameControllers.Entities.Guardian.Animation;
using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Entities.Guardian.AI;

public class GuardianPrayerIdleState : State
{
	private GuardianPrayerBehaviour _behaviour;

	public override void OnStateInitialize(Gameplay.GameControllers.Entities.StateMachine.StateMachine machine)
	{
		base.OnStateInitialize(machine);
		_behaviour = Machine.GetComponent<GuardianPrayerBehaviour>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		Awaiting();
	}

	public override void Update()
	{
		base.Update();
		if (Core.Logic.Penitent.Status.IsGrounded)
		{
			GuardianPrayer guardian = _behaviour.Guardian;
			guardian.AnimationHandler.Appearing();
			guardian.Behaviour.SetInitialOrientation();
			guardian.transform.position = _behaviour.Master.transform.position;
			_behaviour.SetState(GuardianPrayerBehaviour.GuardianState.Follow);
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}

	private void Awaiting()
	{
		_behaviour.IdleFlag = false;
		GuardianPrayer guardian = _behaviour.Guardian;
		guardian.AnimationHandler.SetAnimatorTrigger(GuardianPrayerAnimationHandler.AwaitingTrigger);
	}
}
