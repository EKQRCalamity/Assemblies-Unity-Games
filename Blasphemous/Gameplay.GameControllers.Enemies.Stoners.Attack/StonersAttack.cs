using System;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Stoners.Rock;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Stoners.Attack;

public class StonersAttack : EnemyAttack
{
	private Stoners _stoners;

	public float CriticalDistance = 2f;

	public Entity EntityTarget;

	public float NearDistanceRange = 4f;

	public float RndThrowHorOffset = 1f;

	public float StraightImpulseForce = 30f;

	public float TargetHeightOffset = 0.5f;

	public CollisionSensor VisualSensor;

	public Vector3 CurrentTargetPos { get; set; }

	public Vector2 BullsEyePos { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		_stoners = GetComponentInParent<Stoners>();
		VisualSensor.OnEntityEnter += VisualSensorOnEntityEnter;
	}

	private void Update()
	{
		if (!(EntityTarget == null) && !(_stoners == null) && _stoners.Status.IsVisibleOnCamera)
		{
			CurrentTargetPos = new Vector2(EntityTarget.transform.position.x, EntityTarget.transform.position.y);
			if (GetDistanceToTarget(CurrentTargetPos) < CriticalDistance)
			{
				_stoners.StonerBehaviour.CurrentAtackWaitTime = 0f;
			}
		}
	}

	public void SetBullsEyeWhenThrow()
	{
		if (!(EntityTarget == null))
		{
			BullsEyePos = new Vector2(CurrentTargetPos.x, CurrentTargetPos.y);
		}
	}

	public void ThrowRock()
	{
		try
		{
			Vector3 position = _stoners.RockSpawnPoint.transform.position;
			GameObject rock = _stoners.RockPool.GetRock(position);
			StonersRock componentInChildren = rock.GetComponentInChildren<StonersRock>();
			componentInChildren.SetOwner(_stoners);
			float distanceToTarget = GetDistanceToTarget(BullsEyePos);
			if (distanceToTarget > NearDistanceRange)
			{
				SetProjectileMotion(rock, BullsEyePos);
			}
			else
			{
				SetStraightMotion(rock, BullsEyePos);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + ex.StackTrace);
		}
	}

	private void SetProjectileMotion(GameObject rock, Vector3 target)
	{
		Vector2 vector = new Vector2(UnityEngine.Random.Range(target.x - RndThrowHorOffset, target.x + RndThrowHorOffset), target.y);
		float num = vector.x - rock.transform.position.x;
		float num2 = vector.y - rock.transform.position.y;
		float f = Mathf.Atan((num2 + 7f) / num);
		float num3 = num / Mathf.Cos(f);
		float x = num3 * Mathf.Cos(f);
		float y = num3 * Mathf.Sin(f);
		Rigidbody2D component = rock.GetComponent<Rigidbody2D>();
		component.velocity = new Vector2(x, y);
	}

	private void SetStraightMotion(GameObject rock, Vector3 target)
	{
		Vector3 vector = new Vector3(UnityEngine.Random.Range(target.x - RndThrowHorOffset, target.x + RndThrowHorOffset), target.y + TargetHeightOffset);
		Vector3 normalized = (vector - rock.transform.position).normalized;
		Rigidbody2D component = rock.GetComponent<Rigidbody2D>();
		component.AddForce(normalized * StraightImpulseForce, ForceMode2D.Impulse);
	}

	private void VisualSensorOnEntityEnter(Entity entity)
	{
		EntityTarget = entity;
	}

	private void OnDestroy()
	{
		VisualSensor.OnEntityEnter -= VisualSensorOnEntityEnter;
	}

	private float GetDistanceToTarget(Vector3 target)
	{
		float result = 0f;
		if (EntityTarget != null)
		{
			result = Vector3.Distance(_stoners.transform.position, target);
		}
		return result;
	}
}
