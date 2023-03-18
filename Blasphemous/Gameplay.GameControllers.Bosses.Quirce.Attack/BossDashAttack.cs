using System;
using System.Collections.Generic;
using Framework.Managers;
using Framework.Pooling;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Quirce.Attack;

public class BossDashAttack : EnemyAttack, IDirectAttack, ISpawnerAttack, IPaintAttackCollider
{
	public delegate void OnDashAdvancedFunction(float value);

	public delegate void RotatingFunction(Transform parentToRotate, Vector3 point);

	public AnimationCurve curve;

	public float dashDuration;

	public bool checkCollisions = true;

	public bool dashAlongGround;

	public LayerMask collisionMask;

	private Transform _parentToMove;

	private Vector3 _targetPoint;

	private Coroutine _currentCoroutine;

	[FoldoutGroup("Graphics", 0)]
	public bool rotateTowardsDirection;

	[FoldoutGroup("Instantiations", 0)]
	public List<DashAttackInstantiations> objectsToInstantiate;

	[ShowIf("checkCollisions", true)]
	[FoldoutGroup("Instantiations", 0)]
	public List<DashAttackInstantiations> instantiateOnCollision;

	[FoldoutGroup("Damage", 0)]
	public bool dealsDamage = true;

	[ShowIf("dealsDamage", true)]
	[FoldoutGroup("Damage", 0)]
	public bool unavoidable;

	[ShowIf("dealsDamage", true)]
	[FoldoutGroup("Damage", 0)]
	public bool unblockable;

	[ShowIf("dealsDamage", true)]
	[FoldoutGroup("Damage", 0)]
	public bool stopsOnBlock = true;

	[ShowIf("dealsDamage", true)]
	[FoldoutGroup("Damage", 0)]
	public bool dealsDamageOnStart;

	[ShowIf("dealsDamage", true)]
	[FoldoutGroup("Damage", 0)]
	public int damage;

	[FoldoutGroup("Screenshake", 0)]
	public bool screenshakeOnEnd;

	[FoldoutGroup("Screenshake", 0)]
	[ShowIf("screenshakeOnEnd", true)]
	public int vibrato = 40;

	[FoldoutGroup("Screenshake", 0)]
	[ShowIf("screenshakeOnEnd", true)]
	public float shakeForce;

	[FoldoutGroup("Screenshake", 0)]
	[ShowIf("screenshakeOnEnd", true)]
	public float shakeDuration;

	public bool useSpeed;

	[ShowIf("useSpeed", true)]
	public float speed;

	private List<DashAttackInstantiations> alreadyInstantiated;

	private Hit _hit;

	private RotatingFunction currentRotatingFunction;

	private bool willStopOnCollision;

	private bool currentlyDealsDamage;

	private Vector2 _screenShakeDir;

	private Vector2 _dashDir;

	public AttackArea AttackArea { get; set; }

	public Hit BossDashHit => _hit;

	public event OnDashAdvancedFunction OnDashAdvancedEvent;

	public event Action<BossDashAttack> OnDashBlockedEvent;

	public event Action OnDashFinishedEvent;

