using Gameplay.GameControllers.Entities.MiriamPortal.Animation;
using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Entities.MiriamPortal.AI;

public class MiriamPortalPrayerVanishState : State
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
		Vanish();
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}

	private void Vanish()
	{
		_behaviour.VanishFlag = false;
		_behaviour.IdleFlag = false;
		MiriamPortalPrayer miriamPortal = _behaviour.MiriamPortal;
		miriamPortal.AnimationHandler.SetAnimatorTrigger(MiriamPortalPrayerAnimationHandler.VanishTrigger);
		_behaviour.MiriamPortal.Audio.PlayVanish();
	}
}
