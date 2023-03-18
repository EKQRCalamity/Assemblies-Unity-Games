using System;
using System.Collections;
using CreativeSpore.SmartColliders;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.AnimationBehaviours.Player.Attack;
using Gameplay.GameControllers.Camera;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Effects.NPCs.BloodDecals;
using Gameplay.GameControllers.Effects.Player.Dash;
using Gameplay.GameControllers.Effects.Player.Dust;
using Gameplay.GameControllers.Effects.Player.Sparks;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using Gameplay.GameControllers.Penitent.Abilities;
using Gameplay.GameControllers.Penitent.Animator;
using Gameplay.GameControllers.Penitent.Attack;
using Gameplay.GameControllers.Penitent.Audio;
using Gameplay.GameControllers.Penitent.Damage;
using Gameplay.GameControllers.Penitent.InputSystem;
using Gameplay.GameControllers.Penitent.Movement;
using Gameplay.GameControllers.Penitent.Sensor;
using Gameplay.UI;
using Sirenix.OdinInspector;
using Tools.Level;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent;

public class Penitent : Entity, IDamageable
{
	public struct CollisionSkin
	{
		public Vector3 CenterCollision;

		public Vector2 CollisionSize;

		public CollisionSkin(Vector3 centerCollision, Vector2 collisionSize)
		{
			CenterCollision = centerCollision;
			CollisionSize = collisionSize;
		}
	}

	public Core.SimpleEvent OnDead;

	[BoxGroup("Sleep Time By Hit Type", true, false, 0)]
	[SerializeField]
	[Range(0f, 1f)]
	public float Normal;

	[BoxGroup("Sleep Time By Hit Type", true, false, 0)]
	[SerializeField]
	[Range(0f, 1f)]
	public float Heavy;

	[BoxGroup("Sleep Time By Hit Type", true, false, 0)]
	[SerializeField]
	[Range(0f, 1f)]
	public float Critical;

	public bool obtainsFervour = true;

	public bool GuiltDrop = true;

	public bool AllowEquipSwords = true;

	[SerializeField]
	protected GameObject Cherubs;

	public bool cliffLedeClimbingStarted;

	public bool canClimbCliffLede;

	public Vector2 jumpOffRoot;

	public bool isJumpOffReady;

	public bool startedJumpOff;

	private float fadeInTime;

	private bool fadeIn;

	public PIDI_2DReflection reflections;

	public LevelSleepTime LevelSleepTime { get; private set; }

	public bool IsJumping { get; set; }

	public bool JumpFromLadder { get; set; }

	public Dash Dash { get; private set; }

	public DashDustGenerator DashDustGenerator { get; set; }

	public PlatformCharacterInput PlatformCharacterInput { get; private set; }

	public PenitentAudio Audio { get; private set; }

	public bool IsGrabbingCliffLede { get; set; }

	public bool IsClimbingCliffLede { get; set; }

	public EntityOrientation CliffLedeOrientation { get; set; }

	public bool IsClimbingLadder { get; set; }

	public bool IsOnLadder { get; set; }

	public bool StepOnLadder { get; set; }

	public bool IsStickedOnWall { get; set; }

	public bool IsGrabbingLadder { get; set; }

	public bool StartingGoingDownLadders { get; set; }

	public bool CanJumpFromLadder { get; set; }

	public bool IsJumpingOff { get; set; }

	public bool BeginCrouch { get; set; }

	public bool IsCrouchAttacking { get; set; }

	public bool IsCrouched { get; set; }

	public bool WatchBelow { get; set; }

	public bool IsDeadInAir { get; set; }

	public bool DeathEventLaunched { get; set; }

	public bool IsFallingStunt { get; set; }

	public bool IsSmashed { get; set; }

	public EntityOrientation HurtOrientation { get; set; }

	public EntityRumble Rumble { get; private set; }

	public Vector3 RootTargetPosition { get; set; }

	public Vector3 RootMotionDrive { get; set; }

	public PenitentCancelEffect CancelEffect { get; private set; }

	public ParticleSystem ParticleSystem { get; private set; }

	public bool IsDashing { get; set; }

	public bool IsPickingCollectibleItem { get; set; }

	public MotionLerper MotionLerper { get; set; }

	public bool IsChargingAttack { get; set; }

	public bool IsAttackCharged { get; set; }

	public bool IsOnExecution { get; set; }

	public bool ReleaseChargedAttack { get; set; }

	public Parry Parry { get; private set; }

	public ChargedAttack ChargedAttack { get; private set; }

	public VerticalAttack VerticalAttack { get; private set; }

	public LungeAttack LungeAttack { get; private set; }

