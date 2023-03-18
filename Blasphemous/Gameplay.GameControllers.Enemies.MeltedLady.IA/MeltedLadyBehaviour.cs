using System;
using System.Collections;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.MeltedLady.Attack;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.MeltedLady.IA;

public class MeltedLadyBehaviour : EnemyBehaviour
{
	[FoldoutGroup("Activation Settings", true, 0)]
	[OnValueChanged("SetMaxAttackDistance", false)]
	public float ActivationDistance;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float MaxAttackDistance;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float AttackCoolDown = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float TeleportInterval = 0.6f;

	private WaitForSeconds _teleportYield;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float TeleportCooldown = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float AttackHeight = 2f;

	[FoldoutGroup("Attacks amount", true, 0)]
	public int AttackAmount = 3;

	private int _currentAttackAmount;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float AttackDistance = 2f;

	[FoldoutGroup("Floating Settings", true, 0)]
	public float AmplitudeY = 3f;

	[FoldoutGroup("Floating Settings", true, 0)]
	public float SpeedY = 1f;

	private float _index;

	private float _currentAttackLapse;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float DistanceToTarget { get; private set; }

	public float TeleportCooldownLapse { get; set; }

	public bool CanTeleport { get; set; }

	public bool IsInOrigin { get; set; }

	public int CurrentAttackAmount
	{
		get
		{
			return _currentAttackAmount;
		}
		set
		{
			_currentAttackAmount = Mathf.Clamp(value, 0, AttackAmount);
		}
	}

	public Vector2 OriginPosition { get; private set; }

	public FloatingLady MeltedLady { get; private set; }

	public MeltedLadyTeleportPoint[] TeleportPoints { get; private set; }

	public MeltedLadyTeleportPoint CurrentTeleportPoint { get; private set; }

	public bool Awaken { get; private set; }

	private void SetMaxAttackDistance()
	{
		MaxAttackDistance = Mathf.Clamp(MaxAttackDistance, ActivationDistance, float.MaxValue);
	}

	public override void OnAwake()
	{
		base.OnAwake();
		MeltedLady = (FloatingLady)Entity;
		_teleportYield = new WaitForSeconds(TeleportInterval);
	}

	public override void OnStart()
	{
		base.OnStart();
		MeltedLady.StateMachine.SwitchState<MeltedLadyIdleState>();
		Vector3 position = MeltedLady.transform.position;
		OriginPosition = new Vector2(position.x, position.y);
		CurrentAttackAmount = AttackAmount;
		TeleportPoints = UnityEngine.Object.FindObjectsOfType<MeltedLadyTeleportPoint>();
		if (TeleportPoints.Length < 1)
		{
			Debug.LogError("You have to add at least one teleport point to the scene.");
			MeltedLady.gameObject.SetActive(value: false);
		}
		MeltedLady.OnDamaged += OnDamaged;
	}

	private void OnDamaged()
	{
		if (!(MeltedLady is InkLady) || !MeltedLady.IsAttacking)
		{
			MeltedLady.Status.IsHurt = true;
			MeltedLady.DamageArea.DamageAreaCollider.enabled = false;
			StopCoroutine(TeleportCoroutine());
		}
	}

	private void Update()
	{
		if (!(MeltedLady.Target == null))
		{
			DistanceToTarget = Vector2.Distance(MeltedLady.transform.position, MeltedLady.Target.transform.position);
			if (MeltedLady.SpriteRenderer.isVisible)
			{
				Floating();
			}
			if (MeltedLady.Status.Dead || MeltedLady.Status.IsHurt)
			{
				MeltedLady.StateMachine.SwitchState<MeltedLadyDeathState>();
			}
			else if (DistanceToTarget <= ActivationDistance)
			{
				MeltedLady.StateMachine.SwitchState<MeltedLadyAttackState>();
			}
			else
			{
				MeltedLady.StateMachine.SwitchState<MeltedLadyIdleState>();
			}
		}
	}

	public override void Idle()
	{
		StopMovement();
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		MeltedLady.SetOrientation((!(targetPos.x > MeltedLady.transform.position.x)) ? EntityOrientation.Left : EntityOrientation.Right);
	}

	private void Floating()
	{
		base.transform.position += AmplitudeY * (Mathf.Sin((float)Math.PI * 2f * SpeedY * Time.time) - Mathf.Sin((float)Math.PI * 2f * SpeedY * (Time.time - Time.deltaTime))) * base.transform.up;
	}

	public void Teleport()
	{
		StartCoroutine(TeleportCoroutine());
	}

	private IEnumerator TeleportCoroutine()
	{
		CanTeleport = false;
		TeleportCooldownLapse = 0f;
		MeltedLady.AnimatorInyector.TeleportOut();
		MeltedLady.DamageArea.DamageAreaCollider.enabled = false;
		MeltedLady.DamageByContact = false;
		yield return _teleportYield;
		TeleportToTarget();
	}

	public void TeleportToTarget()
	{
		MeltedLady.transform.position = GetAttackPosition();
		LookAtTarget(MeltedLady.Target.transform.position);
		MeltedLady.AnimatorInyector.TeleportIn();
	}

	private Vector2 GetAttackPosition()
	{
		Vector3 vector = GetNearestTeleportPointToTarget().TeleportPosition;
		MeltedLady.Behaviour.IsInOrigin = false;
		float num = Vector2.Distance(MeltedLady.Behaviour.OriginPosition, vector);
		if (num >= MeltedLady.Behaviour.MaxAttackDistance)
		{
			vector = MeltedLady.Behaviour.OriginPosition;
			MeltedLady.Behaviour.IsInOrigin = true;
		}
		return vector;
	}

	private MeltedLadyTeleportPoint GetNearestTeleportPointToTarget()
	{
		float num = float.PositiveInfinity;
		MeltedLadyTeleportPoint meltedLadyTeleportPoint = null;
		MeltedLadyTeleportPoint[] teleportPoints = TeleportPoints;
		foreach (MeltedLadyTeleportPoint meltedLadyTeleportPoint2 in teleportPoints)
		{
			if (!meltedLadyTeleportPoint2.Equals(CurrentTeleportPoint))
			{
				float num2 = Vector2.Distance(meltedLadyTeleportPoint2.transform.position, MeltedLady.Target.transform.position);
				if (num2 < num)
				{
					num = num2;
					meltedLadyTeleportPoint = meltedLadyTeleportPoint2;
				}
			}
		}
		CurrentTeleportPoint = meltedLadyTeleportPoint;
		return meltedLadyTeleportPoint;
	}

	public void Chase(Vector3 position)
	{
	}

	public override void Damage()
	{
	}

	public void Death()
	{
	}

	public void ResetCoolDown()
	{
	}

	public void ResetAttackCounter()
	{
		if (CurrentAttackAmount < AttackAmount)
		{
			CurrentAttackAmount = AttackAmount;
		}
	}

	public override void Attack()
	{
		MeltedLady.AnimatorInyector.Attack();
	}

	public override void StopMovement()
	{
	}

	public override void Wander()
	{
	}

	public override void Chase(Transform targetPosition)
	{
	}

	private void OnDestroy()
	{
		MeltedLady.OnDamaged -= OnDamaged;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.magenta;
	}
}
