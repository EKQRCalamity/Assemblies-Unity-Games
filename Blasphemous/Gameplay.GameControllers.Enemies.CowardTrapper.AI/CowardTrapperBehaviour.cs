using System;
using System.Collections;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.CowardTrapper.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.CowardTrapper.AI;

public class CowardTrapperBehaviour : EnemyBehaviour
{
	public LayerMask OneWayDownLayer;

	public float ActivationDistance = 6f;

	public float SpawningTrapInterval = 1f;

	public int MaxTrapsAmount = 3;

	private List<CowardTrap> _cowardTraps = new List<CowardTrap>();

	private float _currentTrapInterval;

	private bool isActive;

	public float TimeRunning = 4f;

	public float TimeAwaiting = 1f;

	private float _currentTimeRunning;

	private float _currentTimeAwaiting;

	private Vector2 _runningDir;

	private IEnumerator _runningCoroutine;

	public CowardTrapper CowardTrapper { get; private set; }

	public bool IsRunAway { get; set; }

	public bool ReverseDirection { get; set; }

	private Vector2 GetRunningDirection
	{
		get
		{
			Vector2 vector = ((!(CowardTrapper.Target.transform.position.x > CowardTrapper.transform.position.x)) ? Vector2.right : Vector2.left);
			if (!ReverseDirection)
			{
				return vector;
			}
			ReverseDirection = false;
			return vector * -1f;
		}
	}

	public new bool IsBlocked => CowardTrapper.MotionChecker.HitsBlock || !CowardTrapper.MotionChecker.HitsFloor || CowardTrapper.MotionChecker.HitsPatrolBlock;

	public bool IsAwaiting => _currentTimeAwaiting > 0f;

	public bool CanSeeTarget => CowardTrapper.VisionCone.CanSeeTarget(CowardTrapper.Target.transform, "Penitent");

	private bool IsOnOneWayDownPlatform => Physics2D.Raycast(base.transform.position, -base.transform.up, 2f, OneWayDownLayer);

	private bool CanSpawnTrap => _cowardTraps.Count < MaxTrapsAmount;

	public override void OnAwake()
	{
		base.OnAwake();
		CowardTrapper = (CowardTrapper)Entity;
		CowardTrapper.OnDeath += OnDeath;
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnDeath()
	{
		CowardTrapper.OnDeath -= OnDeath;
	}

	public override void OnStart()
	{
		base.OnStart();
		_currentTimeRunning = TimeRunning;
		_currentTimeAwaiting = TimeAwaiting;
		_currentTrapInterval = SpawningTrapInterval;
		_runningCoroutine = RunAwayCoroutine();
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		isActive = true;
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (isActive && !CowardTrapper.Status.Dead)
		{
			_currentTimeAwaiting -= Time.deltaTime;
			if (CowardTrapper.DistanceToTarget <= ActivationDistance && !IsRunAway && !IsAwaiting && CanSeeTarget)
			{
				CowardTrapper.AnimatorInjector.Scared();
			}
			if (IsBlocked && IsRunAway)
			{
				StopMovement();
			}
			SpawnTraps();
		}
	}

	private IEnumerator RunAwayCoroutine()
	{
		IsRunAway = true;
		_runningDir = GetRunningDirection;
		while (_currentTimeRunning > 0f)
		{
			_currentTimeRunning -= Time.deltaTime;
			RunAway(_runningDir);
			if (IsBlocked || Entity.Status.Dead)
			{
				if (IsBlocked)
				{
					ReverseOrientation();
				}
				StopMovement();
				yield break;
			}
			yield return null;
		}
		StopMovement();
	}

	public void StartRun()
	{
		StartCoroutine(RunAwayCoroutine());
	}

	public void RunAway(Vector2 dir)
	{
		CowardTrapper.Input.HorizontalInput = dir.x;
		Entity.Status.Orientation = ((!(dir.x > 0f)) ? EntityOrientation.Left : EntityOrientation.Right);
		Entity.SpriteRenderer.flipX = Entity.Status.Orientation == EntityOrientation.Left;
	}

	public override void StopMovement()
	{
		CowardTrapper.Input.HorizontalInput = 0f;
		CowardTrapper.AnimatorInjector.StopRun();
		CowardTrapper.Controller.PlatformCharacterPhysics.HSpeed = 0f;
		IsRunAway = false;
		_currentTimeRunning = TimeRunning;
		_currentTimeAwaiting = TimeAwaiting;
		_currentTrapInterval = SpawningTrapInterval;
		if (IsBlocked)
		{
			ReverseDirection = true;
			LookAtTarget(CowardTrapper.Target.transform.position);
		}
	}

	private void SpawnTraps()
	{
		if (IsRunAway && CanSpawnTrap && !IsOnOneWayDownPlatform)
		{
			_currentTrapInterval -= Time.deltaTime;
			if (_currentTrapInterval < 0f)
			{
				_currentTrapInterval = SpawningTrapInterval;
				CowardTrap component = CowardTrapper.Attack.SummonAreaOnPoint(base.transform.position).GetComponent<CowardTrap>();
				component.SetOwner(CowardTrapper);
				AddTrap(component);
			}
		}
	}

	public void AddTrap(CowardTrap trap)
	{
		if (!_cowardTraps.Contains(trap))
		{
			_cowardTraps.Add(trap);
		}
	}

	public void RemoveTrap(CowardTrap trap)
	{
		if (_cowardTraps.Contains(trap))
		{
			_cowardTraps.Remove(trap);
		}
	}

	private new void ReverseOrientation()
	{
		EntityOrientation orientation = Entity.Status.Orientation;
		EntityOrientation orientation2 = ((orientation != EntityOrientation.Left) ? EntityOrientation.Left : EntityOrientation.Right);
		Entity.SetOrientation(orientation2);
	}

	public override void Idle()
	{
		throw new NotImplementedException();
	}

	public override void Wander()
	{
		throw new NotImplementedException();
	}

	public override void Chase(Transform targetPosition)
	{
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void Damage()
	{
		throw new NotImplementedException();
	}
}
