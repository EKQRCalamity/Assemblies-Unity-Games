using System;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.PietyMonster.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PietyMonster.IA;

public class PietyMonsterBehaviour : EnemyBehaviour
{
	private PietyMonster _pietyMonster;

	private LogicManager _gameLogic;

	public int StompAttackCounter;

	private bool lastAttackWasArea;

	public bool Awake { get; set; }

	public bool TargetOnRange { get; set; }

	public bool ReadyToAttack { get; set; }

	public bool Attacking { get; set; }

	public bool Spiting { get; set; }

	public PietyStompAttack StompAttack { get; private set; }

	public PietyClawAttack ClawAttack { get; private set; }

	public PietySmashAttack SmashAttack { get; private set; }

	public PietySpitAttack SpitAttack { get; set; }

	public bool StatusChange { get; set; }

	public bool CanSpit => StompAttackCounter > 1;

	public event Core.SimpleEvent OnBehaviourChange;

	public override void OnAwake()
	{
		base.OnAwake();
		_pietyMonster = (PietyMonster)Entity;
		StompAttack = _pietyMonster.GetComponentInChildren<PietyStompAttack>();
		ClawAttack = _pietyMonster.GetComponentInChildren<PietyClawAttack>();
		SmashAttack = _pietyMonster.GetComponentInChildren<PietySmashAttack>();
		SpitAttack = _pietyMonster.GetComponentInChildren<PietySpitAttack>();
	}

	public override void OnStart()
	{
		base.OnStart();
		_pietyMonster.OnDeath += PietyMonsterOnEntityDie;
		_gameLogic = Core.Logic;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		switch (_gameLogic.CurrentState)
		{
		case LogicStates.PlayerDead:
			Idle();
			if (base.BehaviourTree.isRunning)
			{
				base.BehaviourTree.StopBehaviour();
			}
			_pietyMonster.AnimatorInyector.ResetAttacks();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case LogicStates.Unresponsive:
		case LogicStates.Playing:
		case LogicStates.Console:
		case LogicStates.Pause:
		case LogicStates.PopUp:
			break;
		}
		if (_pietyMonster.Stats.Life.MissingRatio >= 0.9f)
		{
			if (_pietyMonster.CurrentBossStatus == PietyMonster.BossStatus.First)
			{
				return;
			}
			_pietyMonster.CurrentBossStatus = PietyMonster.BossStatus.First;
		}
		if (!Attacking && !base.IsAttacking && !lastAttackWasArea)
		{
			if (_pietyMonster.Stats.Life.MissingRatio >= 0.8f)
			{
				SetNewBossStatus(PietyMonster.BossStatus.Second);
			}
			else if (_pietyMonster.Stats.Life.MissingRatio >= 0.6f)
			{
				SetNewBossStatus(PietyMonster.BossStatus.Third);
			}
			else if (_pietyMonster.Stats.Life.MissingRatio >= 0.4f)
			{
				SetNewBossStatus(PietyMonster.BossStatus.Forth);
			}
			else if (_pietyMonster.Stats.Life.MissingRatio >= 0.2f)
			{
				SetNewBossStatus(PietyMonster.BossStatus.Fifth);
			}
			else if (_pietyMonster.Stats.Life.MissingRatio >= 0.05f)
			{
				SetNewBossStatus(PietyMonster.BossStatus.Sixth);
			}
		}
	}

	private void SetNewBossStatus(PietyMonster.BossStatus currentStatus)
	{
		if (_pietyMonster.CurrentBossStatus != currentStatus)
		{
			_pietyMonster.CurrentBossStatus = currentStatus;
			_pietyMonster.AnimatorInyector.AreaAttack();
			OnBossBehaviourChange();
			lastAttackWasArea = true;
		}
	}

	public override void Idle()
	{
		StopMovement();
		_pietyMonster.AnimatorInyector.Idle();
	}

	public override void Wander()
	{
	}

	public override void Chase(Transform target)
	{
		if (!(_pietyMonster == null))
		{
			float num = ((_pietyMonster.Status.Orientation != 0) ? (-1f) : 1f);
			_pietyMonster.Inputs.HorizontalInput = ((!_pietyMonster.CanMove) ? 0f : num);
			_pietyMonster.AnimatorInyector.Walk();
		}
	}

	public override void Attack()
	{
		StopMovement();
		if (!Attacking && TargetOnRange)
		{
			Attacking = true;
			float value = UnityEngine.Random.value;
			if (value <= 0.45f)
			{
				_pietyMonster.AnimatorInyector.StompAttack();
				StompAttackCounter++;
			}
			else
			{
				_pietyMonster.AnimatorInyector.ClawAttack();
			}
			lastAttackWasArea = false;
		}
	}

	public void Spit()
	{
		StopMovement();
		_pietyMonster.AnimatorInyector.SpitAttack();
	}

	public override void Damage()
	{
	}

	public override void StopMovement()
	{
		_pietyMonster.Inputs.HorizontalInput = 0f;
		_pietyMonster.AnimatorInyector.Stop();
	}

	public void StopTurning()
	{
		if (base.TurningAround)
		{
			base.TurningAround = !base.TurningAround;
		}
		_pietyMonster.AnimatorInyector.StopTurning();
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (_pietyMonster.Animator.GetNextAnimatorStateInfo(0).IsName("Walk"))
		{
			float num = ((_pietyMonster.Status.Orientation != 0) ? (-1f) : 1f);
			_pietyMonster.Inputs.HorizontalInput = ((!_pietyMonster.CanMove) ? 0f : num);
		}
		else
		{
			StopMovement();
		}
		_pietyMonster.AnimatorInyector.TurnAround();
	}

	private void PietyMonsterOnEntityDie()
	{
		StopMovement();
		base.BehaviourTree.StopBehaviour();
		_pietyMonster.AnimatorInyector.Death();
	}

	private void OnDestroy()
	{
		_pietyMonster.OnDeath -= PietyMonsterOnEntityDie;
	}

	protected virtual void OnBossBehaviourChange()
	{
		this.OnBehaviourChange?.Invoke();
	}
}
