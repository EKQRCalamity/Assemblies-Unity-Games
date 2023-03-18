using Framework.Managers;
using Gameplay.GameControllers.Entities.Guardian.Animation;
using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Entities.MiriamPortal.AI;

public class MiriamPortalPrayerIdleState : State
{
	private MiriamPortalPrayerBehaviour _behaviour;

	public override void OnStateInitialize(Gameplay.GameControllers.Entities.StateMachine.StateMachine machine)
	{
		base.OnStateInitialize(machine);
		_behaviour = Machine.GetComponent<MiriamPortalPrayerBehaviour>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		_behaviour.ReappearFlag = false;
		Awaiting();
	}

	public override void Update()
	{
		base.Update();
		if (Core.Logic.Penitent.Status.IsGrounded)
		{
			_behaviour.MiriamPortal.AnimationHandler.Appearing();
			_behaviour.SetInitialOrientation();
			_behaviour.SetState(MiriamPortalPrayerBehaviour.MiriamPortalState.Follow);
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}

	private void Awaiting()
	{
		_behaviour.IdleFlag = false;
		MiriamPortalPrayer miriamPortal = _behaviour.MiriamPortal;
		miriamPortal.AnimationHandler.SetAnimatorTrigger(GuardianPrayerAnimationHandler.AwaitingTrigger);
	}
}
