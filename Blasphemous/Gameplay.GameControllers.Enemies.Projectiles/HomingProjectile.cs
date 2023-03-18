using System;
using DG.Tweening;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Projectiles;

[RequireComponent(typeof(Rigidbody2D))]
public class HomingProjectile : Projectile
{
	[Header("Speed")]
	[FoldoutGroup("Motion Settings", 0)]
	public float Speed;

	[FoldoutGroup("Motion Settings", 0)]
	[Range(0f, 10f)]
	public float speedRandomRange;

	[FoldoutGroup("Motion Settings", 0)]
	[Range(0f, 1f)]
	public float InitialSpeedFactor = 0.5f;

	[Header("Acceleration")]
	[FoldoutGroup("Motion Settings", 0)]
	[Tooltip("Time to reach max speed")]
	public float Acceleration;

	[FoldoutGroup("Motion Settings", 0)]
	public AnimationCurve AccelerationEase;

	[Header("Rotation")]
	[FoldoutGroup("Motion Settings", 0)]
	public float RotateSpeed;

	[FoldoutGroup("Motion Settings", 0)]
	public Vector2 TargetOffset = Vector2.up;

	[FoldoutGroup("Motion Settings", 0)]
	public float TargetOffsetFactor;

	[FoldoutGroup("Motion Settings", 0)]
	[Range(0f, 90f)]
	public float rotateSpeedRandomRange;

	[FoldoutGroup("Motion Settings", 0)]
	public bool TargetsPenitent = true;

	[FoldoutGroup("Motion Settings", 0)]
	public Transform AlternativeTarget;

	[FoldoutGroup("Motion Settings", 0)]
	public bool DestroyedOnReachingTarget = true;

	[FoldoutGroup("Motion Settings", 0)]
	public bool ChangesRotatesSpeedInFlight;

	[FoldoutGroup("Motion Settings", 0)]
	[ShowIf("ChangesRotatesSpeedInFlight", true)]
	public float RotateSpeedWhenHorMoving;

	[FoldoutGroup("Motion Settings", 0)]
	[ShowIf("ChangesRotatesSpeedInFlight", true)]
	public float RotateSpeedWhenVerMoving;

	[FoldoutGroup("Motion Settings", 0)]
	public bool ChangesSortingOrderInFlight;

	[FoldoutGroup("Motion Settings", 0)]
	[ShowIf("ChangesSortingOrderInFlight", true)]
	[HideIf("ChangeOrderWhenVerMoving", true)]
	public bool ChangeOrderWhenHorMoving;

	[FoldoutGroup("Motion Settings", 0)]
	[ShowIf("ChangesSortingOrderInFlight", true)]
	[HideIf("ChangeOrderWhenHorMoving", true)]
	public bool ChangeOrderWhenVerMoving;

	[FoldoutGroup("Motion Settings", 0)]
	[ShowIf("ChangesSortingOrderInFlight", true)]
	public int PosMoveSortingOrder;

	[FoldoutGroup("Motion Settings", 0)]
	[ShowIf("ChangesSortingOrderInFlight", true)]
	public int NegMoveSortingOrder;

	[FoldoutGroup("Debug", 0)]
	public Vector2 currentDirection;

	private float currentSpeed;

	private float instanceSpeed;

	private float instanceRotationSpeed;

	private float crossColorHue;

	private Rigidbody2D Rigidbody { get; set; }

	public event Action<Projectile> OnDisableEvent;

	private void OnEnable()
	{
		ResetTTL();
		Randomize();
		Accelerate();
		crossColorHue = UnityEngine.Random.Range(0f, 1f);
	}

	private void OnDisable()
	{
		if (this.OnDisableEvent != null)
		{
			this.OnDisableEvent(this);
		}
	}

	public void ResetSpeed()
	{
		instanceSpeed = Speed;
	}

	public void ResetRotateSpeed()
	{
		instanceRotationSpeed = RotateSpeed;
	}

	private void Randomize()
	{
		instanceRotationSpeed = RotateSpeed + UnityEngine.Random.Range(0f - rotateSpeedRandomRange, rotateSpeedRandomRange);
		instanceSpeed = Speed + UnityEngine.Random.Range(0f - speedRandomRange, speedRandomRange);
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		Rigidbody = GetComponent<Rigidbody2D>();
		currentDirection = base.transform.right;
	}

