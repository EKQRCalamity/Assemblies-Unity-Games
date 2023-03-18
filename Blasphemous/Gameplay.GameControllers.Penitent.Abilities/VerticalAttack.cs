using System;
using System.Collections.Generic;
using CreativeSpore.SmartColliders;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent.InputSystem;
using Rewired;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class VerticalAttack : Ability
{
	[Serializable]
	public struct AttackAreaDimensions
	{
		[SerializeField]
		public Vector2 AttackAreaOffset;

		[SerializeField]
		public Vector2 AttackAreaSize;

		public AttackAreaDimensions(Vector2 attackAreaSize, Vector2 attackAreaOffset)
		{
			AttackAreaOffset = attackAreaOffset;
			AttackAreaSize = attackAreaSize;
		}
	}

	public float DamageFactor = 2f;

	public GameObject HardLandingEffectPrefab;

	public GameObject HardLandingEffectUpgradedPrefab;

	public GameObject VerticalAttackBeamEffectPrefab;

	[BoxGroup("Stick Params", true, false, 0)]
	public float BeamAttackRequiredHeight = 5f;

	[Tooltip("Attack Area Dimensions By Skill Upgrade")]
	public AttackAreaDimensions[] AttackAreaDimension;

	[SerializeField]
	protected GhostTrailGenerator GhostTrailGenerator;

	private Penitent _penitent;

	private Player _rewired;

	private readonly Vector3 _defaultGravity = new Vector3(0f, -9.8f, 0f);

	[SerializeField]
	[BoxGroup("Trigger AAbility Params", true, false, 0)]
	[Range(0f, 0.5f)]
	private float AttackButtonHoldTime = 0.185f;

	private bool _ghostTrailTrigered;

	private bool _instantiateExplosion;

	private bool _canVerticalAttack;

	private float _initialHorPosition;

	private List<IDamageable> _damageableEntities;

	private Vector2 _defaultAttackAreaSize;

	private Vector2 _defaultAttackAreaOffset;

	public PlatformCharacterController CharacterController;

	public PlatformCharacterInput CharacterInput;

	[FoldoutGroup("Motion", 0)]
	[PropertyTooltip("The decrease factor of the vertical attack first phase")]
	public float DecelerateFactor = 0.5f;

	[FoldoutGroup("Motion", 0)]
	[PropertyTooltip("Max Distance to perform vertical attack")]
	public float RaycastDistance = 3f;

	[FoldoutGroup("Motion", 0)]
	[PropertyTooltip("Minimun distance to perform vertical attack")]
	public float DistanceThreshold = 2f;

	[Tooltip("Floor layers")]
	public LayerMask FloorLayer;

	[SerializeField]
	[FoldoutGroup("Audio Attack", 0)]
	[EventRef]
	public string VerticalLandingFxLevel1;

	[SerializeField]
	[FoldoutGroup("Audio Attack", 0)]
	[EventRef]
	public string VerticalLandingFxLevel2;

	[SerializeField]
	[FoldoutGroup("Audio Attack", 0)]
	[EventRef]
	public string VerticalLandingFxLevel3;

	[SerializeField]
	[FoldoutGroup("Audio Attack", 0)]
	[EventRef]
	public string EnemyImpactSound;

	public AttackArea VerticalAttackArea;

	private bool BeamRequiredHeightReached { get; set; }

	private Hit GetHardLandingHit
	{
		get
		{
			Hit result = default(Hit);
			result.AttackingEntity = base.EntityOwner.gameObject;
			result.DamageType = DamageArea.DamageType.Heavy;
			result.DamageAmount = base.EntityOwner.Stats.Strength.Final * DamageFactor;
			result.HitSoundId = EnemyImpactSound;
			return result;
		}
	}

	public string GetLandingFxEventKey
	{
		get
		{
			if (base.LastUnlockedSkillId.IsNullOrWhitespace())
			{
				return null;
			}
			string result = string.Empty;
			switch (base.LastUnlockedSkillId)
			{
			case "VERTICAL_1":
				result = VerticalLandingFxLevel1;
				break;
			case "VERTICAL_2":
				result = VerticalLandingFxLevel2;
				break;
			case "VERTICAL_3":
				result = VerticalLandingFxLevel3;
				break;
			}
			return result;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		_damageableEntities = new List<IDamageable>();
		_penitent = base.EntityOwner.GetComponent<Penitent>();
		_rewired = ReInput.players.GetPlayer(0);
		_defaultAttackAreaOffset = new Vector2(VerticalAttackArea.WeaponCollider.offset.x, VerticalAttackArea.WeaponCollider.offset.y);
		_defaultAttackAreaSize = new Vector2(VerticalAttackArea.WeaponCollider.bounds.size.x, VerticalAttackArea.WeaponCollider.bounds.size.y);
		PoolManager.Instance.CreatePool(HardLandingEffectPrefab, 1);
		PoolManager.Instance.CreatePool(VerticalAttackBeamEffectPrefab, 1);
		PoolManager.Instance.CreatePool(HardLandingEffectUpgradedPrefab, 1);
		VerticalAttackArea.OnEnter += OnEnterAttackArea;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		_canVerticalAttack = _penitent.PlatformCharacterController.GroundDist >= DistanceThreshold;
		float vSpeed = _penitent.PlatformCharacterController.PlatformCharacterPhysics.VSpeed;
		bool flag = _penitent.IsClimbingCliffLede || _penitent.IsGrabbingCliffLede;
		if (!_penitent.Status.IsGrounded && _canVerticalAttack && !flag && !_penitent.IsJumpingOff && vSpeed < 0.1f && _penitent.PlatformCharacterInput.isJoystickDown && _rewired.GetButtonTimedPress("Attack", AttackButtonHoldTime) && !base.Casting && base.HasEnoughFervour)
		{
			UnlockableSkill lastUnlockedSkill = GetLastUnlockedSkill();
			if (lastUnlockedSkill == null)
			{
				return;
			}
			base.LastUnlockedSkillId = lastUnlockedSkill.id;
			_ghostTrailTrigered = true;
			EnableVerticalAttackCollider();
			ShowGhostTrail();
			base.Animator.SetBool("VERTICAL_ATTACK", value: true);
			_initialHorPosition = _penitent.transform.position.x;
			BeamRequiredHeightReached = _penitent.PlatformCharacterController.GroundDist >= BeamAttackRequiredHeight;
			Cast();
		}
		if (base.Animator.GetCurrentAnimatorStateInfo(0).IsName("VerticalAttackStart"))
		{
			StopInTheAir();
		}
		else if (base.Animator.GetCurrentAnimatorStateInfo(0).IsName("VerticalAttackFalling"))
		{
			base.Animator.SetBool("VERTICAL_ATTACK", value: false);
			base.Animator.SetBool("ATTACKING", value: true);
			CharacterController.PlatformCharacterPhysics.Gravity = _defaultGravity * 5f;
		}
		else if (base.Animator.GetCurrentAnimatorStateInfo(0).IsName("VerticalAttackLanding"))
		{
			ShowGhostTrail(show: false);
			base.Animator.SetBool("ATTACKING", value: true);
			_penitent.PlatformCharacterInput.IsAttacking = true;
			if (!_instantiateExplosion)
			{
				_instantiateExplosion = true;
				Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("HardFall");
				_penitent.Rumble.UsePreset("VerticalAttack");
				_penitent.AnimatorInyector.ResetStuntByFall();
				TriggerVerticalAttack();
			}
		}
		else
		{
			if ((!_ghostTrailTrigered || !_penitent.Status.IsGrounded) && !base.Animator.GetCurrentAnimatorStateInfo(0).IsName("WallClimbContact"))
			{
				return;
			}
			_ghostTrailTrigered = false;
			ShowGhostTrail(show: false);
			if (base.Casting)
			{
				StopCast();
			}
		}
		if (base.Casting)
		{
			Vector3 position = _penitent.transform.position;
			_penitent.transform.position = new Vector3(_initialHorPosition, position.y, position.z);
		}
	}

	protected override void OnCastStart()
	{
		base.OnCastStart();
		_penitent.Status.Unattacable = true;
		_penitent.GrabLadder.EnableClimbLadderAbility(enable: false);
		_instantiateExplosion = false;
	}

	protected override void OnCastEnd(float castingTime)
	{
		base.OnCastEnd(castingTime);
		_penitent.Status.Unattacable = false;
		base.Animator.SetBool("ATTACKING", value: false);
		_penitent.PlatformCharacterInput.IsAttacking = false;
		_penitent.GrabLadder.EnableClimbLadderAbility();
		CharacterController.PlatformCharacterPhysics.Gravity = _defaultGravity;
	}

	private void ShowGhostTrail(bool show = true)
	{
		if (show)
		{
			if (!GhostTrailGenerator.EnableGhostTrail)
			{
				GhostTrailGenerator.EnableGhostTrail = true;
			}
		}
		else if (GhostTrailGenerator.EnableGhostTrail)
		{
			GhostTrailGenerator.EnableGhostTrail = false;
		}
	}

	private void StopInTheAir()
	{
		if (!(_penitent == null))
		{
			_penitent.PlatformCharacterController.PlatformCharacterPhysics.VSpeed = 0f;
			_penitent.PlatformCharacterController.PlatformCharacterPhysics.HSpeed = 0f;
			_penitent.PlatformCharacterController.PlatformCharacterPhysics.Acceleration = Vector3.zero;
		}
	}

	public void TriggerVerticalAttack(bool instantiateExplosion = true)
	{
		AttackDamageableEntities(GetHardLandingHit);
		EnableVerticalAttackCollider(enable: false);
		InstantiateExplosion(instantiateExplosion);
	}

	private void InstantiateExplosion(bool instantiate)
	{
		if (!instantiate || HardLandingEffectPrefab == null || VerticalAttackBeamEffectPrefab == null)
		{
			return;
		}
		UnlockableSkill lastUnlockedSkill = GetLastUnlockedSkill();
		if (!(lastUnlockedSkill == null))
		{
			bool flag = lastUnlockedSkill.id.Equals("VERTICAL_3");
			if (flag && BeamRequiredHeightReached)
			{
				PoolManager.Instance.ReuseObject(VerticalAttackBeamEffectPrefab, Core.Logic.Penitent.DamageArea.TopCenter, Quaternion.identity);
			}
			PoolManager.Instance.ReuseObject((!flag || !BeamRequiredHeightReached) ? HardLandingEffectPrefab : HardLandingEffectUpgradedPrefab, base.EntityOwner.transform.position, Quaternion.identity);
		}
	}

	private List<IDamageable> GetDamageableEntities()
	{
		GameObject[] array = VerticalAttackArea.OverlappedEntities();
		int num = array.Length;
		for (byte b = 0; b < num; b = (byte)(b + 1))
		{
			IDamageable componentInParent = array[b].GetComponentInParent<IDamageable>();
			_damageableEntities.Add(componentInParent);
		}
		return _damageableEntities;
	}

	private void AttackDamageableEntities(Hit hardLandingHit)
	{
		List<IDamageable> damageableEntities = GetDamageableEntities();
		int count = damageableEntities.Count;
		if (count > 0)
		{
			for (byte b = 0; b < count; b = (byte)(b + 1))
			{
				damageableEntities[b]?.Damage(hardLandingHit);
			}
			_damageableEntities.Clear();
		}
	}

	public void EnableVerticalAttackCollider(bool enable = true)
	{
		if (enable)
		{
			if (!VerticalAttackArea.WeaponCollider.enabled)
			{
				VerticalAttackArea.WeaponCollider.enabled = true;
			}
			if (!VerticalAttackArea.enabled)
			{
				VerticalAttackArea.enabled = true;
			}
		}
		else
		{
			if (VerticalAttackArea.WeaponCollider.enabled)
			{
				VerticalAttackArea.WeaponCollider.enabled = false;
			}
			if (VerticalAttackArea.enabled)
			{
				VerticalAttackArea.enabled = false;
			}
		}
	}

	public void SetAttackAreaDimensionsBySkill()
	{
		if (VerticalAttackArea == null)
		{
			return;
		}
		UnlockableSkill lastUnlockedSkill = GetLastUnlockedSkill();
		if (!(lastUnlockedSkill == null))
		{
			switch (lastUnlockedSkill.id)
			{
			case "VERTICAL_1":
				SetDefaultAttackAreaDimensions();
				break;
			case "VERTICAL_2":
				VerticalAttackArea.SetOffset(AttackAreaDimension[0].AttackAreaOffset);
				VerticalAttackArea.SetSize(AttackAreaDimension[0].AttackAreaSize);
				break;
			case "VERTICAL_3":
			{
				int num = (BeamRequiredHeightReached ? 1 : 0);
				VerticalAttackArea.SetOffset(AttackAreaDimension[num].AttackAreaOffset);
				VerticalAttackArea.SetSize(AttackAreaDimension[num].AttackAreaSize);
				break;
			}
			default:
				SetDefaultAttackAreaDimensions();
				break;
			}
		}
	}

	public void SetDefaultAttackAreaDimensions()
	{
		if (!(VerticalAttackArea == null))
		{
			VerticalAttackArea.SetOffset(_defaultAttackAreaOffset);
			VerticalAttackArea.SetSize(_defaultAttackAreaSize);
		}
	}

	private void OnEnterAttackArea(object sender, Collider2DParam e)
	{
		if (base.Casting && !base.EntityOwner.Status.IsGrounded && !base.Animator.GetCurrentAnimatorStateInfo(0).IsName("VerticalAttackStart"))
		{
			AttackDamageableEntities(GetHardLandingHit);
		}
	}

	private void OnDestroy()
	{
		VerticalAttackArea.OnEnter -= OnEnterAttackArea;
	}
}
