using System;
using System.Collections;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity.BlobShadow;
using Gameplay.GameControllers.Entities.Animations;
using I2.Loc;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Tools.Level.Layout;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

[SelectionBase]
public class Entity : MonoBehaviour
{
	public delegate void EntityFlagEvent(string key, bool active);

	public static Core.ObjectEvent OnCreated;

	private static readonly List<Entity> livingEntities = new List<Entity>();

	private readonly List<string> flags = new List<string>();

	private bool invulnerable;

	[TitleGroup("Entity Id", "A code which uniquely identifies an entity the documentation.", TitleAlignments.Left, true, true, false, 0)]
	public string Id;

	[TitleGroup("Entity Name", "The name of the entity in the documentation.", TitleAlignments.Left, true, true, false, 0)]
	public string EntityName;

	[SerializeField]
	public LocalizedString displayName;

	[SerializeField]
	[BoxGroup("Events", true, false, 0)]
	[ReadOnly]
	private string GenericEntityDead = "ENTITY_DEAD";

	[SerializeField]
	[BoxGroup("Events", true, false, 0)]
	private string OnDead;

	public EntityStats Stats = new EntityStats();

	public EntityStatus Status = new EntityStatus();

	public bool IsExecutable;

	public EntityOrientation OrientationBeforeHit;

	private float paralysisTimeCount;

	private Attack entityAttack;

	private DamageArea entityDamageArea;

	[SerializeField]
	protected SpriteRenderer spriteRenderer;

	[HideInInspector]
	public EntityStates entityCurrentState;

	public Animator Animator { get; private set; }

	public SpriteRenderer SpriteRenderer { get; private set; }

	public bool Dead => Stats.Life.Current <= 0f;

	public bool IsImpaled { get; set; }

	public bool Mute { get; set; }

	public static Entity LastSpawned { get; private set; }

	public static Entity[] LivingEntities => livingEntities.ToArray();

	public float CurrentLife
	{
		get
		{
			return Stats.Life.Current;
		}
		set
		{
			Stats.Life.Current = value;
		}
	}

	public float CurrentFervour
	{
		get
		{
			return Stats.Fervour.Current;
		}
		set
		{
			Stats.Fervour.Current = value;
		}
	}

	public float CurrentCriticalChance
	{
		get
		{
			return Stats.CriticalChance.Final;
		}
		set
		{
			Stats.CriticalChance.Bonus = value;
		}
	}

	public EntityAnimationEvents EntityAnimationEvents { get; set; }

	public BlobShadow Shadow { get; set; }

	public int CurrentOutputDamage { get; set; }

	public float SlopeAngle { get; set; }

	public Attack EntityAttack => entityAttack;

	public DamageArea EntityDamageArea => entityDamageArea;

	public event Core.SimpleEvent OnDamaged;

	public event Action<float> OnDamageTaken;

	public event Core.SimpleEvent OnDeath;

	public event Core.SimpleEvent OnDestroyEntity;

	public event Core.EntityEvent OnEntityDeath;

	public event EntityFlagEvent FlagChanged;

	public static event Core.EntityEvent Started;

	public static event Core.EntityEvent Death;

	public static event Core.EntityEvent Damaged;

	[Button(ButtonSizes.Small)]
	public void KillEntity()
	{
		Debug.Log("TRYING TO KILL ENTITY");
		Kill();
	}

	private void Awake()
	{
		Animator = GetComponentInChildren<Animator>();
		if (Animator != null)
		{
			SpriteRenderer = Animator.GetComponent<SpriteRenderer>();
		}
		entityAttack = GetComponentInChildren<Attack>();
		entityDamageArea = GetComponentInChildren<DamageArea>();
		EntityAnimationEvents = GetComponentInChildren<EntityAnimationEvents>();
		OnAwake();
		Stats.Initialize();
		livingEntities.Add(this);
		LastSpawned = this;
		if (OnCreated != null)
		{
			OnCreated(base.gameObject);
		}
	}

	protected virtual void OnDestroy()
	{
		livingEntities.Remove(this);
		if (this.OnDestroyEntity != null)
		{
			this.OnDestroyEntity();
		}
	}

	private void Start()
	{
		OnStart();
		if (Entity.Started != null)
		{
			Entity.Started(this);
		}
	}

	private void Update()
	{
		OnUpdate();
	}

	private void FixedUpdate()
	{
		OnFixedUpdated();
	}

