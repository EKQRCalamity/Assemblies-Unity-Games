using System;
using DG.Tweening;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Isidora;

public class HomingBonfireBehaviour : EnemyBehaviour
{
	private enum HomingBonfireStates
	{
		Idle,
		Attack,
		ChargeIsidora
	}

	[FoldoutGroup("Attack Settings", 0)]
	public float AttackCooldown;

	[FoldoutGroup("Attack Settings", 0)]
	public HomingBonfireAttack BonfireAttack;

	[FoldoutGroup("Halo GameObjects", 0)]
	public GameObject HaloMaskGameObject;

	[FoldoutGroup("Halo GameObjects", 0)]
	public GameObject RampLight;

	[FoldoutGroup("Charge Settings", 0)]
	public AnimationCurve ChargeRate;

	[HideInInspector]
	public float TimeToMaxRate;

	[FoldoutGroup("Debug", 0)]
	public bool IsSpawningIsidora;

	[FoldoutGroup("Debug", 0)]
	public bool IsActive;

	[FoldoutGroup("Debug", 0)]
	public bool IsChargingIsidora;

	[FoldoutGroup("Debug", 0)]
	public bool IsFullyCharged;

	private HomingBonfireStates CurrentState;

	private HomingBonfire HomingBonfire { get; set; }

	private StateMachine StateMachine { get; set; }

	public override void OnAwake()
	{
		base.OnAwake();
		StateMachine = GetComponent<StateMachine>();
		IsSpawningIsidora = true;
	}

