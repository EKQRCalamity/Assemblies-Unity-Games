using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using NodeCanvas.BehaviourTrees;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

public abstract class EnemyBehaviour : MonoBehaviour
{
	public LayerMask BlockLayerMask;

	public bool EnableBehaviourOnLoad = true;

	protected float DeltaTargetTime;

	protected Enemy Entity;

	[SerializeField]
	protected CollisionSensor HearingSensor;

	protected bool isBlocked;

	public Core.SimpleEvent OnTurning;

	private readonly float targetTime;

	protected bool TrapDetected;

	[SerializeField]
	protected CollisionSensor VisualSensor;

	public bool PlayerHeard { get; protected set; }

	public bool PlayerSeen { get; protected set; }

	public bool TurningAround { get; set; }

	public bool SensorHitsFloor { get; protected set; }

	public bool GotParry { get; set; }

	protected BehaviourTreeOwner BehaviourTree { get; set; }

	public bool IsGrounded => SensorHitsFloor;

	public bool IsHurt
	{
		get
		{
			return Entity.Status.IsHurt;
		}
		set
		{
			Entity.Status.IsHurt = value;
		}
	}

	public bool IsBlocked => isBlocked;

	public bool IsTrapDetected => TrapDetected;

	public bool IsChasing
	{
		get
		{
			return Entity.IsChasing;
		}
		set
		{
			Entity.IsChasing = value;
		}
	}

	public bool IsAttacking => Entity.IsAttacking;

	public Collider2D GetVisualSensor()
	{
		return VisualSensor.SensorCollider2D;
	}

	public Collider2D GetHearingSensor()
	{
		return HearingSensor.SensorCollider2D;
	}

	public bool IsPlayerHeard()
	{
		return PlayerHeard;
	}

	public bool IsPlayerSeen()
	{
		return PlayerSeen;
	}

	public bool IsTurningAround()
	{
		return TurningAround;
	}

	public bool IsDead()
	{
		return Entity.Status.Dead;
	}

	private void Awake()
	{
		Entity = GetComponent<Enemy>();
		BehaviourTree = GetComponent<BehaviourTreeOwner>();
		OnAwake();
	}

	public virtual void OnAwake()
	{
	}

	private void Start()
	{
		if (HearingSensor != null)
		{
			HearingSensor.SensorTriggerStay += HearingSensorOnTriggerStay;
			HearingSensor.OnEntityExit += HearingSensor_OnEntityExit;
		}
		if (VisualSensor != null)
		{
			VisualSensor.SensorTriggerStay += VisualSensorOnTriggerStay;
			VisualSensor.OnEntityExit += VisualSensor_OnEntityExit;
		}
		OnStart();
	}

	public virtual void OnStart()
	{
	}

	private void Update()
	{
		DeltaTargetTime += Time.deltaTime;
		if (!EnableBehaviourOnLoad)
		{
			StopBehaviour();
		}
		else
		{
			OnUpdate();
		}
	}

	public virtual void OnUpdate()
	{
	}

	private void FixedUpdate()
	{
		OnFixedUpdate();
	}

	public virtual void OnFixedUpdate()
	{
	}

	public virtual void ReverseOrientation()
	{
		EntityOrientation orientation = Entity.Status.Orientation;
		EntityOrientation orientation2 = ((orientation != EntityOrientation.Left) ? EntityOrientation.Left : EntityOrientation.Right);
		Entity.SetOrientation(orientation2);
	}

	public virtual void LookAtTarget(Vector3 targetPos)
	{
		if (Entity.Status.Dead)
		{
			return;
		}
		DeltaTargetTime += Time.deltaTime;
		if (!(DeltaTargetTime >= targetTime))
		{
			return;
		}
		DeltaTargetTime = 0f;
		if (Entity.transform.position.x >= targetPos.x + 1f)
		{
			if (Entity.Status.Orientation != EntityOrientation.Left)
			{
				if (OnTurning != null)
				{
					OnTurning();
				}
				Entity.SetOrientation(EntityOrientation.Left);
			}
		}
		else if (Entity.transform.position.x < targetPos.x - 1f && Entity.Status.Orientation != 0)
		{
			if (OnTurning != null)
			{
				OnTurning();
			}
			Entity.SetOrientation(EntityOrientation.Right);
		}
	}

	protected void EnableColliders(bool enableCollider = true)
	{
		Collider2D[] componentsInChildren = Entity.GetComponentsInChildren<Collider2D>();
		Collider2D[] array = componentsInChildren;
		foreach (Collider2D collider2D in array)
		{
			collider2D.enabled = enableCollider;
		}
	}

	public bool DetectTrap(RaycastHit2D[] hits)
	{
		bool result = false;
		for (int i = 0; i < hits.Length; i++)
		{
			RaycastHit2D raycastHit2D = hits[i];
			if (!(raycastHit2D.collider == null) && raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Trap"))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public Transform GetTarget()
	{
		if (!Entity)
		{
			Entity = GetComponentInParent<Enemy>();
		}
		if ((bool)Entity.Target)
		{
			return Entity.Target.transform;
		}
		if (!Core.Logic.Penitent)
		{
			Debug.LogError("ERROR: Penitent reference cant be accesed yet");
			Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
			if (!penitent)
			{
				return null;
			}
			Transform transform = penitent.transform;
			Entity.Target = transform.gameObject;
			return transform;
		}
		Entity.Target = Core.Logic.Penitent.gameObject;
		return Entity.Target.transform;
	}

	private void VisualSensorOnTriggerStay(Collider2D objectCollider)
	{
		if (!PlayerSeen)
		{
			PlayerSeen = true;
		}
	}

	private void HearingSensorOnTriggerStay(Collider2D objectCollider)
	{
		if (!PlayerHeard)
		{
			PlayerHeard = true;
		}
	}

	private void VisualSensor_OnEntityExit(Entity entity)
	{
		if (PlayerSeen)
		{
			PlayerSeen = !PlayerSeen;
		}
	}

	private void HearingSensor_OnEntityExit(Entity entity)
	{
		if (PlayerHeard)
		{
			PlayerHeard = !PlayerHeard;
		}
	}

	public void StartBehaviour()
	{
		if (!(BehaviourTree == null) && !BehaviourTree.isRunning)
		{
			BehaviourTree.StartBehaviour();
		}
	}

	public void PauseBehaviour()
	{
		if (!(BehaviourTree == null) && BehaviourTree.isRunning)
		{
			BehaviourTree.PauseBehaviour();
		}
	}

	public void StopBehaviour()
	{
		if (!(BehaviourTree == null) && BehaviourTree.isRunning)
		{
			BehaviourTree.StopBehaviour();
		}
	}

	public abstract void Idle();

	public abstract void Wander();

	public abstract void Chase(Transform targetPosition);

	public abstract void Attack();

	public abstract void Damage();

	public abstract void StopMovement();

	public virtual void ReadSpawnerConfig(SpawnBehaviourConfig config)
	{
	}

	public virtual void Parry()
	{
	}

	public virtual void Alive()
	{
		if (Entity.IsStunt)
		{
			Entity.IsStunt = false;
		}
	}

	public virtual void Execution()
	{
		if (!Entity.IsStunt)
		{
			Entity.IsStunt = true;
		}
	}
}
