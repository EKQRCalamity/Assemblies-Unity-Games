using System;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.AnimationBehaviours.Player.Attack;
using Gameplay.GameControllers.Effects.Player.Sparks;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using Gameplay.GameControllers.Penitent.Attack;
using Gameplay.GameControllers.Penitent.Movement;
using Sirenix.OdinInspector;
using UnityEngine;

public class TPOAnimatedFollower : MonoBehaviour
{
	private Animator animator;

	private SpriteRenderer spriteRenderer;

	private readonly int _upwardAttackAnimHash = Animator.StringToHash("GroundUpwardAttack");

	private readonly int _airupwardAttackAnimHash = Animator.StringToHash("AirUpwardAttack");

	private readonly int _jumpAnimHash = Animator.StringToHash("Air");

	private readonly int _runAnimHash = Animator.StringToHash("Run");

	private readonly int _attackAnimHash = Animator.StringToHash("GroundAttack");

	private readonly int _airAttackAnimHash = Animator.StringToHash("AirAttack");

	private readonly int _dashAnimHash = Animator.StringToHash("Dash");

	private readonly int _hurtAnimHash = Animator.StringToHash("Hurt");

	private readonly int _deathAnimHash = Animator.StringToHash("Death");

	private readonly int _victoryAnimHash = Animator.StringToHash("Victory");

	private const float HorSpeedThreshold = 0.5f;

	private const float AttackDuration = 0.15f;

	private float attackCoolDown;

	private const float DamageBlockDuration = 0.1f;

	private float damageCoolDown;

	private Penitent target;

	private TPOFollowerAudioEvents audioEvents;

	private SwordAnimatorInyector swordAnimator;

	public RuntimeAnimatorController demakeSlashAnimatorController;

	public GameObject deathSparkPrefab;

	[SerializeField]
	private AbilityDeactivator abilityDeactivator;

	[SerializeField]
	private BloodVFXTable bloodVFXTable;

	[SerializeField]
	private CharacterMotionProfile motionProfile;

	private bool TargetIsAirAttacking => (bool)target && target.Animator.GetCurrentAnimatorStateInfo(0).IsName("Air Attack 1");

	private void Awake()
	{
		animator = GetComponentInChildren<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		LevelManager.OnLevelLoaded += LevelManager_OnLevelLoaded;
		audioEvents = GetComponent<TPOFollowerAudioEvents>();
		PoolManager.Instance.CreatePool(deathSparkPrefab, 8);
		DemakeManager demakeManager = Core.DemakeManager;
		demakeManager.OnDemakeLevelCompletion = (Core.SimpleEvent)Delegate.Combine(demakeManager.OnDemakeLevelCompletion, new Core.SimpleEvent(OnDemakeVictory));
	}

	private void OnDemakeVictory()
	{
		SetVictory(victory: true);
	}