	public override void OnStart()
	{
		base.OnStart();
		HomingBonfire = (HomingBonfire)Entity;
		SwitchToState(HomingBonfireStates.Idle);
		HaloMaskGameObject.SetActive(value: false);
		RampLight.SetActive(value: false);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		switch (CurrentState)
		{
		case HomingBonfireStates.Idle:
			Idle();
			break;
		case HomingBonfireStates.Attack:
			Attack();
			break;
		case HomingBonfireStates.ChargeIsidora:
			ChargeIsidora();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void ActivateBonfire(bool changeMask, float animDelay = 1f, float enlargeDelay = 0f)
	{
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(animDelay);
		sequence.OnComplete(delegate
		{
			SetActive(active: true);
		});
		sequence.Play();
		if (changeMask)
		{
			HaloMaskGameObject.transform.localScale = Vector3.one * 0.01f;
			RampLight.transform.localScale = Vector3.one * 0.01f;
			HaloMaskGameObject.SetActive(value: true);
			RampLight.SetActive(value: true);
			Ease ease = Ease.OutCubic;
			Ease ease2 = Ease.OutCubic;
			float duration = 2f;
			float duration2 = 1f;
			Sequence sequence2 = DOTween.Sequence();
			sequence2.SetDelay(enlargeDelay);
			sequence2.Append(HaloMaskGameObject.transform.DOScale(new Vector3(2.4f, 2.5f, 1f), duration).SetEase(ease));
			sequence2.AppendInterval(2f);
			sequence2.Append(HaloMaskGameObject.transform.DOScale(Vector3.zero, duration2).SetEase(ease2));
			sequence2.Play();
			Sequence sequence3 = DOTween.Sequence();
			sequence3.SetDelay(enlargeDelay);
			sequence3.Append(RampLight.transform.DOScale(new Vector3(2.5f, 2.5f, 1f), duration).SetEase(ease));
			sequence3.AppendInterval(2.5f);
			sequence3.Append(RampLight.transform.DOScale(Vector3.zero, duration2).SetEase(ease2));
			sequence3.Play();
		}
	}

	public void StartChargingIsidora(float timeToMaxRate)
	{
		if (!IsActive)
		{
			ActivateBonfire(changeMask: true);
		}
		IsChargingIsidora = true;
		BonfireAttack.ChargesIsidora = true;
		TimeToMaxRate = timeToMaxRate;
	}

	public void EnlargeMask()
	{
		IsFullyCharged = true;
		SetActive(active: true);
		HaloMaskGameObject.transform.DOKill();
		RampLight.transform.DOKill();
		float duration = 10f;
		HaloMaskGameObject.transform.DOScale(Vector3.one * 100f, duration).SetEase(Ease.InQuad).OnComplete(delegate
		{
			HaloMaskGameObject.transform.localScale = Vector3.one * 100f;
		});
		RampLight.transform.DOScale(Vector3.one * 100f, duration).SetEase(Ease.InQuad).OnComplete(delegate
		{
			RampLight.transform.localScale = Vector3.one * 100f;
		});
	}

	public void DeactivateBonfire(bool changeMask, bool explode = false)
	{
		IsFullyCharged = IsFullyCharged && !changeMask;
		SetActive(active: false, explode);
		if (changeMask)
		{
			HaloMaskGameObject.transform.DOKill();
			RampLight.transform.DOKill();
			float delay = ((!explode) ? 0f : 2f);
			float duration = 1f;
			HaloMaskGameObject.transform.DOScale(Vector3.one * 0.01f, duration).SetEase(Ease.InQuad).SetDelay(delay)
				.OnComplete(delegate
				{
					HaloMaskGameObject.SetActive(value: false);
				});
			RampLight.transform.DOScale(Vector3.one * 0.01f, duration).SetEase(Ease.InQuad).SetDelay(delay)
				.OnComplete(delegate
				{
					RampLight.SetActive(value: false);
				});
		}
	}

	public void SetupAttack(int numProjectiles, Vector2 castingPosition, float horizontalSpacingFactor, float verticalSpacingFactor)
	{
		SetupAttack(AttackCooldown, numProjectiles, useCastingPosition: true, castingPosition, horizontalSpacingFactor, verticalSpacingFactor);
	}

	public void SetupAttack(float attackCooldown, int numProjectiles, bool useCastingPosition, Vector2 castingPosition, float horizontalSpacingFactor, float verticalSpacingFactor)
	{
		AttackCooldown = attackCooldown;
		BonfireAttack.NumProjectiles = numProjectiles;
		BonfireAttack.UseCastingPosition = useCastingPosition;
		BonfireAttack.CastingPosition = castingPosition;
		BonfireAttack.HorizontalSpacingFactor = horizontalSpacingFactor;
		BonfireAttack.VerticalSpacingFactor = verticalSpacingFactor;
	}

	public void SetActive(bool active, bool explode = false)
	{
		if (active)
		{
			ActivateHalfCharged();
			if (IsFullyCharged)
			{
				ActivateFullCharged();
			}
		}
		else
		{
			DeactivateHalfCharged();
			DeactivateFullCharged();
		}
		if (explode)
		{
			ActivateExplode();
		}
		IsActive = active;
	}

	public void FireProjectile()
	{
		BonfireAttack.FireProjectile();
	}

	public void ActivateHalfCharged()
	{
		HomingBonfire.AnimationInyector.SetParamHalf(paramValue: true);
	}

	public void ActivateFullCharged()
	{
		HomingBonfire.AnimationInyector.SetParamFull(paramValue: true);
	}

	public void DeactivateHalfCharged()
	{
		HomingBonfire.AnimationInyector.SetParamHalf(paramValue: false);
	}

	public void DeactivateFullCharged()
	{
		HomingBonfire.AnimationInyector.SetParamFull(paramValue: false);
	}

	public void ActivateExplode()
	{
		HomingBonfire.AnimationInyector.SetParamExplode();
	}

	public override void Idle()
	{
		if (IsActive && !IsSpawningIsidora)
		{
			if (IsChargingIsidora)
			{
				SwitchToState(HomingBonfireStates.ChargeIsidora);
			}
			else
			{
				SwitchToState(HomingBonfireStates.Attack);
			}
		}
	}

	public override void Attack()
	{
		if (!IsActive || IsChargingIsidora)
		{
			if (IsChargingIsidora)
			{
				SwitchToState(HomingBonfireStates.ChargeIsidora);
			}
			else
			{
				SwitchToState(HomingBonfireStates.Idle);
			}
		}
	}

	public void ChargeIsidora()
	{
		if (!IsChargingIsidora || !IsActive)
		{
			if (IsActive)
			{
				SwitchToState(HomingBonfireStates.Attack);
			}
			else
			{
				SwitchToState(HomingBonfireStates.Idle);
			}
		}
	}

	private void SwitchToState(HomingBonfireStates targetState)
	{
		CurrentState = targetState;
		switch (targetState)
		{
		case HomingBonfireStates.Idle:
			StateMachine.SwitchState<HomingBonfireIdleState>();
			break;
		case HomingBonfireStates.Attack:
			StateMachine.SwitchState<HomingBonfireAttackState>();
			break;
		case HomingBonfireStates.ChargeIsidora:
			StateMachine.SwitchState<HomingBonfireChargeIsidoraState>();
			break;
		}
	}

	public bool IsAnyProjectileVisible()
	{
		return BonfireAttack.IsAnyProjectileActive();
	}

	public override void Damage()
	{
	}

	public override void Wander()
	{
	}

	public override void Chase(Transform targetPosition)
	{
	}

	public override void StopMovement()
	{
	}
}
