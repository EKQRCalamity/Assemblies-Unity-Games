using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

public class EnemyAI : MonoBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	public bool allowEntityOrientation;

	public float attackTime = 1f;

	private RaycastHit2D[] bottomHits;

	private RaycastHit2D[] forwardsHits;

	private float currentGroundDetection;

	private float deltaAttackTime;

	private float deltaStateCounter;

	private float deltaTargetTime;

	private float deltaTurnAroundTime;

	public LayerMask enemyLayerMask;

	private Enemy entity;

	[SerializeField]
	private CollisionSensor hearingSensor;

	public float hurtTime = 2f;

	public float idleTimeLimit = 5f;

	private bool isBlocked;

	private int maxHitsAllocated;

	public float maxRangeGroundDetection = 5f;

	[Header("Sensors variables")]
	private float myWidth;

	[Header("Sensors variables")]
	private float myHeight;

	private bool playerHeard;

	private Vector3 playerPosition;

	private bool playerSeen;

	[Tooltip("The length of the block detection raycast")]
	[Range(0f, 1f)]
	public float rangeBlockDectection = 0.5f;

	[Tooltip("The length og the ground detection raycast")]
	[Range(0f, 10f)]
	public float rangeGroundDetection = 2f;

	public float speed = 1f;

	private float stateTimeTreshold;

	private readonly float targetTime;

	[Header("Behaviour time variables")]
	public float timeChasing = 1f;

	private bool trapDetected;

	private readonly float turnAroundTime = 1.5f;

	[SerializeField]
	private CollisionSensor visualSensor;

	public float walkTimeLimit = 10f;

	public float RangeGroundDetection
	{
		get
		{
			return rangeGroundDetection;
		}
		set
		{
			rangeGroundDetection = value;
		}
	}

	public bool GroundSensorHitsFloor { get; private set; }

	private void Awake()
	{
		entity = GetComponent<Enemy>();
	}

	private void Start()
	{
		BoxCollider2D componentInChildren = GetComponentInChildren<BoxCollider2D>();
		myWidth = componentInChildren.bounds.extents.x;
		myHeight = componentInChildren.bounds.extents.y;
		stateTimeTreshold = walkTimeLimit;
		if (hearingSensor != null)
		{
			hearingSensor.OnEntityEnter += HearingSensor_OnEntityEnter;
			hearingSensor.OnEntityExit += HearingSensor_OnEntityExit;
		}
		if (visualSensor != null)
		{
			visualSensor.OnEntityEnter += VisualSensor_OnEntityEnter;
			visualSensor.OnEntityExit += VisualSensor_OnEntityExit;
		}
		_penitent = Core.Logic.Penitent;
		currentGroundDetection = rangeGroundDetection;
		maxHitsAllocated = 2;
		bottomHits = new RaycastHit2D[maxHitsAllocated];
		forwardsHits = new RaycastHit2D[maxHitsAllocated];
	}

	private void Update()
	{
		deltaTurnAroundTime += Time.deltaTime;
		if (trapDetected)
		{
			deltaTurnAroundTime = 0f;
		}
		if (deltaTurnAroundTime <= turnAroundTime)
		{
			playerSeen = (playerHeard = false);
		}
		switch (entity.entityCurrentState)
		{
		case EntityStates.Wander:
			evaluateWalk();
			break;
		case EntityStates.Idle:
			evaluateIdle();
			break;
		case EntityStates.Chasing:
			evaluateChasing();
			break;
		case EntityStates.Attack:
			evaluateAttack();
			break;
		case EntityStates.Hurt:
			evaluateHurt();
			break;
		}
	}

	private void FixedUpdate()
	{
		if (entity.Status.Orientation == EntityOrientation.Left)
		{
			Vector2 vector = (Vector2)base.transform.position - ((Vector2)base.transform.right * myWidth * 1.5f + Vector2.up * (myHeight * 2f));
			Debug.DrawLine(vector, vector - Vector2.up * currentGroundDetection, Color.yellow);
			GroundSensorHitsFloor = Physics2D.LinecastNonAlloc(vector, vector - Vector2.up * currentGroundDetection, bottomHits, enemyLayerMask) > 0;
			Debug.DrawLine(vector, vector - (Vector2)base.transform.right * rangeBlockDectection, Color.yellow);
			isBlocked = Physics2D.LinecastNonAlloc(vector, vector - (Vector2)base.transform.right * rangeBlockDectection, forwardsHits, enemyLayerMask) > 0;
		}
		else
		{
			Vector2 vector = (Vector2)base.transform.position + ((Vector2)base.transform.right * myWidth * 1.5f - Vector2.up * (myHeight * 2f));
			Debug.DrawLine(vector, vector - Vector2.up * currentGroundDetection, Color.yellow);
			GroundSensorHitsFloor = Physics2D.LinecastNonAlloc(vector, vector - Vector2.up * currentGroundDetection, bottomHits, enemyLayerMask) > 0;
			Debug.DrawLine(vector, vector + (Vector2)base.transform.right * rangeBlockDectection, Color.yellow);
			isBlocked = Physics2D.LinecastNonAlloc(vector, vector + (Vector2)base.transform.right * rangeBlockDectection, forwardsHits, enemyLayerMask) > 0;
		}
		trapDetected = detectTrap(bottomHits);
		if (trapDetected)
		{
			stopChasing();
			entity.entityCurrentState = EntityStates.Idle;
			reverseOrientation();
		}
		if ((!GroundSensorHitsFloor || isBlocked) && !entity.Status.IsHurt)
		{
			if (allowEntityOrientation)
			{
				reverseOrientation();
			}
			stopChasing();
		}
	}

	private void reverseOrientation()
	{
		EntityOrientation orientation = entity.Status.Orientation;
		EntityOrientation orientation2 = ((orientation != EntityOrientation.Left) ? EntityOrientation.Left : EntityOrientation.Right);
		entity.SetOrientation(orientation2);
	}

	public void ResetStateTime()
	{
		if (deltaStateCounter > 0f)
		{
			deltaStateCounter = 0f;
		}
	}

	private void lookAtTarget(Vector3 targetPos)
	{
		if (entity.Status.Dead)
		{
			return;
		}
		deltaTargetTime += Time.deltaTime;
		if (!(deltaTargetTime >= targetTime))
		{
			return;
		}
		deltaTargetTime = 0f;
		if (entity.transform.position.x >= targetPos.x + 1f)
		{
			if (entity.Status.Orientation != EntityOrientation.Left)
			{
				entity.SetOrientation(EntityOrientation.Left);
			}
		}
		else if (entity.transform.position.x < targetPos.x - 1f && entity.Status.Orientation != 0)
		{
			entity.SetOrientation(EntityOrientation.Right);
		}
	}

	private bool detectTrap(RaycastHit2D[] hits)
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

	private void evaluateWalk()
	{
		deltaStateCounter += Time.deltaTime;
		if (deltaStateCounter >= stateTimeTreshold)
		{
			deltaStateCounter = 0f;
			stateTimeTreshold = idleTimeLimit;
			entity.entityCurrentState = EntityStates.Idle;
		}
		if ((playerSeen || playerHeard) && entity.entityCurrentState != EntityStates.Chasing && !_penitent.Status.Dead)
		{
			stateTimeTreshold = timeChasing;
			entity.entityCurrentState = EntityStates.Chasing;
		}
	}

	private void evaluateIdle()
	{
		deltaStateCounter += Time.deltaTime;
		if (deltaStateCounter >= stateTimeTreshold)
		{
			deltaStateCounter = 0f;
			stateTimeTreshold = walkTimeLimit;
			entity.entityCurrentState = EntityStates.Wander;
		}
		if ((playerSeen || playerHeard) && entity.entityCurrentState != EntityStates.Chasing && !_penitent.Status.Dead)
		{
			entity.entityCurrentState = EntityStates.Chasing;
		}
	}

	private void evaluateChasing()
	{
		if (_penitent.Status.Dead)
		{
			stateTimeTreshold = idleTimeLimit;
			entity.entityCurrentState = EntityStates.Idle;
			return;
		}
		currentGroundDetection = maxRangeGroundDetection;
		if (_penitent != null)
		{
			lookAtTarget(_penitent.transform.position);
		}
		if (entity.EntityAttack.IsEnemyHit)
		{
			deltaStateCounter = 0f;
			stateTimeTreshold = idleTimeLimit;
			entity.entityCurrentState = EntityStates.Attack;
		}
		if (!playerSeen && !playerHeard)
		{
			deltaStateCounter += Time.deltaTime;
			if (deltaStateCounter >= timeChasing)
			{
				currentGroundDetection = rangeGroundDetection;
				deltaStateCounter = 0f;
				stateTimeTreshold = walkTimeLimit;
				entity.entityCurrentState = EntityStates.Wander;
			}
		}
	}

	private void evaluateAttack()
	{
		if (_penitent != null && _penitent.Status.Dead)
		{
			stateTimeTreshold = idleTimeLimit;
			entity.entityCurrentState = EntityStates.Idle;
		}
		else if (!entity.EntityAttack.IsEnemyHit)
		{
			deltaAttackTime += Time.deltaTime;
			if (deltaAttackTime >= attackTime)
			{
				deltaAttackTime = 0f;
				stateTimeTreshold = timeChasing;
				entity.entityCurrentState = EntityStates.Chasing;
			}
		}
		else
		{
			deltaAttackTime = 0f;
		}
	}

	private void evaluateHurt()
	{
		deltaStateCounter += Time.deltaTime;
		if (deltaStateCounter >= hurtTime)
		{
			deltaStateCounter = 0f;
			stateTimeTreshold = timeChasing;
			entity.entityCurrentState = EntityStates.Chasing;
		}
	}

	private void stopChasing()
	{
		if (entity.entityCurrentState == EntityStates.Chasing)
		{
			playerHeard = false;
			playerSeen = false;
			deltaStateCounter = timeChasing;
			currentGroundDetection = rangeGroundDetection;
		}
	}

	private void VisualSensor_OnEntityExit(Entity entity)
	{
		playerSeen = false;
	}

	private void VisualSensor_OnEntityEnter(Entity entity)
	{
		playerSeen = true;
	}

	private void HearingSensor_OnEntityExit(Entity entity)
	{
		playerHeard = false;
	}

	private void HearingSensor_OnEntityEnter(Entity entity)
	{
		playerHeard = true;
	}
}