	public PenitentAttack PenitentAttack { get; private set; }

	public RangeAttack RangeAttack { get; private set; }

	public GuardSlide GuardSlide { get; private set; }

	public ActiveRiposte ActiveRiposte { get; private set; }

	public StateMachine StateMachine { get; private set; }

	public DrivePlayer DrivePlayer { get; private set; }

	public GrabCliffLede GrabCliffLede { get; private set; }

	public bool CanClimbLadder { get; set; }

	public bool CanLowerCliff { get; set; }

	public bool IsLadderSliding { get; set; }

	public GrabLadder GrabLadder { get; private set; }

	public bool ReachTopLadder { get; set; }

	public bool ReachBottonLadder { get; set; }

	public PenitentDamageArea DamageArea { get; private set; }

	public BloodDecalPumper BloodDecalPumper { get; private set; }

	public ThrowBack ThrowBack { get; private set; }

	public ColorFlash ColorFlash { get; private set; }

	public CameraManager CameraManager { get; private set; }

	public AttackArea AttackArea { get; private set; }

	public PenitentAttackAnimations PenitentAttackAnimations { get; private set; }

	public SwordSparkSpawner SwordSparkSpawner { get; private set; }

	public StepDustSpawner StepDustSpawner { get; private set; }

	public AnimatorInyector AnimatorInyector { get; private set; }

	public PenitentMoveAnimations PenitentMoveAnimations { get; private set; }

	public PhysicsSwitcher Physics { get; private set; }

	public Rigidbody2D RigidBody { get; private set; }

	public FloorDistanceChecker FloorChecker { get; private set; }

	public CheckTrap TrapChecker { get; private set; }

	public MotionLerper PlayerHitMotionLerper { get; private set; }

	public PlatformCharacterController PlatformCharacterController { get; private set; }

	public Healing Healing { get; private set; }

	public PrayerUse PrayerCast { get; private set; }

	public FervourPenance Penance { get; private set; }

	public event Action<AttackBehaviour> OnAttackBehaviourEnters;

	public event Action OnJump;

	public event Action<AirAttackBehaviour> OnAirAttackBehaviourEnters;

	public Penitent()
	{
		IsDeadInAir = false;
	}

	public void OnAttackBehaviour_OnEnter(AttackBehaviour attackBehaviour)
	{
		if (this.OnAttackBehaviourEnters != null)
		{
			this.OnAttackBehaviourEnters(attackBehaviour);
		}
	}

	public void OnAirAttackBehaviour_OnEnter(AirAttackBehaviour airAttackBehaviour)
	{
		if (this.OnAirAttackBehaviourEnters != null)
		{
			this.OnAirAttackBehaviourEnters(airAttackBehaviour);
		}
	}

	public void IncrementFervour(Hit hit)
	{
		float num = Stats.FervourStrength.Final;
		switch (hit.DamageType)
		{
		case Gameplay.GameControllers.Entities.DamageArea.DamageType.Heavy:
			num *= 2f;
			break;
		case Gameplay.GameControllers.Entities.DamageArea.DamageType.Critical:
			num *= 5f;
			break;
		}
		num *= Core.GuiltManager.GetFervourGainFactor();
		Stats.Fervour.Current += num;
	}

	private void OnEntityDead(Entity entity)
	{
		Enemy enemy = entity as Enemy;
		if ((bool)enemy)
		{
			GetPurge(enemy);
		}
		Penitent penitent = entity as Penitent;
		if (!(penitent == null))
		{
			Core.Events.SetFlag("CHERUB_RESPAWN", b: true);
			EnableAbilities(enableAbility: false);
			EnableTraits(enableTraits: false);
			DamageArea.IncludeEnemyLayer(include: false);
		}
	}

	private void GetPurge(Enemy enemy)
	{
		float num = Stats.PurgeStrength.Final * enemy.purgePointsWhenDead;
		num *= Core.GuiltManager.GetPurgeGainFactor();
		if (IsOnExecution)
		{
			num += num * 0.5f;
		}
		Stats.Purge.Current += num;
	}

	public void Respawn()
	{
		Core.Logic.EnemySpawner.Reset();
		Core.Persistence.RestoreStored();
		Core.SpawnManager.Respawn();
	}

	public void CherubRespawn()
	{
		if (Cherubs != null)
		{
			PoolManager.Instance.ReuseObject(Cherubs, base.transform.position, Quaternion.identity);
		}
		UIController.instance.UpdateGuiltLevel(whenDead: true);
	}

