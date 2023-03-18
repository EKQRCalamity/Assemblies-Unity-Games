using Gameplay.GameControllers.Entities.Guardian.Animation;
using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Entities.Guardian.AI;

public class GuardianPrayerVanishState : State
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
		GuardianPrayer guardian = _behaviour.Guardian;
		guardian.AnimationHandler.SetAnimatorTrigger(GuardianPrayerAnimationHandler.VanishTrigger);
		_behaviour.Guardian.Audio.PlayVanish();
	}
}