	private void LevelManager_OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		if (newLevel.LevelName.StartsWith("D25"))
		{
			ApplyDemakeCameraSettings();
			target = Core.Logic.Penitent;
			target.SpriteRenderer.enabled = false;
			DeactivateTPOAbilities();
			LoadMotionProfile();
			target.OnAttackBehaviourEnters += Target_OnAttackBehaviourEnters;
			target.OnAirAttackBehaviourEnters += Target_OnAirAttackBehaviourEnters;
			target.OnDeath += Target_OnDeath;
			target.OnDamaged += Target_OnDamaged;
			target.OnJump += Target_OnJump;
			swordAnimator = target.GetComponentInChildren<SwordAnimatorInyector>();
			LoadBloodVfx();
			if ((bool)swordAnimator)
			{
				swordAnimator.SwordAnimator.runtimeAnimatorController = demakeSlashAnimatorController;
			}
		}
		else
		{
			LevelManager.OnLevelLoaded -= LevelManager_OnLevelLoaded;
		}
	}

	private void Target_OnJump()
	{
		audioEvents.PlayJumpFX();
	}

	private void Target_OnDamaged()
	{
		damageCoolDown = 0.1f;
		attackCoolDown = 0f;
		PlayHurt();
	}

	private void Target_OnDeath()
	{
		PlayDeath();
	}

	private void DeactivateTPOAbilities()
	{
		if ((bool)abilityDeactivator)
		{
			abilityDeactivator.SetUp();
		}
	}

	private void LoadMotionProfile()
	{
		if ((bool)target)
		{
			motionProfile.Init(target.PlatformCharacterController);
		}
	}

	private void LoadBloodVfx()
	{
		if (!bloodVFXTable || !target)
		{
			return;
		}
		BloodSpawner componentInChildren = target.GetComponentInChildren<BloodSpawner>();
		if (!componentInChildren)
		{
			return;
		}
		componentInChildren.bloodVFXTable = bloodVFXTable;
		foreach (BloodFXTableElement bloodVFX in componentInChildren.bloodVFXTable.bloodVFXList)
		{
			PoolManager.Instance.CreatePool(bloodVFX.prefab, bloodVFX.poolSize);
		}
	}

	[Button("Test death sparks", ButtonSizes.Small)]
	private void PlayDeathSparks()
	{
		float num = 25f;
		float duration = 1.5f;
		Vector3 up = Vector3.up;
		for (int i = 0; i < 8; i++)
		{
			GameObject gameObject = PoolManager.Instance.ReuseObject(deathSparkPrefab, base.transform.position + up, Quaternion.identity).GameObject;
			Vector2 vector = Quaternion.Euler(0f, 0f, 45 * i) * Vector2.right;
			gameObject.transform.DOMove((Vector2)gameObject.transform.position + vector * num, duration).SetEase(Ease.InOutQuad);
		}
	}

	private void ApplyDemakeCameraSettings()
	{
		UnityEngine.Object.FindObjectOfType<ProCamera2DPixelPerfect>().PixelsPerUnit = 16f;
	}

	private void Target_OnAirAttackBehaviourEnters(AirAttackBehaviour obj)
	{
		PlayAirAttack();
	}

	private void Target_OnAttackBehaviourEnters(AttackBehaviour obj)
	{
		PlayAttack();
		attackCoolDown = 0.15f;
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		CheckAttackCooldown(deltaTime);
		CheckDamageCooldown(deltaTime);
	}

	private void LateUpdate()
	{
		if ((bool)target)
		{
			CopyTargetPosition();
			CopyTargetOrientation();
			CopyTargetGrounded();
			CopyTargetSpeed();
			CopyDash();
		}
	}

	private void CheckAttackCooldown(float dt)
	{
		if ((bool)target)
		{
			attackCoolDown -= dt;
			bool isAttacking = attackCoolDown > 0f;
			target.PenitentAttack.IsAttacking = isAttacking;
			target.PlatformCharacterInput.IsAttacking = isAttacking;
		}
	}

	private void CheckDamageCooldown(float dt)
	{
		if ((bool)target)
		{
			damageCoolDown -= dt;
			bool isHurt = damageCoolDown > 0f;
			target.Status.IsHurt = isHurt;
		}
	}

	private void CopyDash()
	{
		SetDashing(target.IsDashing);
	}

	private void CopyTargetSpeed()
	{
		float num = Math.Abs(target.PlatformCharacterController.PlatformCharacterPhysics.HSpeed);
		SetRun(num > 0.5f);
	}

	private void CopyTargetGrounded()
	{
		if (target.PlatformCharacterController.IsGrounded)
		{
			SetGrounded();
		}
		else
		{
			SetOnTheAir();
		}
	}

	private void CopyTargetOrientation()
	{
		base.transform.position = target.GetPosition();
	}

	private void CopyTargetPosition()
	{
		spriteRenderer.flipX = target.Status.Orientation != EntityOrientation.Right;
	}

	private void PlayAttack()
	{
		animator.SetTrigger(_attackAnimHash);
		audioEvents.PlayAttackFX();
	}

	private void PlayHurt()
	{
		animator.SetTrigger(_hurtAnimHash);
		audioEvents.PlayHurtFX();
	}

	private void PlayDeath()
	{
		animator.SetTrigger(_deathAnimHash);
		audioEvents.PlayDeathFX();
	}

	private void PlayAirAttack()
	{
		LandingAttack();
		animator.SetTrigger(_airAttackAnimHash);
		audioEvents.PlayAttackFX();
	}

	private void LandingAttack()
	{
		if ((bool)target)
		{
			bool flag = target.PlatformCharacterController.PlatformCharacterPhysics.VSpeed < 0f;
			bool flag2 = target.PlatformCharacterController.GroundDist < 2f;
			if (TargetIsAirAttacking && flag2 && flag)
			{
				StartCoroutine(AirAttackCoroutine());
			}
		}
	}

	private IEnumerator AirAttackCoroutine()
	{
		yield return new WaitForSeconds(0.1f);
		target.PenitentAttack.CurrentWeaponAttack(DamageArea.DamageType.Normal);
	}

	private void SetOnTheAir()
	{
		animator.SetBool(_jumpAnimHash, value: true);
	}

	private void SetVictory(bool victory)
	{
		animator.SetBool(_victoryAnimHash, victory);
	}

	private void SetDashing(bool dashing)
	{
		animator.SetBool(_dashAnimHash, dashing);
	}

	private void SetGrounded()
	{
		animator.SetBool(_jumpAnimHash, value: false);
	}

	private void SetRun(bool isRunning)
	{
		animator.SetBool(_runAnimHash, isRunning);
	}

	private void OnDestroy()
	{
		if ((bool)target)
		{
			target.OnAttackBehaviourEnters -= Target_OnAttackBehaviourEnters;
			target.OnAirAttackBehaviourEnters -= Target_OnAirAttackBehaviourEnters;
			target.OnDeath -= Target_OnDeath;
			target.OnJump -= Target_OnJump;
			target.OnDamaged -= Target_OnDamaged;
		}
		DemakeManager demakeManager = Core.DemakeManager;
		demakeManager.OnDemakeLevelCompletion = (Core.SimpleEvent)Delegate.Remove(demakeManager.OnDemakeLevelCompletion, new Core.SimpleEvent(OnDemakeVictory));
		LevelManager.OnLevelLoaded -= LevelManager_OnLevelLoaded;
	}
}
