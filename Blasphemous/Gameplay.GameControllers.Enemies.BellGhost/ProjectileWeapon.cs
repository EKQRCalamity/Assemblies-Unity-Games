using System;
using System.Collections.Generic;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Damage;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.GameControllers.Enemies.BellGhost;

public class ProjectileWeapon : Weapon, IDamageable
{
	private float _cdCounter;

	private bool _destroyed;

	private readonly float _hitCooldown = 0.1f;

	private int _hitsRemaining;

	private float _hitStrength = 1f;

	private bool poolsCreated;

	[FoldoutGroup("References", 0)]
	public GameObject AttackingEntity;

	[FoldoutGroup("Design settings", 0)]
	public int damage = 10;

	[FoldoutGroup("Design settings", 0)]
	public DamageArea.DamageElement damageElement;

	[FoldoutGroup("Design settings", 0)]
	public DamageArea.DamageType damageType;

	[FoldoutGroup("Design settings", 0)]
	public bool destroyOnHit = true;

	[FoldoutGroup("Design settings", 0)]
	public bool unparryable;

	[FoldoutGroup("Design settings", 0)]
	public bool forceGuardslide;

	[FoldoutGroup("Design settings", 0)]
	public float force;

	[FoldoutGroup("Design settings", 0)]
	public ProjectileReaction ProjectileReaction;

	[FoldoutGroup("Design settings", 0)]
	public GameObject explosion;

	[FoldoutGroup("Design settings", 0)]
	public bool hasAdditionalExplosions;

	[FoldoutGroup("Design settings", 0)]
	[ShowIf("hasAdditionalExplosions", true)]
	public List<GameObject> additionalExplosions = new List<GameObject>();

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string explosionSound;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string flightSound;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string hitSound;

	[FoldoutGroup("Design settings", 0)]
	public bool multiHit;

	[FoldoutGroup("Design settings", 0)]
	[ShowIf("destroyOnHit", true)]
	public int numberOfHits = 1;

	[FoldoutGroup("Design settings", 0)]
	[ShowIf("destroyOnHit", true)]
	public LayerMask pierceLayers;

	[FoldoutGroup("Design settings", 0)]
	public GameObject onHitEffect;

	[FoldoutGroup("Design settings", 0)]
	public GameObject onDamageEffect;

	[FoldoutGroup("References", 0)]
	public Projectile projectile;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string shootSound;

	[FoldoutGroup("Design settings", 0)]
	public bool unavoidable = true;

	[FoldoutGroup("Design settings", 0)]
	public bool unblockable;

	private Hit weaponHit;

	public UnityEvent OnSpawn;

	public UnityEvent OnDeath;

	private const string PROJECTILE_BARRIER_LAYER = "ProjectileBarrier";

	private EventInstance _ghostBulletFlightAudioInstance;

	private const string LabelPanning = "Panning";

	public SpriteRenderer SpriteRenderer { get; private set; }

	public AttackArea AttackArea { get; private set; }

	public event Action<ProjectileWeapon> OnProjectileDeath;

	public event Action<ProjectileWeapon> OnProjectileHitsSomething;

