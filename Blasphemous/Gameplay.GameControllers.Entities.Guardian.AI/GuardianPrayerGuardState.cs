using System;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Guardian.Animation;
using Gameplay.GameControllers.Entities.StateMachine;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Damage;

namespace Gameplay.GameControllers.Entities.Guardian.AI;

public class GuardianPrayerGuardState : State
{
	private GuardianPrayer _guardianPrayer;

	private bool _playerPreviousState;

	public override void OnStateInitialize(Gameplay.GameControllers.Entities.StateMachine.StateMachine machine)
	{
		base.OnStateInitialize(machine);
		_guardianPrayer = Machine.GetComponentInChildren<GuardianPrayer>();
		PenitentDamageArea.OnHitGlobal = (PenitentDamageArea.PlayerHitEvent)Delegate.Combine(PenitentDamageArea.OnHitGlobal, new PenitentDamageArea.PlayerHitEvent(OnMasterHit));
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		_playerPreviousState = Core.Logic.Penitent.Status.Invulnerable;
		SetPlayerInvulnerable(invulnerable: true);
		ForwardMovement();
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		SetPlayerInvulnerable(_playerPreviousState);
	}

	public override void Update()
	{
		base.Update();
	}

	private void ForwardMovement()
	{
		float shieldDistance = _guardianPrayer.Behaviour.ShieldDistance;
		float actionDirection = _guardianPrayer.Behaviour.GetActionDirection(shieldDistance);
		_guardianPrayer.transform.DOMoveX(actionDirection, 0.1f, snapping: true).SetEase(Ease.InSine).OnStart(OnStartForwardMovement)
			.OnComplete(OnFinishForwardMovement);
	}

	private void OnStartForwardMovement()
	{
		Guard();
	}

	private void OnFinishForwardMovement()
	{
	}

	private void Guard()
	{
		_guardianPrayer.AnimationHandler.SetAnimatorTrigger(GuardianPrayerAnimationHandler.GuardTrigger);
		_guardianPrayer.Audio.PlayGuard();
	}

	private void OnMasterHit(Gameplay.GameControllers.Penitent.Penitent penitent, Hit hit)
	{
		Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("SimpleHit");
	}

	private void SetPlayerInvulnerable(bool invulnerable)
	{
		Core.Logic.Penitent.Status.Invulnerable = invulnerable;
	}

	public override void Destroy()
	{
		base.Destroy();
		PenitentDamageArea.OnHitGlobal = (PenitentDamageArea.PlayerHitEvent)Delegate.Remove(PenitentDamageArea.OnHitGlobal, new PenitentDamageArea.PlayerHitEvent(OnMasterHit));
	}
}