	protected override void OnUpdate()
	{
		_currentTTL -= Time.deltaTime;
		if (_currentTTL < 0f)
		{
			OnLifeEnded();
		}
		if (DestroyedOnReachingTarget && !TargetsPenitent && Vector2.Distance(base.transform.position, CalculateTargetPosition()) < 0.5f)
		{
			_currentTTL = 0f;
		}
		if (ChangesRotatesSpeedInFlight)
		{
			UpdateRotatesSpeedInFlight();
		}
		if (ChangesSortingOrderInFlight)
		{
			UpdateSortingOrderInFlight();
		}
	}

	public void SetTTL(float ttl)
	{
		_currentTTL = ttl;
	}

	private void UpdateSortingOrderInFlight()
	{
		bool flag = (ChangeOrderWhenHorMoving && currentDirection.x > 0f) || (ChangeOrderWhenVerMoving && currentDirection.y > 0f);
		spriteRenderer.sortingOrder = ((!flag) ? NegMoveSortingOrder : PosMoveSortingOrder);
	}

	private void UpdateRotatesSpeedInFlight()
	{
		Vector2 normalized = currentDirection.normalized;
		instanceRotationSpeed = RotateSpeedWhenHorMoving * Mathf.Abs(normalized.x) + RotateSpeedWhenVerMoving * Mathf.Abs(normalized.y);
	}

	protected override void OnFixedUpdated()
	{
		HomingDisplacement();
		GizmoExtensions.DrawDebugCross(base.transform.position, Color.HSVToRGB(crossColorHue, 1f, 1f), 5f);
	}

	private void HomingDisplacement()
	{
		Vector2 vector = CalculateTargetPosition();
		Vector2 normalized = ((Vector2)base.transform.position - vector).normalized;
		float num = Mathf.Sign(Vector3.Cross(normalized, velocity).z);
		float z = instanceRotationSpeed * num * Time.deltaTime;
		Quaternion quaternion = Quaternion.Euler(0f, 0f, z);
		currentDirection = quaternion * currentDirection;
		velocity = currentDirection * currentSpeed;
		base.transform.position += (Vector3)velocity * Time.deltaTime;
	}

	public Vector2 CalculateTargetPosition()
	{
		Vector2 vector = TargetOffset * (1f + TargetOffsetFactor * Mathf.Pow(_currentTTL / timeToLive, 5f));
		Vector2 vector2 = ((!TargetsPenitent) ? ((Vector2)AlternativeTarget.position) : ((Vector2)Core.Logic.Penitent.GetPosition()));
		return vector2 + vector;
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			Vector2 vector = CalculateTargetPosition();
			Gizmos.DrawSphere(vector, 0.5f);
			Gizmos.DrawLine(vector, base.transform.position);
			Gizmos.DrawLine((Vector2)base.transform.position + velocity, base.transform.position);
		}
	}

	public bool ChangeTargetToAlternative(Transform transform, float speedFactor, float accelerationTimeFactor, float rotateSpeedFactor)
	{
		if (!TargetsPenitent)
		{
			return false;
		}
		TargetsPenitent = false;
		AlternativeTarget = transform;
		AccelerateAlternative(speedFactor, accelerationTimeFactor, rotateSpeedFactor);
		ResetTTL();
		return true;
	}

	public bool ChangeTargetToPenitent(bool changeTargetOnlyIfInactive)
	{
		if (TargetsPenitent || (changeTargetOnlyIfInactive && base.gameObject.activeInHierarchy))
		{
			return false;
		}
		TargetsPenitent = true;
		Accelerate();
		ResetTTL();
		return true;
	}

	private void Accelerate()
	{
		base.transform.DOKill();
		if (currentSpeed == 0f)
		{
			currentSpeed = instanceSpeed * InitialSpeedFactor;
		}
		float startValue = currentSpeed;
		DOTween.To(delegate(float x)
		{
			currentSpeed = x;
		}, startValue, instanceSpeed, Acceleration).SetEase(AccelerationEase);
	}

	private void AccelerateAlternative(float speedFactor, float accelerationTimeFactor, float rorateSpeedFactor)
	{
		base.transform.DOKill();
		instanceRotationSpeed = RotateSpeed * rorateSpeedFactor;
		if (currentSpeed == 0f)
		{
			currentSpeed = instanceSpeed * InitialSpeedFactor;
		}
		float startValue = currentSpeed;
		DOTween.To(delegate(float x)
		{
			currentSpeed = x;
		}, startValue, instanceSpeed * speedFactor, Acceleration * accelerationTimeFactor).SetEase(AccelerationEase);
	}
}
