using System;
using System.Linq;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Bosses.BejeweledSaint.IA;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Damage;
using Gameplay.GameControllers.Penitent.Gizmos;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BejeweledSaint.Attack;

public class BejeweledSaintArmAttack : EnemyAttack
{
	[Serializable]
	public struct FailedAttackTier
	{
		[Range(1f, 10f)]
		public int MinFailedAttackAmount;

		[Range(1f, 10f)]
		public int MaxFailedAttackAmount;
	}

	public FailedAttackTier[] FailedAttackTiers;

	public RootMotionDriver StaffRoot;

	public Enemy Owner;

	public Transform angleCastCenter;

	public Transform impactTransform;

	[FoldoutGroup("FX", 0)]
	public GameObject fxStaffSlash;

	[FoldoutGroup("FX", 0)]
	public GameObject fxStaffImpact;

	[FoldoutGroup("FX", 0)]
	public bool screenshakeOnEnd;

	[ShowIf("screenshakeOnEnd", true)]
	[FoldoutGroup("FX", 0)]
	public int vibrato = 40;

	[ShowIf("screenshakeOnEnd", true)]
	[FoldoutGroup("FX", 0)]
	public float shakeForce;

	[ShowIf("screenshakeOnEnd", true)]
	[FoldoutGroup("FX", 0)]
	public float shakeDuration;

	public float maxArmAngle = 22f;

	private AttackArea _attackArea;

	private Hit _weaponHit;

	private int _currentFailedAttacksLimit;

	public int AttacksMadeAmount { get; set; }

	public int SucceedAttacksAmount { get; set; }

	public bool CanFireSweepAttack => AttacksMadeAmount - SucceedAttacksAmount >= _currentFailedAttacksLimit;

	private void Awake()
	{
		base.CurrentEnemyWeapon = GetComponent<Weapon>();
		base.EntityOwner = Owner;
		_attackArea = GetComponentInChildren<AttackArea>();
		PoolManager.Instance.CreatePool(fxStaffSlash, 3);
		PoolManager.Instance.CreatePool(fxStaffImpact, 3);
	}

	protected override void OnStart()
	{
		base.OnStart();
		BejeweledSaintStaff.OnSucceedHit = (Core.SimpleEvent)Delegate.Combine(BejeweledSaintStaff.OnSucceedHit, new Core.SimpleEvent(OnSucceedHit));
		PenitentDamageArea.OnDamagedGlobal = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Combine(PenitentDamageArea.OnDamagedGlobal, new PenitentDamageArea.PlayerDamagedEvent(OnDamagedGlobal));
		_attackArea.OnEnter += OnEnterAttackArea;
		_weaponHit = new Hit
		{
			AttackingEntity = Owner.gameObject,
			DamageAmount = Owner.Stats.Strength.Final,
			DamageType = DamageType,
			Force = Force,
			Unnavoidable = true,
			HitSoundId = HitSound
		};
	}

	private void OnSucceedHit()
	{
		SucceedAttacksAmount++;
	}

	private void OnDamagedGlobal(Gameplay.GameControllers.Penitent.Penitent damaged, Hit hit)
	{
		if (hit.DamageType != 0)
		{
			BejeweledSaintBehaviour bejeweledSaintBehaviour = (BejeweledSaintBehaviour)Owner.EnemyBehaviour;
			Vector3 position = bejeweledSaintBehaviour.StaffRoot.transform.position;
			Core.Logic.Penitent.SetOrientationbyHit(position);
		}
	}

	private void OnEnterAttackArea(object sender, Collider2DParam e)
	{
		BejeweledSaintBehaviour bejeweledSaintBehaviour = (BejeweledSaintBehaviour)Owner.EnemyBehaviour;
		if (bejeweledSaintBehaviour.IsPerformingAttack)
		{
			StaffBasicAttack();
		}
	}

