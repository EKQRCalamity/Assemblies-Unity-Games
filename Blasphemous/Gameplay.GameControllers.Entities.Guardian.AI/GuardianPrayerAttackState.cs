using DG.Tweening;
using Gameplay.GameControllers.Entities.Guardian.Animation;
using Gameplay.GameControllers.Entities.StateMachine;

namespace Gameplay.GameControllers.Entities.Guardian.AI;

public class GuardianPrayerAttackState : State
{
	private GuardianPrayer _guardianPrayer;

	public override void OnStateInitialize(Gameplay.GameControllers.Entities.StateMachine.StateMachine machine)
	{
		base.OnStateInitialize(machine);
		_guardianPrayer = Machine.GetComponentInChildren<GuardianPrayer>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		ForwardMovement();
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}

	public override void Update()
	{
		base.Update();
	}

	private void ForwardMovement()
	{
		float attackDistance = _guardianPrayer.Behaviour.AttackDistance;
		float actionDirection = _guardianPrayer.Behaviour.GetActionDirection(attackDistance);
		_guardianPrayer.transform.DOMoveX(actionDirection, 0.2f, snapping: true).SetEase(Ease.InSine).OnStart(OnStartForwardMovement)
			.OnComplete(OnFinishForwardMovement);
	}

	private void Attack()
	{
		_guardianPrayer.AnimationHandler.SetAnimatorTrigger(GuardianPrayerAnimationHandler.AttackTrigger);
		_guardianPrayer.Audio.PlayAttack();
	}

	private void OnStartForwardMovement()
	{
		Attack();
	}

	private void OnFinishForwardMovement()
	{
	}
}
