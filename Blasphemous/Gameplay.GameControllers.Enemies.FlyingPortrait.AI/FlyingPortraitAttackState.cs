using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.FlyingPortrait.AI;

public class FlyingPortraitAttackState : State
{
	private FlyingPortrait _flyingPortrait;

	private StateMachine _stateMachine;

	private Transform _target;

	private float _attackCoolDownLapse;

	private bool _stopChase;

	public override void OnStateInitialize(StateMachine machine)
	{
		base.OnStateInitialize(machine);
		_flyingPortrait = machine.GetComponent<FlyingPortrait>();
		_target = Core.Logic.Penitent.transform;
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
	}

	public override void Update()
	{
		base.Update();
		if (_flyingPortrait.Behaviour.IsPlayerHeard() && !_flyingPortrait.Behaviour.IsAwake)
		{
			SetOrientation();
			_flyingPortrait.AnimatorInjector.UnHang();
		}
		if (!_flyingPortrait.Behaviour.IsAwake)
		{
			return;
		}
		SetOrientation();
		_attackCoolDownLapse += Time.deltaTime;
		if (Vector2.Distance(_flyingPortrait.transform.position, _target.position) > _flyingPortrait.Behaviour.DistanceAttack)
		{
			_flyingPortrait.Behaviour.Chase(_target);
			_stopChase = false;
			return;
		}
		StopChase();
		if (_attackCoolDownLapse >= _flyingPortrait.Behaviour.AttackCoolDown)
		{
			_attackCoolDownLapse = 0f;
			_flyingPortrait.AnimatorInjector.Attack();
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		_flyingPortrait.AnimatorInjector.ResetAttack();
	}

	private void StopChase()
	{
		if (!_stopChase)
		{
			_stopChase = true;
			bool flag = _flyingPortrait.Status.Orientation == EntityOrientation.Right;
			float endValue = _flyingPortrait.transform.position.x + ((!flag) ? 0.5f : (-0.5f));
			_flyingPortrait.transform.DOMoveX(endValue, 1f);
		}
	}

	private void SetOrientation()
	{
		if (!_flyingPortrait.Behaviour.IsAttacking)
		{
			GameObject gameObject = Core.Logic.Penitent.gameObject;
			EntityOrientation orientation = _flyingPortrait.Status.Orientation;
			float num = ((orientation != EntityOrientation.Left) ? 0.1f : (-0.1f));
			bool flag = gameObject.transform.position.x + num > _flyingPortrait.transform.position.x;
			_flyingPortrait.SetOrientation((!flag) ? EntityOrientation.Left : EntityOrientation.Right);
		}
	}
}
