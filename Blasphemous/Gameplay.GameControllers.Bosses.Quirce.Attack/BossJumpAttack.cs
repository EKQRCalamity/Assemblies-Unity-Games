using System;
using System.Collections;
using System.Collections.Generic;
using BezierSplines;
using Framework.Managers;
using Framework.Pooling;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Quirce.Attack;

public class BossJumpAttack : EnemyAttack, IDirectAttack
{
	public BezierSpline jumpCurve;

	public AnimationCurve easingCurve;

	public LayerMask groundLayerMask;

	public float moveSeconds;

	public GameObject instantiateOnStart;

	public GameObject instantiateOnEnd;

	[FoldoutGroup("Instantiations", 0)]
	public List<DashAttackInstantiations> objectsToInstantiate;

	private List<DashAttackInstantiations> alreadyInstantiated;

	[FoldoutGroup("Damage", 0)]
	public bool dealsDamage = true;

	[ShowIf("dealsDamage", true)]
	[FoldoutGroup("Damage", 0)]
	public bool unavoidable;

	[ShowIf("dealsDamage", true)]
	[FoldoutGroup("Damage", 0)]
	public int damage;

	[FoldoutGroup("FX", 0)]
	public bool screenshakeOnEnd;

	[FoldoutGroup("FX", 0)]
	public bool ShockWaveOnEnd = true;

	[ShowIf("screenshakeOnEnd", true)]
	[FoldoutGroup("FX", 0)]
	public int vibrato = 40;

	[ShowIf("screenshakeOnEnd", true)]
	[FoldoutGroup("FX", 0)]
	public float shakeForce;

	[ShowIf("screenshakeOnEnd", true)]
	[FoldoutGroup("FX", 0)]
	public float shakeDuration;

	private Transform _parentToMove;

	private Coroutine _currentCoroutine;

	private Vector2 _lastPoint;

	private Hit _hit;

	private bool forceJumpEnd;

	public AttackArea AttackArea { get; set; }

	public event Action<Vector2> OnJumpAdvancedEvent;