	public void Teleport(Vector2 position)
	{
		Penitent penitent = Core.Logic.Penitent;
		penitent.PlatformCharacterController.PlatformCharacterPhysics.Acceleration = Vector3.zero;
		penitent.PlatformCharacterController.PlatformCharacterPhysics.Velocity = Vector3.zero;
		penitent.Physics.EnableColliders(enable: false);
		penitent.Physics.Enable2DCollision(enable: false);
		base.transform.position = position;
		penitent.Physics.EnableColliders();
		penitent.Physics.Enable2DCollision();
	}

	public void ForceMove(Vector2 position)
	{
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		StartCoroutine(ForceMovementAction(position));
		Core.Logic.SetPreviousState();
	}

	private IEnumerator ForceMovementAction(Vector2 targetPosition)
	{
		do
		{
			float direction = targetPosition.x - base.transform.position.x;
			if (Mathf.Sign(direction) < 0f)
			{
				break;
			}
			Core.Logic.Penitent.PlatformCharacterInput.forceHorizontalMovement = direction;
			yield return new WaitForFixedUpdate();
		}
		while (base.transform.position.x < targetPosition.x + 0.2f);
		do
		{
			float direction = targetPosition.x - base.transform.position.x;
			if (Mathf.Sign(direction) > 0f)
			{
				break;
			}
			Core.Logic.Penitent.PlatformCharacterInput.forceHorizontalMovement = direction;
			yield return new WaitForFixedUpdate();
		}
		while (base.transform.position.x > targetPosition.x - 0.2f);
		yield return new WaitForSeconds(0.5f);
		Core.Logic.Penitent.PlatformCharacterInput.forceHorizontalMovement = 0f;
	}

	private void EnableAbilities(bool enableAbility)
	{
		if (this == null)
		{
			return;
		}
		Ability[] componentsInChildren = GetComponentsInChildren<Ability>();
		foreach (Ability ability in componentsInChildren)
		{
			if (ability.GetType() != typeof(ThrowBack))
			{
				ability.enabled = enableAbility;
			}
		}
	}

	private void EnableTraits(bool enableTraits)
	{
		if (!(this == null))
		{
			Trait[] componentsInChildren = GetComponentsInChildren<Trait>();
			foreach (Trait trait in componentsInChildren)
			{
				trait.enabled = enableTraits;
			}
		}
	}

	public override void SetOrientation(EntityOrientation orientation, bool allowFlipRenderer = true, bool searchForRenderer = false)
	{
		base.SetOrientation(orientation, allowFlipRenderer, searchForRenderer);
		PlatformCharacterInput.faceRight = orientation == EntityOrientation.Right;
	}

	public EntityOrientation GetOrientation()
	{
		return (!PlatformCharacterInput.faceRight) ? EntityOrientation.Left : EntityOrientation.Right;
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		AnimatorInyector = GetComponent<AnimatorInyector>();
		PlayerHitMotionLerper = GetComponent<MotionLerper>();
		PlatformCharacterInput = GetComponent<PlatformCharacterInput>();
		PlatformCharacterController = GetComponent<PlatformCharacterController>();
		Physics = GetComponent<PhysicsSwitcher>();
		RigidBody = GetComponent<Rigidbody2D>();
		StateMachine = GetComponent<StateMachine>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		TrapChecker = GetComponentInChildren<CheckTrap>();
		PenitentMoveAnimations = GetComponentInChildren<PenitentMoveAnimations>();
		PenitentAttackAnimations = GetComponentInChildren<PenitentAttackAnimations>();
		StepDustSpawner = GetComponentInChildren<StepDustSpawner>();
		AttackArea = GetComponentInChildren<AttackArea>();
		DamageArea = GetComponentInChildren<PenitentDamageArea>();
		PenitentAttack = GetComponentInChildren<PenitentAttack>();
		VerticalAttack = GetComponentInChildren<VerticalAttack>();
		RangeAttack = GetComponentInChildren<RangeAttack>();
		LungeAttack = GetComponentInChildren<LungeAttack>();
		SwordSparkSpawner = GetComponentInChildren<SwordSparkSpawner>();
		GrabCliffLede = GetComponentInChildren<GrabCliffLede>();
		GrabLadder = GetComponentInChildren<GrabLadder>();
		Audio = GetComponentInChildren<PenitentAudio>();
		ChargedAttack = GetComponentInChildren<ChargedAttack>();
		Dash = GetComponentInChildren<Dash>();
		FloorChecker = GetComponentInChildren<FloorDistanceChecker>();
		BloodDecalPumper = GetComponentInChildren<BloodDecalPumper>();
		MotionLerper = GetComponentInChildren<MotionLerper>();
		DashDustGenerator = GetComponentInChildren<DashDustGenerator>();
		Healing = GetComponentInChildren<Healing>();
		PrayerCast = GetComponentInChildren<PrayerUse>();
		Penance = GetComponentInChildren<FervourPenance>();
		Parry = GetComponentInChildren<Parry>();
		Rumble = GetComponentInChildren<EntityRumble>();
		GuardSlide = GetComponentInChildren<GuardSlide>();
		ParticleSystem = GetComponentInChildren<ParticleSystem>();
		DrivePlayer = GetComponentInChildren<DrivePlayer>();
		ActiveRiposte = GetComponentInChildren<ActiveRiposte>();
		ThrowBack = GetComponentInChildren<ThrowBack>();
		CancelEffect = GetComponentInChildren<PenitentCancelEffect>();
		base.Animator.Play("Idle");
		LogicManager.GoToMainMenu = (Core.SimpleEvent)Delegate.Combine(LogicManager.GoToMainMenu, new Core.SimpleEvent(GoToMainMenu));
		if ((bool)PoolManager.Instance)
		{
			PoolManager.Instance.CreatePool(Cherubs.gameObject, 1);
		}
		Entity.Death += OnEntityDead;
	}

