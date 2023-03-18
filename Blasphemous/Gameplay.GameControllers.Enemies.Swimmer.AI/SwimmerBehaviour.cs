using System;
using System.Collections;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Swimmer.Animator;
using Gameplay.GameControllers.Enemies.Swimmer.Attack;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Swimmer.AI;

public class SwimmerBehaviour : EnemyBehaviour
{
	[FoldoutGroup("Activation Settings", true, 0)]
	public float ActivationDistance;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float AttackDistance = 0.5f;

	[MinMaxSlider(5f, 10f, false)]
	[FoldoutGroup("Attack Settings", true, 0)]
	public Vector2 ChasingSpeed;

	public GameObject SwimmerTerrainEffects;

	protected Vector2 JumpPosition;

	private const float TargetHeightOffset = 0.1f;

	[FoldoutGroup("Attack Settings", 0)]
	[MinMaxSlider(0f, 1f, false)]
	public Vector2 LapseBeforeJump;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float DistanceToTarget { get; private set; }

	public Swimmer Swimmer { get; private set; }

	public bool IsTriggerAttack { get; set; }

	public bool IsSwimming { get; set; }

	public bool IsJumping { get; set; }

	public bool IsVisible { get; private set; }

	public bool CanChase => Swimmer.MotionChecker.HitsFloor && !Swimmer.MotionChecker.HitsBlock;

	private bool IsTargetAbove
	{
		get
		{
			if (!Swimmer.Target)
			{
				return false;
			}
			return Swimmer.transform.position.y <= Swimmer.Target.transform.position.y + 0.1f;
		}
	}

	public bool CanJump => Mathf.Abs(GetTarget().position.x - base.transform.position.x) < AttackDistance;

	public bool EnableCollision
	{
		get
		{
			return Swimmer.Collider.EnableCollision2D;
		}
		set
		{
			Swimmer.Collider.EnableCollision2D = value;
		}
	}

	private float DistanceToJumpPosition => Vector2.Distance(base.transform.position, JumpPosition);

	public override void OnStart()
	{
		base.OnStart();
		Swimmer = (Swimmer)Entity;
		if ((bool)SwimmerTerrainEffects)
		{
			PoolManager.Instance.CreatePool(SwimmerTerrainEffects, 1);
		}
		Swimmer.Controller.MaxWalkingSpeed = UnityEngine.Random.Range(ChasingSpeed.x, ChasingSpeed.y);
		SetVisible();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		float vSpeed = Swimmer.Controller.PlatformCharacterPhysics.VSpeed;
		if (vSpeed > 1f)
		{
			EnableCollision = false;
		}
		if (DistanceToJumpPosition < 1f && vSpeed < -0.1f)
		{
			EnableCollision = true;
		}
		if (IsTriggerAttack || Swimmer.Status.Dead)
		{
			StopMovement();
			return;
		}
		DistanceToTarget = Vector2.Distance(Swimmer.transform.position, GetTarget().position);
		base.IsChasing = DistanceToTarget <= ActivationDistance && IsTargetAbove;
		if (base.IsChasing)
		{
			SetVisible();
			Chase(GetTarget());
		}
		else
		{
			SetVisible(visible: false);
			StopMovement();
		}
		if (CanJump && IsTargetAbove)
		{
			Jump();
			IsTriggerAttack = true;
		}
	}

	public override void Chase(Transform targetPosition)
	{
		float horizontalInput = ((!(GetTarget().position.x > base.transform.position.x)) ? (-1f) : 1f);
		if (!CanChase)
		{
			horizontalInput = 0f;
			StopMovement();
		}
		Swimmer.Input.HorizontalInput = horizontalInput;
		IsSwimming = true;
		IsJumping = false;
		LookAtTarget(targetPosition.position);
	}

	private void SetVisible(bool visible = true)
	{
		if (IsVisible && !visible)
		{
			Swimmer.AnimatorInjector.SpriteVisible(visible: false, 0.5f, OnBecomeInvisible);
			IsVisible = false;
		}
		else if (!IsVisible && visible)
		{
			Swimmer.AnimatorInjector.SpriteVisible(visible: true, 0.5f);
			IsVisible = true;
		}
	}

	private void OnBecomeInvisible()
	{
		if (!base.IsChasing)
		{
			Swimmer.transform.position = new Vector2(Swimmer.StartPoint.x, Swimmer.transform.position.y);
		}
	}

	public override void StopMovement()
	{
		Swimmer.Controller.PlatformCharacterPhysics.HSpeed = 0f;
		Swimmer.Input.HorizontalInput = 0f;
		IsSwimming = false;
	}

	public void Jump()
	{
		SwimmerAttack swimmerAttack = (SwimmerAttack)Swimmer.EnemyAttack();
		swimmerAttack.JumpPosition = Swimmer.transform.position;
		IsJumping = true;
		JumpPosition = new Vector2(base.transform.position.x, base.transform.position.y);
		StartCoroutine(JumpPress());
	}

	private IEnumerator JumpPress()
	{
		yield return new WaitForSeconds(UnityEngine.Random.Range(LapseBeforeJump.x, LapseBeforeJump.y));
		Swimmer.Input.Jump = true;
		yield return new WaitForSeconds(1f);
		Swimmer.Input.Jump = false;
	}

	public void RisingTerrainEffect(bool isRising, Vector2 position)
	{
		PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(SwimmerTerrainEffects, position, Quaternion.identity);
		if (objectInstance != null)
		{
			SwimmerTerrainEffect component = objectInstance.GameObject.GetComponent<SwimmerTerrainEffect>();
			component.RisingEffect(isRising);
		}
	}

	public override void Idle()
	{
		throw new NotImplementedException();
	}

	public override void Wander()
	{
		throw new NotImplementedException();
	}

	public override void Attack()
	{
		if ((bool)Swimmer)
		{
			SwimmerAttack swimmerAttack = (SwimmerAttack)Swimmer.EntityAttack;
			if (swimmerAttack.IsTargetTouched)
			{
				swimmerAttack.ContactAttack(Core.Logic.Penitent);
			}
		}
	}

	public override void Damage()
	{
		throw new NotImplementedException();
	}
}