	public event Action OnJumpLanded;

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
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
	}

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea = GetComponentInChildren<AttackArea>();
		if (dealsDamage)
		{
			CreateHit();
		}
		CheckPools();
	}

	public bool IsInsideWall(Vector2 p)
	{
		RaycastHit2D[] results = new RaycastHit2D[1];
		p += Vector2.up;
		int num = Physics2D.LinecastNonAlloc(p + Vector2.right * 0.1f, p, results, groundLayerMask);
		if (num > 0)
		{
			return true;
		}
		return false;
	}

	public Vector2 GetPointOutOfWall(Vector2 targetPoint, Vector2 dir, float skinWidth)
	{
		int num = 0;
		bool flag = false;
		Vector2 vector = Vector2.zero;
		RaycastHit2D[] array = new RaycastHit2D[1];
		targetPoint += Vector2.up * 2f;
		while (!flag && num < 100)
		{
			num++;
			int num2 = Physics2D.LinecastNonAlloc(targetPoint + dir * skinWidth, targetPoint, array, groundLayerMask);
			if (num2 > 0)
			{
				vector = array[0].point;
				Debug.DrawLine(vector, targetPoint, Color.red, 6f);
				targetPoint += dir * skinWidth;
			}
			else
			{
				vector += dir * skinWidth;
				Debug.DrawLine(vector, targetPoint, Color.green, 6f);
				flag = true;
			}
		}
		return vector;
	}

	public void Use(Transform parentToMove, Vector3 targetPoint)
	{
		_parentToMove = parentToMove;
		OnJumped();
		if (IsInsideWall(targetPoint))
		{
			targetPoint = GetPointOutOfWall(targetPoint, Vector2.right * Mathf.Sign(parentToMove.transform.position.x - targetPoint.x), 0.5f);
		}
		RaycastHit2D[] array = new RaycastHit2D[1];
		int num = Physics2D.LinecastNonAlloc((Vector2)targetPoint + Vector2.up * 2f, (Vector2)targetPoint + Vector2.down * 10f, array, groundLayerMask);
		if (num > 0)
		{
			Debug.DrawLine(targetPoint + Vector3.up, targetPoint + Vector3.down * 10f, Color.green);
			targetPoint = array[0].point;
			_lastPoint = targetPoint;
			Vector3 vector = jumpCurve.GetControlPoint(2) - jumpCurve.GetControlPoint(3);
			jumpCurve.SetControlPoint(3, parentToMove.InverseTransformPoint(targetPoint));
			jumpCurve.SetControlPoint(2, parentToMove.InverseTransformPoint(targetPoint) + vector);
			StartCoroutine(JumpCoroutine(parentToMove, OnLanded));
		}
		else
		{
			Debug.DrawLine(targetPoint + Vector3.up, targetPoint + Vector3.down * 10f, Color.red);
			GameplayUtils.DrawDebugCross(targetPoint + Vector3.down * 10f, Color.red, 5f);
			GameplayUtils.DrawDebugCross(targetPoint + Vector3.up * 2f, Color.yellow, 5f);
			GameObject gameObject = new GameObject("DEBUG_JUMP_ERROR");
			gameObject.transform.position = targetPoint;
			Debug.LogError("COULDNT JUMP, THERES NO FLOOR");
			OnLanded();
		}
	}

	public void StopJump()
	{
		forceJumpEnd = true;
		AttackArea.OnEnter -= OnPenitentEntersArea;
	}

	private IEnumerator JumpCoroutine(Transform parentToMove, Action callback = null)
	{
		alreadyInstantiated = new List<DashAttackInstantiations>();
		forceJumpEnd = false;
		float counter = 0f;
		Vector3 originPos = parentToMove.position;
		while (counter < moveSeconds && !forceJumpEnd)
		{
			float normalized = counter / moveSeconds;
			OnJumpAdvanced(normalized);
			float eased = easingCurve.Evaluate(normalized);
			parentToMove.position = originPos + parentToMove.InverseTransformPoint(jumpCurve.GetPoint(eased));
			Debug.DrawLine(base.transform.position, parentToMove.position + Vector3.up * 0.1f, Color.green);
			counter += Time.deltaTime;
			yield return null;
		}
		OnJumpAdvanced(1f);
		parentToMove.position = originPos + parentToMove.InverseTransformPoint(jumpCurve.GetPoint(1f));
		callback?.Invoke();
	}

	private void OnJumpAdvanced(float nvalue)
	{
		if (this.OnJumpAdvancedEvent != null)
		{
			this.OnJumpAdvancedEvent(GetCurveDirection(nvalue));
		}
		foreach (DashAttackInstantiations item in objectsToInstantiate)
		{
			if (item.dashMoment <= nvalue && !alreadyInstantiated.Contains(item))
			{
				InstantiateObject(item);
			}
		}
	}

	private Vector2 GetCurveDirection(float n)
	{
		return jumpCurve.GetDirection(n);
	}

	private void InstantiateObject(DashAttackInstantiations objectConfig)
	{
		alreadyInstantiated.Add(objectConfig);
		PoolObject component = objectConfig.prefabToInstantiate.GetComponent<PoolObject>();
		GameObject gameObject = ((!(component != null)) ? UnityEngine.Object.Instantiate(objectConfig.prefabToInstantiate, _parentToMove.position + (Vector3)objectConfig.offset, Quaternion.identity) : PoolManager.Instance.ReuseObject(component.gameObject, _parentToMove.position + (Vector3)objectConfig.offset, Quaternion.identity).GameObject);
		BossSpawnedAreaAttack component2 = gameObject.GetComponent<BossSpawnedAreaAttack>();
		if (component2 != null)
		{
			component2.SetOwner(base.EntityOwner);
		}
	}

	private void OnJumped()
	{
		if (dealsDamage)
		{
			AttackArea.OnEnter += OnPenitentEntersArea;
		}
	}

	private void OnLanded()
	{
		if (instantiateOnEnd != null)
		{
			InstantiateArea(instantiateOnEnd);
		}
		if (screenshakeOnEnd)
		{
			Core.Logic.CameraManager.ProCamera2DShake.Shake(shakeDuration, Vector3.down * shakeForce, vibrato, 0.2f, 0f, default(Vector3), 0f);
		}
		if (this.OnJumpLanded != null)
		{
			this.OnJumpLanded();
		}
		if (ShockWaveOnEnd)
		{
			Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 0.8f, 0.3f, 1.8f);
		}
		if (dealsDamage)
		{
			AttackArea.OnEnter -= OnPenitentEntersArea;
		}
	}

	private void InstantiateArea(GameObject toInstantiate)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(toInstantiate, _parentToMove.position, Quaternion.identity);
		BossSpawnedAreaAttack component = gameObject.GetComponent<BossSpawnedAreaAttack>();
		if (component != null)
		{
			component.SetOwner(base.EntityOwner);
		}
	}

	private void OnPenitentEntersArea(object sender, Collider2DParam e)
	{
		Debug.Log("ON PENITENT ENTERS THIS OBJECT AREA: " + base.gameObject.name);
		GameObject gameObject = e.Collider2DArg.gameObject;
		_hit.OnGuardCallback = OnGuard;
		gameObject.GetComponentInParent<IDamageable>().Damage(_hit);
	}

	private void OnGuard(Hit h)
	{
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(_lastPoint, 0.25f);
	}

	public void CreateHit()
	{
		_hit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageType = DamageType,
			Force = Force,
			DamageAmount = damage,
			HitSoundId = HitSound,
			Unnavoidable = unavoidable
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
}
