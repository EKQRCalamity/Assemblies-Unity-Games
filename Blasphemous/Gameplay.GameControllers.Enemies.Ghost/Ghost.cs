using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Ghost;

public class Ghost : Enemy
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private SpriteRenderer ghostSpriteRenderer;

	private GhostFlight ghostFlight;

	[SerializeField]
	private GhostPath ghostPath;

	public float maxTimeToNextFlight = 15f;

	protected float remainTimeToNextFlight;

	public GhostPath GhostPath => ghostPath;

	public int CurrentWayPointId { get; set; }

	protected float GetRemainTimeToNextFlight()
	{
		return UnityEngine.Random.Range(1f, maxTimeToNextFlight);
	}

	public override EnemyAttack EnemyAttack()
	{
		throw new NotImplementedException();
	}

	public override EnemyBumper EnemyBumper()
	{
		throw new NotImplementedException();
	}

	public override EnemyFloorChecker EnemyFloorChecker()
	{
		throw new NotImplementedException();
	}

	protected override void EnablePhysics(bool enable)
	{
		throw new NotImplementedException();
	}

	protected override void OnAwake()
	{
		base.OnAwake();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_penitent = Core.Logic.Penitent;
		ghostSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
		ghostFlight = GetComponentInChildren<GhostFlight>();
		entityCurrentState = EntityStates.Idle;
		GhostFlight obj = ghostFlight;
		obj.OnStopFloating = (Core.SimpleEvent)Delegate.Combine(obj.OnStopFloating, new Core.SimpleEvent(ghost_OnStopFloating));
		GhostFlight obj2 = ghostFlight;
		obj2.OnLanding = (Core.SimpleEvent)Delegate.Combine(obj2.OnLanding, new Core.SimpleEvent(ghost_OnLanding));
		remainTimeToNextFlight = GetRemainTimeToNextFlight();
		if (ghostPath == null)
		{
			Debug.LogWarning("El fantasma necesita un GhostPath para patrullar");
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		switch (entityCurrentState)
		{
		case EntityStates.Wander:
			ghostFlight.Landing(down: false);
			break;
		case EntityStates.Attack:
			ghostFlight.Landing();
			break;
		case EntityStates.Idle:
			ghostFlight.Floating();
			break;
		}
		if (ghostSpriteRenderer.transform.position.x >= _penitent.transform.position.x)
		{
			SetOrientation(EntityOrientation.Left);
		}
		else if (ghostSpriteRenderer.transform.position.x < _penitent.transform.position.x)
		{
			SetOrientation(EntityOrientation.Right);
		}
		remainTimeToNextFlight -= Time.deltaTime;
		if (remainTimeToNextFlight <= 0f)
		{
			remainTimeToNextFlight = GetRemainTimeToNextFlight();
			ghostFlight.EnableFloating(enable: false);
		}
	}

	private void ghost_OnStopFloating()
	{
		if (entityCurrentState == EntityStates.Idle)
		{
			ghostFlight.SetTargetPosition(base.transform.position, ghostFlight.GetRandomWaypointPosition());
			entityCurrentState = EntityStates.Attack;
		}
	}

	private void ghost_OnLanding()
	{
		ghostFlight.EnableFloating();
		if (entityCurrentState != EntityStates.Idle)
		{
			entityCurrentState = EntityStates.Idle;
		}
	}
}
