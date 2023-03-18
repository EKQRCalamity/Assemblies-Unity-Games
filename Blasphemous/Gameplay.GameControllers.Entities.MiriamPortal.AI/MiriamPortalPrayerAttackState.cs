using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Entities.MiriamPortal.Animation;
using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.MiriamPortal.AI;

public class MiriamPortalPrayerAttackState : State
{
	private MiriamPortalPrayer _miriamPortalPrayer;

	public override void OnStateInitialize(Gameplay.GameControllers.Entities.StateMachine.StateMachine machine)
	{
		base.OnStateInitialize(machine);
		_miriamPortalPrayer = Machine.GetComponentInChildren<MiriamPortalPrayer>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		GameObject portalShatteringVfx = _miriamPortalPrayer.Behaviour.PortalShatteringVfx;
		Vector3 position = _miriamPortalPrayer.transform.position;
		PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(portalShatteringVfx, position, Quaternion.identity);
		objectInstance.GameObject.GetComponent<SimpleVFX>().SetOrientation(_miriamPortalPrayer.Status.Orientation);
		Core.Logic.ScreenFreeze.Freeze(0.05f, 0.2f);
		ForwardMovement();
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		_miriamPortalPrayer.transform.DOKill();
	}

	public override void Update()
	{
		base.Update();
	}

	private void ForwardMovement()
	{
		Vector2 actionDirection = _miriamPortalPrayer.Behaviour.GetActionDirection(checkToHitGorund: true);
		Vector3 position = _miriamPortalPrayer.transform.position;
		float num = Mathf.Abs(actionDirection.y - position.y);
		float num2 = 0.1f;
		float descendingTime = Mathf.Lerp(0.1f, 0.5f, num / _miriamPortalPrayer.Behaviour.MaxDistanceToHitGround);
		_miriamPortalPrayer.transform.DOMoveY(position.y + 1f, num2).SetEase(Ease.InQuad).OnComplete(delegate
		{
			_miriamPortalPrayer.transform.DOMoveY(actionDirection.y, descendingTime).SetEase(Ease.InQuad).OnComplete(delegate
			{
				FinishAttack(actionDirection);
			});
		});
		_miriamPortalPrayer.transform.DOMoveX(actionDirection.x, num2 + descendingTime).SetEase(Ease.OutQuad).OnStart(StartAttack);
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(descendingTime);
		sequence.AppendCallback(delegate
		{
			ResumeAnimation();
		});
		sequence.Play();
	}

	private void StartAttack()
	{
		_miriamPortalPrayer.AnimationHandler.SetAnimatorTrigger(MiriamPortalPrayerAnimationHandler.AttackTrigger);
		_miriamPortalPrayer.Audio.PlayAttack();
		_miriamPortalPrayer.GhostTrail.EnableGhostTrail = true;
	}

	private void ResumeAnimation()
	{
		_miriamPortalPrayer.Animator.speed = 1f;
	}

	private void FinishAttack(Vector2 actionDirection)
	{
		_miriamPortalPrayer.transform.DOMoveY(actionDirection.y - 0.3f, 1f).SetEase(Ease.InQuad);
		_miriamPortalPrayer.Behaviour.CheckAndSpawnLandingVfx();
		_miriamPortalPrayer.GhostTrail.EnableGhostTrail = false;
		ResumeAnimation();
	}
}