	private void SetAttackOrientation()
	{
		BejeweledSaintBehaviour component = Owner.GetComponent<BejeweledSaintBehaviour>();
		float x = Core.Logic.Penitent.transform.position.x;
		float x2 = component.StaffRoot.transform.position.x;
		EntityOrientation orientation = ((!(x >= x2)) ? EntityOrientation.Left : EntityOrientation.Right);
		Owner.SetOrientation(orientation, allowFlipRenderer: false);
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		SetAttackOrientation();
		base.CurrentEnemyWeapon.Attack(_weaponHit);
		AttacksMadeAmount++;
	}

	public void StaffBasicAttack()
	{
		CurrentWeaponAttack();
		PlaySlashFX();
		PlayImpactFX();
	}

	public void PlaySlashFX()
	{
		GameObject gameObject = PoolManager.Instance.ReuseObject(fxStaffSlash, angleCastCenter.position + angleCastCenter.up * -4.5f, angleCastCenter.rotation).GameObject;
	}

	public void PlayImpactFX()
	{
		GameObject gameObject = PoolManager.Instance.ReuseObject(fxStaffImpact, impactTransform.position, Quaternion.identity).GameObject;
		if (screenshakeOnEnd)
		{
			Core.Logic.CameraManager.ProCamera2DShake.Shake(shakeDuration, -angleCastCenter.up * shakeForce, vibrato, 0.01f, 0f, default(Vector3), 0.06f, ignoreTimeScale: true);
		}
	}

	public void DefaultArmAngle(float s = 0.9f)
	{
		base.transform.DOLocalRotate(Vector3.zero, s).SetEase(Ease.InOutCubic);
	}

	public void SetArmAngle()
	{
		Vector3 dir = Core.Logic.Penitent.transform.position - angleCastCenter.position;
		Debug.DrawRay(angleCastCenter.position, dir, Color.red, 2f);
		float num = 57.29578f * Mathf.Atan2(dir.y, dir.x);
		num += 90f;
		Debug.Log("UNCLAMPED: " + num);
		num = Mathf.Clamp(num, 0f - maxArmAngle, maxArmAngle);
		Debug.Log("ANGLE:" + num);
		base.transform.DOLocalRotate(new Vector3(0f, 0f, num), 0.4f).SetEase(Ease.InOutCubic);
	}

	public void QuickAttackMode(bool mode)
	{
		if (mode)
		{
			animator.speed = 2f;
		}
		else
		{
			animator.speed = 1f;
		}
	}

	public void SetCurrentFailedAttackLimit()
	{
		FailedAttackTier failedAttackTier = FailedAttackTiers.First();
		_currentFailedAttacksLimit = UnityEngine.Random.Range(failedAttackTier.MinFailedAttackAmount, failedAttackTier.MaxFailedAttackAmount);
	}

	public void ResetAttackCounters()
	{
		AttacksMadeAmount = 0;
		SucceedAttacksAmount = 0;
	}

	public void PlayLiftUpArm()
	{
		BejeweledSaintHead bejeweledSaintHead = (BejeweledSaintHead)Owner;
		if (bejeweledSaintHead != null)
		{
			bejeweledSaintHead.WholeBoss.Audio.PlayLiftUpMace();
		}
	}

	public void PlayAttack()
	{
		BejeweledSaintHead bejeweledSaintHead = (BejeweledSaintHead)Owner;
		if (bejeweledSaintHead != null)
		{
			bejeweledSaintHead.WholeBoss.Audio.PlaySmashMace();
		}
	}

	private void OnDestroy()
	{
		PenitentDamageArea.OnDamagedGlobal = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Remove(PenitentDamageArea.OnDamagedGlobal, new PenitentDamageArea.PlayerDamagedEvent(OnDamagedGlobal));
		BejeweledSaintStaff.OnSucceedHit = (Core.SimpleEvent)Delegate.Remove(BejeweledSaintStaff.OnSucceedHit, new Core.SimpleEvent(OnSucceedHit));
		_attackArea.OnEnter -= OnEnterAttackArea;
	}
}
