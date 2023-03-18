using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent.Gizmos;
using Sirenix.OdinInspector;
using UnityEngine;

public class FloatingWeapon : Entity
{
	public enum FLOATING_WEAPON_STATES
	{
		FLOATING,
		AIMING,
		STOP
	}

	[FoldoutGroup("Follow settings", 0)]
	public RootMotionDriver rootMotion;

	[FoldoutGroup("Follow settings", 0)]
	public Entity ownerEntity;

	[FoldoutGroup("Follow settings", 0)]
	public bool doFollow = true;

	[FoldoutGroup("Follow settings", 0)]
	public float targetAngle;

	[FoldoutGroup("Floating settings", 0)]
	public float floatingFrequency;

	[FoldoutGroup("Floating settings", 0)]
	public float floatingAmplitude;

	[FoldoutGroup("Floating settings", 0)]
	public float smoothFactor;

	[FoldoutGroup("Floating settings", 0)]
	public float maxAngle = 10f;

	private Vector2 _targetPosition;

	private Vector2 _followPosition;

	private Vector2 _floatingOffset;

	[FoldoutGroup("Sparks settings", 0)]
	public GameObject sparksPrefab;

	[FoldoutGroup("Sparks settings", 0)]
	public Vector2 collisionPoint;

	[FoldoutGroup("Sparks settings", 0)]
	public float collisionRadius;

	[FoldoutGroup("Sparks settings", 0)]
	public ContactFilter2D filter;

	[FoldoutGroup("Sparks settings", 0)]
	public float secondsBetweenInstances = 0.5f;

	private RaycastHit2D[] results;

	private float instantiationTimer;

	public FLOATING_WEAPON_STATES currentState;

	public bool isSpinning;

	public bool hidden;

	private Transform _aimTarget;

	public Vector2 aimOffset;

	private Animator animator;

	private void SetTargetPosition()
	{
		if (rootMotion == null)
		{
			return;
		}
		if (ownerEntity.Status.Orientation == EntityOrientation.Right)
		{
			_targetPosition = rootMotion.transform.position;
			SetOrientation(EntityOrientation.Right);
			if (currentState == FLOATING_WEAPON_STATES.FLOATING)
			{
				targetAngle = rootMotion.transform.localEulerAngles.z;
			}
		}
		else
		{
			_targetPosition = rootMotion.ReversePosition;
			SetOrientation(EntityOrientation.Left);
			if (currentState == FLOATING_WEAPON_STATES.FLOATING)
			{
				targetAngle = 0f - rootMotion.transform.localEulerAngles.z;
			}
		}
	}

	public void ChangeState(FLOATING_WEAPON_STATES st)
	{
		currentState = st;
	}

	private void SetAimingTarget(Transform aimTarget)
	{
		_aimTarget = aimTarget;
	}

	protected override void OnStart()
	{
		base.OnStart();
		animator = GetComponentInChildren<Animator>();
		results = new RaycastHit2D[5];
	}

	public void SetAutoFollow(bool follow)
	{
		doFollow = follow;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (doFollow)
		{
			SetTargetPosition();
		}
		switch (currentState)
		{
		case FLOATING_WEAPON_STATES.FLOATING:
			UpdateFloatingOffset();
			break;
		case FLOATING_WEAPON_STATES.AIMING:
			UpdateAimingAngle();
			break;
		}
		if (doFollow)
		{
			UpdatePositionAndRotation();
		}
		if (isSpinning)
		{
			CheckCollision();
		}
	}

	private void UpdatePositionAndRotation()
	{
		_followPosition = _targetPosition + _floatingOffset;
		base.transform.position = Vector3.Lerp(base.transform.position, _followPosition, smoothFactor);
		Quaternion b = Quaternion.Euler(0f, 0f, targetAngle);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, smoothFactor);
	}

	private void UpdateAimingAngle()
	{
		Vector2 vector = _aimTarget.transform.position + (Vector3)aimOffset - base.transform.position;
		targetAngle = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
	}

	private void CheckCollision()
	{
		if (instantiationTimer > 0f)
		{
			instantiationTimer -= Time.deltaTime;
			return;
		}
		instantiationTimer = secondsBetweenInstances;
		Vector2 origin = (Vector2)base.transform.position + collisionPoint;
		if (Physics2D.Raycast(origin, Vector2.down, filter, results, collisionRadius) > 0)
		{
			Vector2 point = results[0].point;
			Object.Instantiate(sparksPrefab, point, Quaternion.identity);
		}
	}

	public void AimToPlayer()
	{
		if (Core.Logic.Penitent != null)
		{
			SetAimingTarget(Core.Logic.Penitent.transform);
		}
	}

	private void UpdateFloatingOffset()
	{
		float y = Mathf.Sin(floatingFrequency * Time.time) * floatingAmplitude;
		_floatingOffset = new Vector2(0f, y);
	}

	public void SetSpinning(bool spin)
	{
		isSpinning = spin;
		instantiationTimer = secondsBetweenInstances;
		animator.SetBool("SPIN", spin);
	}

	public void Hide(bool vanishAnimation = false)
	{
		hidden = true;
		if (vanishAnimation)
		{
			animator.SetBool("SHOW", value: false);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void Show(bool vanishAnimation = false)
	{
		hidden = false;
		base.gameObject.SetActive(value: true);
		if (vanishAnimation)
		{
			animator.SetBool("SHOW", value: true);
		}
	}

	public void Activate(bool act, bool vanishAnimation = false)
	{
		if (act)
		{
			Show(vanishAnimation);
		}
		else
		{
			Hide(vanishAnimation);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(base.transform.position + (Vector3)collisionPoint, base.transform.position + (Vector3)collisionPoint + Vector3.down * collisionRadius);
	}
}