	internal void OnJumpTrigger(AnimatorInyector animatorInyector)
	{
		if (this.OnJump != null)
		{
			this.OnJump();
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		LevelSleepTime = new LevelSleepTime(Normal, Heavy, Critical);
		CameraManager = Core.Logic.CameraManager;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		Status.IsVisibleOnCamera = IsVisible();
		if (!Status.Dead)
		{
			return;
		}
		if (Core.Logic.CurrentState != LogicStates.PlayerDead)
		{
			Core.Logic.SetState(LogicStates.PlayerDead);
		}
		if (!DeathEventLaunched)
		{
			DeathEventLaunched = true;
			Core.Logic.PlayerCurrentLife = -1f;
			if (OnDead != null)
			{
				OnDead();
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Entity.Death -= OnEntityDead;
		LogicManager.GoToMainMenu = (Core.SimpleEvent)Delegate.Remove(LogicManager.GoToMainMenu, new Core.SimpleEvent(GoToMainMenu));
	}

	private void GoToMainMenu()
	{
		PlatformCharacterInput.ResetInputs();
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
	}

	public void ShowTutorial(string tutorialId)
	{
		if (!Core.TutorialManager.IsTutorialUnlocked(tutorialId))
		{
			StartCoroutine(Core.TutorialManager.ShowTutorial(tutorialId));
		}
	}

	public void ShowTutorial(string id, float delay)
	{
		StartCoroutine(ShowTutorialDelayedCoroutine(delay, id));
	}

	private IEnumerator ShowTutorialDelayedCoroutine(float delay, string id)
	{
		yield return new WaitForSeconds(delay);
		ShowTutorial(id);
	}

	public bool IsVisible()
	{
		return Entity.IsVisibleFrom(base.SpriteRenderer, UnityEngine.Camera.main);
	}

	public void SetInVisible(bool invisible = true)
	{
		Color white = Color.white;
		white.a = ((!invisible) ? 0f : 1f);
		base.SpriteRenderer.color = white;
	}

	public void Damage(Hit hit)
	{
		if (!DamageArea || (LungeAttack.Casting && hit.DamageElement == Gameplay.GameControllers.Entities.DamageArea.DamageElement.Contact) || IsOnExecution || IsPickingCollectibleItem || GuardSlide.Casting)
		{
			return;
		}
		if (Parry.IsOnParryChance && hit.DamageType == Gameplay.GameControllers.Entities.DamageArea.DamageType.Normal && !hit.forceGuardslide)
		{
			Parry.IsOnParryChance = false;
			if (!Parry.CheckParry(hit))
			{
				DamageArea.TakeDamage(hit);
			}
		}
		else if (Parry.IsOnParryChance && !hit.Unblockable && (hit.DamageType == Gameplay.GameControllers.Entities.DamageArea.DamageType.Heavy || hit.forceGuardslide))
		{
			PenitentSword penitentSword = (PenitentSword)Core.Logic.Penitent.PenitentAttack.CurrentPenitentWeapon;
			Enemy enemy = ((!(hit.AttackingEntity != null)) ? null : hit.AttackingEntity.GetComponent<Enemy>());
			if (!hit.CheckOrientationsForGuardslide || penitentSword.IsEnemySameDirection(enemy))
			{
				GuardSlide.CastSlide(hit);
			}
			else
			{
				DamageArea.TakeDamage(hit);
			}
		}
		else
		{
			DamageArea.TakeDamage(hit);
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public Vector2 GetVelocity()
	{
		return PlatformCharacterController.InstantVelocity;
	}
}
