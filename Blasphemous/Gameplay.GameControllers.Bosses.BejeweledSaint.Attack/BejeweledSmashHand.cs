using DG.Tweening;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BejeweledSaint.Attack;

public class BejeweledSmashHand : Weapon
{
	public static Core.SimpleEvent OnHandDown;

	public AnimationCurve AppearingMoveCurve;

	public float MinHeight;

	public float MaxHeight;

	public GameObject impactFx;

	[EventRef]
	public string HandSmashHitFx;

	public Transform impactTransform;

	protected Animator Animator;

	private Hit _smashHandHit;

	public bool IsRaised { get; set; }

	protected SpriteRenderer SpriteRender { get; set; }

	public AttackArea AttackArea { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		WeaponOwner = Object.FindObjectOfType<BejeweledSaintHead>();
		Animator = GetComponent<Animator>();
		SpriteRender = GetComponent<SpriteRenderer>();
		AttackArea = GetComponentInChildren<AttackArea>();
		PoolManager.Instance.CreatePool(impactFx, 1);
	}

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea.Entity = WeaponOwner;
		_smashHandHit = new Hit
		{
			AttackingEntity = base.gameObject,
			DamageAmount = WeaponOwner.Stats.Strength.Final * 0.5f,
			DamageType = DamageArea.DamageType.Normal,
			Force = 0f,
			HitSoundId = HandSmashHitFx
		};
	}

	public void SmashAttack()
	{
		Attack(_smashHandHit);
		PoolManager.Instance.ReuseObject(impactFx, impactTransform.position + Vector3.up * 2f, Quaternion.identity);
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	public void AttackAppearing()
	{
		SetSmoothYPos(MaxHeight, 0.5f, AnimatorAttack);
	}

	public void Disappear()
	{
		if (DOTween.IsTweening(base.transform))
		{
			DOTween.Kill(base.transform);
		}
		SetSmoothYPos(MinHeight, 1f, OnDissapear);
	}

	private void SetSmoothYPos(float yPos, float time, TweenCallback endCallback)
	{
		base.transform.DOLocalMoveY(yPos, time).SetEase(AppearingMoveCurve).OnComplete(endCallback)
			.SetId("VerticalMotion");
	}

	public void OnDissapear()
	{
		if (OnHandDown != null)
		{
			OnHandDown();
		}
	}

	public void AnimatorAttack()
	{
		Animator.SetTrigger("ATTACK");
	}

	public void PlaySmash()
	{
		BejeweledSaintHead bejeweledSaintHead = (BejeweledSaintHead)WeaponOwner;
		if (bejeweledSaintHead != null)
		{
			bejeweledSaintHead.WholeBoss.Audio.PlayHandStomp();
		}
	}

	public void DoCameraShake()
	{
		if (SpriteRender.isVisible)
		{
			Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("PietyStomp");
		}
	}
}
