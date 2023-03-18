using DG.Tweening;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.DrownedCorpse.AI.DrownedCorpseStates;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.DrownedCorpse.AI;

public class DrownedCorpseBehaviour : EnemyBehaviour
{
	public enum CorpseState
	{
		Sleep,
		Chase
	}

	[SerializeField]
	[FoldoutGroup("Motion Settings", true, 0)]
	private RaycastHit2D[] _bottomHits;

	private readonly bool _isSpawning;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float ActivationDistance;

	private float currentChasingTimeAfterLostTarget;

	public CorpseState CurrentCorpseState;

	public float DistanceToTarget;

	[FoldoutGroup("Motion Settings", true, 0)]
	public LayerMask GroundLayerMask;

	[SerializeField]
	[FoldoutGroup("Activation Settings", true, 0)]
	private float MaxChasingTime = 0.5f;

	[SerializeField]
	[FoldoutGroup("Activation Settings", true, 0)]
	private float MinChasingTime = 0.2f;

	[SerializeField]
	[FoldoutGroup("Sleep Settings", true, 0)]
	private float maxSleepTime = 16f;

	[SerializeField]
	[FoldoutGroup("Sleep Settings", true, 0)]
	private float minSleepTime = 8f;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float MaxTimeAwaitingBeforeChase;

	public float MaxVisibleHeight = 2f;

	private float sleepTime;

	[FoldoutGroup("Activation Settings", true, 0)]
	public bool startAwaken = true;

	private bool Vanished { get; set; }

	public DrownedCorpse DrownedCorpse { get; set; }

	private StateMachine StateMachine { get; set; }

	private VisionCone visionCone { get; set; }

	public void Awaken()
	{
	}

	public void SleepForever()
	{
	}

	public override void OnAwake()
	{
		base.OnAwake();
		StateMachine = Entity.GetComponent<StateMachine>();
		visionCone = Entity.GetComponentInChildren<VisionCone>();
		_bottomHits = new RaycastHit2D[2];
	}

	public override void OnStart()
	{
		base.OnStart();
		DrownedCorpse = (DrownedCorpse)Entity;
		ResetSleepTime();
		SwitchToState(CorpseState.Sleep);
		if (Random.value > 0.5f)
		{
			LookAtTarget(base.transform.position - Vector3.right);
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		switch (CurrentCorpseState)
		{
		case CorpseState.Sleep:
			Sleep();
			break;
		case CorpseState.Chase:
			Chase(DrownedCorpse.Target.transform);
			break;
		}
	}

	private void SwitchToState(CorpseState targetState)
	{
		CurrentCorpseState = targetState;
		switch (CurrentCorpseState)
		{
		case CorpseState.Sleep:
			StateMachine.SwitchState<DrownedCorpseSleepState>();
			break;
		case CorpseState.Chase:
			StateMachine.SwitchState<DrownedCorpseChaseState>();
			break;
		}
	}

	public bool CanWalk()
	{
		return !_isSpawning;
	}

	public void Sleep()
	{
		currentChasingTimeAfterLostTarget = Random.Range(MinChasingTime, MaxChasingTime);
		sleepTime -= Time.deltaTime;
		if (TargetCanBeVisible() && sleepTime < 0f && CanSeeTarget())
		{
			SwitchToState(CorpseState.Chase);
		}
	}

	public override void Chase(Transform targetPosition)
	{
		if (!DrownedCorpse.MotionChecker.HitsFloor || DrownedCorpse.MotionChecker.HitsBlock)
		{
			StopMovement();
			VanishAfterRun();
		}
		if (TargetIsLost(targetPosition.position))
		{
			currentChasingTimeAfterLostTarget -= Time.deltaTime;
			if (currentChasingTimeAfterLostTarget <= 0f)
			{
				VanishAfterRun();
			}
		}
	}

	private void VanishAfterRun()
	{
		ResetSleepTime();
		DrownedCorpse.AnimatorInyector.VanishAfterRun();
		SwitchToState(CorpseState.Sleep);
	}

	private bool TargetIsLost(Vector3 position)
	{
		bool result = false;
		if (CurrentCorpseState != CorpseState.Chase)
		{
			return false;
		}
		if (DrownedCorpse.Controller.PlatformCharacterPhysics.HSpeed > 0.1f)
		{
			result = position.x < DrownedCorpse.transform.position.x;
		}
		else if (DrownedCorpse.Controller.PlatformCharacterPhysics.HSpeed < 0f)
		{
			result = position.x > DrownedCorpse.transform.position.x;
		}
		return result;
	}

	public bool TargetCanBeVisible()
	{
		GetTarget();
		if (!DrownedCorpse.Target)
		{
			return false;
		}
		Vector2 vector = DrownedCorpse.Target.transform.position - DrownedCorpse.transform.position;
		float num = Mathf.Abs(vector.y);
		float num2 = Mathf.Abs(vector.x);
		return num <= MaxVisibleHeight && num2 < ActivationDistance;
	}

	public bool CanSeeTarget()
	{
		if (visionCone == null)
		{
			return false;
		}
		return visionCone.CanSeeTarget(DrownedCorpse.Target.transform, "Penitent");
	}

	public override void Idle()
	{
	}

	public override void Wander()
	{
	}

	public override void Damage()
	{
		VanishByHit();
		DrownedCorpse.AnimatorInyector.VanishAfterDamage();
	}

	public void OnGuarded()
	{
	}

	private void VanishByHit()
	{
		ResetSleepTime();
		SwitchToState(CorpseState.Sleep);
	}

	public void Death()
	{
		StopMovement();
	}

	public override void StopMovement()
	{
		DrownedCorpse.Input.HorizontalInput = 0f;
		DrownedCorpse.Controller.PlatformCharacterPhysics.HSpeed = 0f;
	}

	public void StopMovement(float elapse)
	{
		DrownedCorpse.Input.HorizontalInput = 0f;
		DOTween.To(delegate(float x)
		{
			DrownedCorpse.Controller.PlatformCharacterPhysics.HSpeed = x;
		}, DrownedCorpse.Controller.PlatformCharacterPhysics.HSpeed, 0f, elapse);
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (targetPos.x > DrownedCorpse.transform.position.x)
		{
			if (DrownedCorpse.Status.Orientation != 0)
			{
				DrownedCorpse.SetOrientation(EntityOrientation.Right);
			}
		}
		else if (DrownedCorpse.Status.Orientation != EntityOrientation.Left)
		{
			DrownedCorpse.SetOrientation(EntityOrientation.Left);
		}
		SetColliderScale();
	}

	private void ResetSleepTime()
	{
		sleepTime = Random.Range(minSleepTime, maxSleepTime);
	}

	public override void Attack()
	{
	}

	private void SetColliderScale()
	{
		DrownedCorpse.EntityDamageArea.DamageAreaCollider.transform.localScale = new Vector3((DrownedCorpse.Status.Orientation == EntityOrientation.Right) ? 1 : (-1), 1f, 1f);
	}
}