	public void SetRotatingFunction(RotatingFunction f)
	{
		currentRotatingFunction = f;
	}

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea = GetComponentInChildren<AttackArea>();
		base.EntityOwner.OnDeath += OnDeath;
		if (dealsDamage)
		{
			CreateHit();
		}
		CheckPools();
		AttachShowScriptIfNeeded();
	}

	private void CheckPools()
	{
		foreach (DashAttackInstantiations item in objectsToInstantiate)
		{
			PoolObject component = item.prefabToInstantiate.GetComponent<PoolObject>();
			if ((bool)component)
			{
				PoolManager.Instance.CreatePool(component.gameObject, 1);
			}
		}
		foreach (DashAttackInstantiations item2 in instantiateOnCollision)
		{
			PoolObject component2 = item2.prefabToInstantiate.GetComponent<PoolObject>();
			if ((bool)component2)
			{
				PoolManager.Instance.CreatePool(component2.gameObject, 1);
			}
		}
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
	}

	public void DashToPoint(Transform parentToMove, Vector2 point, float offset = 0f)
	{
		if (_currentCoroutine != null)
		{
			StopCoroutine(_currentCoroutine);
		}
		if (dealsDamage)
		{
			currentlyDealsDamage = true;
			AttackArea.OnEnter += OnPenitentEntersArea;
			CheckInitialArea();
		}
		_parentToMove = parentToMove;
		_targetPoint = point;
		Vector3 vector = _targetPoint - parentToMove.transform.position;
		_dashDir = vector;
		_screenShakeDir = vector;
		willStopOnCollision = false;
		if (checkCollisions)
		{
			RaycastHit2D[] array = new RaycastHit2D[1];
			if (Physics2D.LinecastNonAlloc(parentToMove.transform.position, _targetPoint, array, collisionMask) > 0)
			{
				Debug.DrawLine(array[0].point, array[0].point + Vector2.up * 0.25f, Color.red, 1f);
				_targetPoint = array[0].point - (Vector2)vector.normalized * offset;
				willStopOnCollision = true;
			}
		}
		if (dashAlongGround)
		{
			RaycastHit2D[] array2 = new RaycastHit2D[1];
			Vector2 vector2 = parentToMove.transform.position + Vector3.up * 0.1f;
			if (Physics2D.LinecastNonAlloc(vector2, vector2 - Vector2.up, array2, collisionMask) > 0)
			{
				base.transform.position = array2[0].point;
				_targetPoint.y = base.transform.position.y;
			}
		}
		Debug.DrawLine(_parentToMove.position, _targetPoint, Color.red);
		if (rotateTowardsDirection && currentRotatingFunction != null)
		{
			currentRotatingFunction(parentToMove, _targetPoint);
		}
		_currentCoroutine = StartCoroutine(GameplayUtils.LerpMoveWithCurveCoroutine(_parentToMove, _parentToMove.position, _targetPoint, curve, dashDuration, OnDashFinished, OnDashAdvanced));
	}

	public void Dash(Transform parentToMove, Vector2 direction, float distance, float offset = 0f, bool updateHit = false)
	{
		if (updateHit)
		{
			CreateHit();
		}
		if (_currentCoroutine != null)
		{
			StopCoroutine(_currentCoroutine);
			CheckInitialArea();
		}
		if (dealsDamage)
		{
			currentlyDealsDamage = true;
			AttackArea.OnEnter += OnPenitentEntersArea;
		}
		_parentToMove = parentToMove;
		_targetPoint = parentToMove.position + (Vector3)direction.normalized * distance;
		if (dashAlongGround)
		{
			RaycastHit2D[] array = new RaycastHit2D[1];
			Vector2 vector = parentToMove.transform.position + Vector3.up * 0.1f;
			if (Physics2D.LinecastNonAlloc(vector, vector - Vector2.up, array, collisionMask) > 0)
			{
				base.transform.position = array[0].point;
			}
		}
		willStopOnCollision = false;
		if (checkCollisions)
		{
			RaycastHit2D[] array2 = new RaycastHit2D[1];
			if (Physics2D.LinecastNonAlloc(parentToMove.transform.position + Vector3.up * 0.1f, _targetPoint, array2, collisionMask) > 0)
			{
				willStopOnCollision = true;
				_targetPoint = array2[0].point - direction * offset;
			}
		}
		_dashDir = direction;
		_screenShakeDir = direction;
		Debug.DrawLine(_parentToMove.position, _targetPoint, Color.red, 15f);
		if (rotateTowardsDirection && currentRotatingFunction != null)
		{
			currentRotatingFunction(parentToMove, _targetPoint);
		}
		float seconds = dashDuration;
		if (useSpeed)
		{
			float num = Vector2.Distance(_parentToMove.position, _targetPoint);
			seconds = num / speed;
		}
		alreadyInstantiated = new List<DashAttackInstantiations>();
		_currentCoroutine = StartCoroutine(GameplayUtils.LerpMoveWithCurveCoroutine(_parentToMove, _parentToMove.position, _targetPoint, curve, seconds, OnDashFinished, OnDashAdvanced));
	}

	public void StopDash(Transform parentToMove, bool launchFinishedCallback = true)
	{
		if (_currentCoroutine != null)
		{
			StopCoroutine(_currentCoroutine);
		}
		if (launchFinishedCallback)
		{
			OnDashFinished(parentToMove);
		}
	}

	public Vector3 GetTargetPoint()
	{
		return _targetPoint;
	}

	private void OnPenitentEntersArea(object sender, Collider2DParam e)
	{
		GameObject gameObject = e.Collider2DArg.gameObject;
		if (stopsOnBlock)
		{
			_hit.OnGuardCallback = OnDashGuarded;
		}
		gameObject.GetComponentInParent<IDamageable>().Damage(_hit);
	}

	private void CheckInitialArea()
	{
		if (dealsDamageOnStart)
		{
			GetComponentInChildren<Weapon>().Attack(_hit);
		}
	}

	private void OnDashGuarded(Hit h)
	{
		Debug.Log("<color=cyan>DASH GUARDED</color>");
		if (this.OnDashBlockedEvent != null)
		{
			this.OnDashBlockedEvent(this);
		}
		StopDash(_parentToMove);
	}

	private void OnDashAdvanced(float nvalue)
	{
		foreach (DashAttackInstantiations item in objectsToInstantiate)
		{
			if (item.dashMoment <= nvalue && !alreadyInstantiated.Contains(item))
			{
				InstantiateObject(item);
			}
		}
		if (this.OnDashAdvancedEvent != null)
		{
			this.OnDashAdvancedEvent(nvalue);
		}
	}

	public void OnDashFinished(Transform parentToMove)
	{
		if (!(parentToMove == null))
		{
			parentToMove.rotation = Quaternion.identity;
			if (dealsDamage && AttackArea != null)
			{
				currentlyDealsDamage = false;
				AttackArea.OnEnter -= OnPenitentEntersArea;
			}
			if (screenshakeOnEnd)
			{
				Core.Logic.CameraManager.ProCamera2DShake.Shake(shakeDuration, _screenShakeDir * shakeForce, vibrato, 0.1f, 0f, default(Vector3), 0.06f);
			}
			if (willStopOnCollision && instantiateOnCollision != null && instantiateOnCollision.Count > 0)
			{
				InstantiateCollisionEffect();
			}
			if (this.OnDashFinishedEvent != null)
			{
				this.OnDashFinishedEvent();
			}
		}
	}

	private void InstantiateCollisionEffect()
	{
		foreach (DashAttackInstantiations item in instantiateOnCollision)
		{
			InstantiateObject(item);
		}
	}

	private void InstantiateObject(DashAttackInstantiations objectConfig)
	{
		alreadyInstantiated.Add(objectConfig);
		PoolObject component = objectConfig.prefabToInstantiate.GetComponent<PoolObject>();
		GameObject gameObject = ((!(component != null)) ? UnityEngine.Object.Instantiate(objectConfig.prefabToInstantiate, _parentToMove.position + (Vector3)objectConfig.offset, Quaternion.identity) : PoolManager.Instance.ReuseObject(component.gameObject, _parentToMove.position + (Vector3)objectConfig.offset, Quaternion.identity).GameObject);
		if (!objectConfig.keepRotation)
		{
			gameObject.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(_dashDir.y, _dashDir.x) * 57.29578f);
		}
		BossSpawnedAreaAttack component2 = gameObject.GetComponent<BossSpawnedAreaAttack>();
		if (component2 != null)
		{
			component2.SetOwner(base.EntityOwner);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(_targetPoint, 0.25f);
		Gizmos.DrawWireSphere(_targetPoint, 5f);
	}

	public void CreateHit()
	{
		_hit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageAmount = damage,
			DamageType = DamageType,
			DamageElement = DamageElement,
			Unnavoidable = unavoidable,
			HitSoundId = HitSound,
			Unblockable = unblockable,
			Force = Force
		};
	}

	public void SetDamage(int damage)
	{
		if (damage >= 0)
		{
			this.damage = damage;
			CreateHit();
		}
	}

	private void OnDeath()
	{
		base.EntityOwner.OnDeath -= OnDeath;
		if (_parentToMove != null)
		{
			StopDash(_parentToMove);
		}
	}

	public void SetSpawnsDamage(int damage)
	{
		foreach (DashAttackInstantiations item in objectsToInstantiate)
		{
			item.prefabToInstantiate.GetComponent<IDirectAttack>()?.SetDamage(damage);
		}
		CreateSpawnsHits();
	}

	public void CreateSpawnsHits()
	{
		foreach (DashAttackInstantiations item in objectsToInstantiate)
		{
			item.prefabToInstantiate.GetComponent<IDirectAttack>()?.CreateHit();
		}
	}

	public bool IsCurrentlyDealingDamage()
	{
		return currentlyDealsDamage;
	}

	public void AttachShowScriptIfNeeded()
	{
	}
}
