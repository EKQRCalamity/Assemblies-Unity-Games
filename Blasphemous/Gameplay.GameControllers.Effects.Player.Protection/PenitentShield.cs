using FMODUnity;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Gameplay.GameControllers.Penitent;
using Plugins.GhostSprites2D.Scripts.GhostSprites;
using Sirenix.OdinInspector;

namespace Gameplay.GameControllers.Effects.Player.Protection;

public class PenitentShield : Weapon
{
	private Hit _shieldHit;

	[FoldoutGroup("Hit Settings", 0)]
	public float Strength;

	[FoldoutGroup("Hit Settings", 0)]
	public DamageArea.DamageElement DamageElement;

	[FoldoutGroup("Hit Settings", 0)]
	public DamageArea.DamageType DamageType;

	[FoldoutGroup("Audio", 0)]
	[EventRef]
	public string HitSoundFx;

	private MasterShaderEffects shieldEffects;

	public AttackArea AttackArea { get; private set; }

	public GhostSprites GhostSprites { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		AttackArea = GetComponent<AttackArea>();
		GhostSprites = GetComponent<GhostSprites>();
		shieldEffects = GetComponent<MasterShaderEffects>();
		_shieldHit = new Hit
		{
			AttackingEntity = base.gameObject,
			DamageElement = DamageElement,
			DamageType = DamageType,
			HitSoundId = HitSoundFx,
			DestroysProjectiles = true
		};
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea.OnEnter += OnEnterAttackArea;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ((bool)shieldEffects)
		{
			shieldEffects.ColorizeWave(1.5f);
		}
	}

	private void OnEnterAttackArea(object sender, Collider2DParam e)
	{
		if ((bool)shieldEffects)
		{
			shieldEffects.TriggerColorFlash();
		}
		Attack(_shieldHit);
	}

	public void SmallDistortion()
	{
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 0.2f, 0.1f, 0.5f);
	}

	private void OnEnable()
	{
		GhostSprites.EnableGhostTrail = true;
	}

	private void OnDisable()
	{
		GhostSprites.EnableGhostTrail = false;
	}

	private void OnDestroy()
	{
		AttackArea.OnEnter -= OnEnterAttackArea;
	}

	public override void Attack(Hit weapondHit)
	{
		weapondHit.DamageAmount = Strength * Core.Logic.Penitent.Stats.PrayerStrengthMultiplier.Final;
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}
}