	protected override void OnAwake()
	{
		base.OnAwake();
		AttackArea = GetComponentInChildren<AttackArea>();
		SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
		ProjectileReaction = GetComponentInChildren<ProjectileReaction>();
		AttackArea attackArea = AttackArea;
		attackArea.enemyLayerMask = (int)attackArea.enemyLayerMask | (1 << LayerMask.NameToLayer("ProjectileBarrier"));
		if (explosion != null)
		{
			PoolManager.Instance.CreatePool(explosion, 1);
		}
		if (hasAdditionalExplosions)
		{
			additionalExplosions.ForEach(delegate(GameObject x)
			{
				PoolManager.Instance.CreatePool(x, 1);
			});
		}
		if (onHitEffect != null)
		{
			PoolManager.Instance.CreatePool(onHitEffect, numberOfHits);
		}
		if (onDamageEffect != null)
		{
			PoolManager.Instance.CreatePool(onDamageEffect, 1);
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea.OnEnter += AttackAreaOnEnter;
		if (multiHit)
		{
			AttackArea.OnStay += AttackArea_OnStay;
		}
		if ((bool)ProjectileReaction)
		{
			ProjectileReaction.OnProjectileHit += OnProjectileHit;
		}
	}

	private bool IsInLayermask(int layer, LayerMask layermask)
	{
		return (int)layermask == ((int)layermask | (1 << layer));
	}

	public void ForceDestroy()
	{
		BulletDestruction();
	}

	private void OnProjectileHit(ProjectileReaction obj)
	{
		if ((!obj.IsPhysical && obj.DestructorHit) || obj.DestroyedByNormalHits)
		{
			BulletDestruction();
		}
	}

	private void OnLifeEnded(Projectile obj)
	{
		BulletDestruction();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (SpriteRenderer.isVisible)
		{
			PlayFlightFx();
		}
		else
		{
			DisposeFxFlightEvent();
		}
		if (_cdCounter > 0f)
		{
			_cdCounter -= Time.deltaTime;
		}
		UpdateEvent(ref _ghostBulletFlightAudioInstance);
	}

	private void CreateOnHitEffect()
	{
		if (onHitEffect != null)
		{
			PoolManager.Instance.ReuseObject(onHitEffect, base.transform.position, base.transform.rotation);
		}
	}

	private void CreateOnDamageEffect()
	{
		if (onDamageEffect != null)
		{
			PoolManager.Instance.ReuseObject(onDamageEffect, base.transform.position, base.transform.rotation);
		}
	}

	private void AttackArea_OnStay(object sender, Collider2DParam e)
	{
		if (_cdCounter > 0f)
		{
			return;
		}
		GameObject gameObject = e.Collider2DArg.gameObject;
		if (gameObject.layer != LayerMask.NameToLayer("Enemy"))
		{
			return;
		}
		_cdCounter = _hitCooldown;
		Attack(weaponHit);
		CreateOnHitEffect();
		if (!IsInLayermask(gameObject.layer, pierceLayers))
		{
			_hitsRemaining--;
			if (destroyOnHit && _hitsRemaining == 0)
			{
				BulletDestruction();
			}
		}
	}

	private void AttackAreaOnEnter(object sender, Collider2DParam collider2DParam)
	{
		if (_destroyed)
		{
			return;
		}
		GameObject gameObject = collider2DParam.Collider2DArg.gameObject;
		Attack(weaponHit);
		_cdCounter = _hitCooldown;
		if (!IsInLayermask(gameObject.layer, pierceLayers))
		{
			_hitsRemaining--;
			if (destroyOnHit && (!multiHit || _hitsRemaining == 0))
			{
				BulletDestruction();
			}
		}
		if (this.OnProjectileHitsSomething != null)
		{
			this.OnProjectileHitsSomething(this);
		}
		CreateOnHitEffect();
	}

	private void CreateHit()
	{
		if (!AttackingEntity)
		{
			AttackingEntity = base.gameObject;
		}
		weaponHit = new Hit
		{
			AttackingEntity = AttackingEntity.gameObject,
			DamageType = damageType,
			DamageAmount = (float)damage * _hitStrength,
			DamageElement = damageElement,
			Unblockable = unblockable,
			Unnavoidable = unavoidable,
			Unparriable = unparryable,
			HitSoundId = hitSound,
			forceGuardslide = forceGuardslide,
			Force = force
		};
	}

	public void SetDamage(int dmg)
	{
		damage = dmg;
		CreateHit();
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	private void BulletDestruction()
	{
		if (this.OnProjectileDeath != null)
		{
			this.OnProjectileDeath(this);
		}
		if (OnDeath != null)
		{
			OnDeath.Invoke();
		}
		_destroyed = true;
		projectile.OnLifeEndedEvent -= OnLifeEnded;
		if ((bool)explosion)
		{
			PoolManager.Instance.ReuseObject(explosion, base.transform.position, Quaternion.identity);
			PlayExplosionSound();
			if (hasAdditionalExplosions)
			{
				additionalExplosions.ForEach(delegate(GameObject x)
				{
					PoolManager.Instance.ReuseObject(x, base.transform.position, Quaternion.identity);
				});
			}
		}
		PenitentDamageArea.OnDamagedGlobal = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Remove(PenitentDamageArea.OnDamagedGlobal, new PenitentDamageArea.PlayerDamagedEvent(OnDamagedGlobal));
		StopFlightFx();
		Destroy();
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		if (!projectile)
		{
			projectile = GetComponent<Projectile>();
		}
		DisposeFxFlightEvent();
		_hitsRemaining = numberOfHits;
		_destroyed = false;
		projectile.OnLifeEndedEvent += OnLifeEnded;
		projectile.ResetTTL();
		CreateHit();
		if (SpriteRenderer.IsVisibleFrom(UnityEngine.Camera.main) && shootSound != string.Empty)
		{
			Core.Audio.PlayOneShot(shootSound);
		}
		if (SpriteRenderer.IsVisibleFrom(UnityEngine.Camera.main) && flightSound != string.Empty)
		{
			PlayFlightFx();
		}
		PenitentDamageArea.OnDamagedGlobal = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Combine(PenitentDamageArea.OnDamagedGlobal, new PenitentDamageArea.PlayerDamagedEvent(OnDamagedGlobal));
		if (OnSpawn != null)
		{
			OnSpawn.Invoke();
		}
	}

	private void OnDamagedGlobal(Gameplay.GameControllers.Penitent.Penitent damaged, Hit hit)
	{
		if (weaponHit.Equals(hit))
		{
			CreateOnDamageEffect();
		}
	}

	public void Damage(Hit hit)
	{
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public void SetDamageStrength(float strength)
	{
		_hitStrength = strength;
		CreateHit();
	}

	private void OnDestroy()
	{
		DisposeFxFlightEvent();
		if (multiHit)
		{
			AttackArea.OnStay -= AttackArea_OnStay;
		}
		if ((bool)ProjectileReaction)
		{
			ProjectileReaction.OnProjectileHit -= OnProjectileHit;
		}
		PenitentDamageArea.OnDamagedGlobal = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Remove(PenitentDamageArea.OnDamagedGlobal, new PenitentDamageArea.PlayerDamagedEvent(OnDamagedGlobal));
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	public void SetOwner(GameObject enemy)
	{
		AttackingEntity = enemy;
	}

	private void PlayFlightFx()
	{
		if (SpriteRenderer.isVisible && !_ghostBulletFlightAudioInstance.isValid() && !(flightSound == string.Empty))
		{
			_ghostBulletFlightAudioInstance = Core.Audio.CreateEvent(flightSound);
			_ghostBulletFlightAudioInstance.start();
		}
	}

	private void StopFlightFx()
	{
		if (_ghostBulletFlightAudioInstance.isValid())
		{
			if (_ghostBulletFlightAudioInstance.getParameter("End", out var instance) == RESULT.OK)
			{
				instance.setValue(1f);
			}
			else
			{
				DisposeFxFlightEvent();
			}
		}
	}

	private void UpdateEvent(ref EventInstance eventInstance)
	{
		if (SpriteRenderer.isVisible && eventInstance.isValid())
		{
			SetPanning(eventInstance);
		}
	}

	private void DisposeFxFlightEvent()
	{
		if (_ghostBulletFlightAudioInstance.isValid())
		{
			_ghostBulletFlightAudioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_ghostBulletFlightAudioInstance.release();
			_ghostBulletFlightAudioInstance = default(EventInstance);
		}
	}

	private void PlayExplosionSound()
	{
		if (!string.IsNullOrEmpty(explosionSound) && SpriteRenderer.IsVisibleFrom(UnityEngine.Camera.main))
		{
			Core.Audio.EventOneShotPanned(explosionSound, base.transform.position);
		}
	}

	private EVENT_CALLBACK SetPanning(EventInstance e)
	{
		e.getParameter("Panning", out var instance);
		if (instance.isValid())
		{
			float panningValueByPosition = FMODAudioManager.GetPanningValueByPosition(base.transform.position);
			instance.setValue(panningValueByPosition);
		}
		return null;
	}

	public bool BleedOnImpact()
	{
		return false;
	}

	public bool SparkOnImpact()
	{
		return true;
	}
}