	protected virtual void OnAwake()
	{
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void OnFixedUpdated()
	{
	}

	public virtual void HitDisplacement(Vector3 enemyPos, DamageArea.DamageType damageType)
	{
	}

	public virtual void SleepTimeByHit(Hit hit)
	{
		float hitSleepTime = Core.Logic.Penitent.LevelSleepTime.GetHitSleepTime(hit);
		LevelInitializer currentLevelConfig = Core.Logic.CurrentLevelConfig;
		currentLevelConfig.sleepTime = hitSleepTime;
		if (!currentLevelConfig.IsSleeping)
		{
			currentLevelConfig.SleepTime();
		}
	}

	public virtual bool BleedOnImpact()
	{
		return false;
	}

	public virtual bool SparkOnImpact()
	{
		return false;
	}

	public float GetReducedDamage(Hit hit)
	{
		float num = hit.DamageAmount;
		switch (hit.DamageElement)
		{
		case DamageArea.DamageElement.Fire:
			num -= Stats.FireDmgReduction.CalculateValue() * num;
			break;
		case DamageArea.DamageElement.Toxic:
			num -= Stats.ToxicDmgReduction.CalculateValue() * num;
			break;
		case DamageArea.DamageElement.Magic:
			num -= Stats.MagicDmgReduction.CalculateValue() * num;
			break;
		case DamageArea.DamageElement.Lightning:
			num -= Stats.LightningDmgReduction.CalculateValue() * num;
			break;
		case DamageArea.DamageElement.Contact:
			num -= Stats.ContactDmgReduction.CalculateValue() * num;
			break;
		case DamageArea.DamageElement.Normal:
			num -= Stats.NormalDmgReduction.CalculateValue() * num;
			break;
		}
		return num;
	}

	public void Damage(float amount, string impactAudioId = "")
	{
		if (!impactAudioId.IsNullOrWhitespace() && !Mute)
		{
			Core.Audio.EventOneShotPanned(impactAudioId, base.transform.position);
			Log.Trace("Audio", "Playing audio hit. " + impactAudioId);
		}
		float num = Mathf.Max(amount - Stats.Defense.Final, 0f);
		Stats.Life.Current -= num;
		if (this.OnDamaged != null)
		{
			this.OnDamaged();
		}
		if (this.OnDamageTaken != null)
		{
			this.OnDamageTaken(num);
		}
		if (Stats.Life.Current <= 0f)
		{
			KillInstanteneously();
		}
		if (Entity.Damaged != null)
		{
			Entity.Damaged(this);
		}
	}

	public void SetHealth(float health)
	{
		Stats.Life.Current = health;
		if (Stats.Life.Current <= 0f)
		{
			KillInstanteneously();
		}
	}

	public virtual void Revive()
	{
		SetHealth(Stats.Life.MaxValue);
		Status.Dead = false;
	}

	public virtual void Kill()
	{
		Damage(Stats.Life.Current + Stats.Defense.Final, string.Empty);
	}

	public void KillInstanteneously()
	{
		Status.Dead = true;
		Stats.Life.Current = 0f;
		if (Entity.Death != null)
		{
			Entity.Death(this);
		}
		if (this.OnDeath != null)
		{
			this.OnDeath();
		}
		if (this.OnEntityDeath != null)
		{
			this.OnEntityDeath(this);
		}
		Core.Events.LaunchEvent(GenericEntityDead, string.Empty);
		Core.Events.LaunchEvent(OnDead, string.Empty);
	}

	public virtual void SetOrientation(EntityOrientation orientation, bool allowFlipRenderer = true, bool searchForRenderer = false)
	{
		Status.Orientation = orientation;
		if (!allowFlipRenderer)
		{
			return;
		}
		if (!spriteRenderer && searchForRenderer)
		{
			spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		}
		if (!spriteRenderer)
		{
			Debug.LogWarning("No sprite renderer available. The entity cannot flip.");
			return;
		}
		switch (Status.Orientation)
		{
		case EntityOrientation.Left:
			spriteRenderer.flipX = true;
			break;
		case EntityOrientation.Right:
			spriteRenderer.flipX = false;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public IEnumerator FreezeAnimator(float freezeTime)
	{
		float deltaFreezeTime = 0f;
		Animator.speed = 0f;
		while (deltaFreezeTime <= freezeTime)
		{
			deltaFreezeTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		Animator.speed = 1f;
	}

	public EntityOrientation SetOrientationbyHit(Vector3 enemyPos)
	{
		float x = base.transform.position.x;
		EntityOrientation entityOrientation = Status.Orientation;
		OrientationBeforeHit = Status.Orientation;
		if (x > enemyPos.x)
		{
			entityOrientation = EntityOrientation.Left;
		}
		else if (x <= enemyPos.x)
		{
			entityOrientation = EntityOrientation.Right;
		}
		SetOrientation(entityOrientation);
		return entityOrientation;
	}

	public Vector3 GetForwardTangent(Vector3 dir, Vector3 up)
	{
		Vector3 lhs = Vector3.Cross(up, dir);
		return Vector3.Cross(lhs, up);
	}

	protected static bool IsVisibleFrom(Renderer entityRenderer, UnityEngine.Camera mainCamera)
	{
		return entityRenderer.isVisible;
	}

	public void SetFlag(string flag, bool active)
	{
		if (active && !HasFlag(flag))
		{
			flags.Add(flag);
			if (this.FlagChanged != null)
			{
				this.FlagChanged(flag, active: true);
			}
		}
		else if (!active && HasFlag(flag))
		{
			flags.Remove(flag);
			if (this.FlagChanged != null)
			{
				this.FlagChanged(flag, active: false);
			}
		}
	}

	public bool HasFlag(string key)
	{
		return flags.Contains(key);
	}
}
