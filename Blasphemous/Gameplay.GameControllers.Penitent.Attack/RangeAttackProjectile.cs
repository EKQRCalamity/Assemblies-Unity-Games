using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Gameplay.GameControllers.Penitent.Abilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Attack;

public class RangeAttackProjectile : Weapon
{
	private const string DecelerationTweenId = "Deceleration";

	private const float VanishingTime = 0.3f;

	[FoldoutGroup("Damage Settings", true, 0)]
	public RangeAttackBalance RangeAttackDamageBySwordLevel;

	[FoldoutGroup("Motion Settings", true, 0)]
	public float Speed;

	[FoldoutGroup("Motion Settings", true, 0)]
	public float MaxSpeed;

	[FoldoutGroup("Motion Settings", true, 0)]
	public float Acceleration;

	[FoldoutGroup("Motion Settings", true, 0)]
	public float Range;

	[FoldoutGroup("Motion Settings", true, 0)]
	public float Lifetime;

	private float _currentLifeTime;

	private float _returningTime;

	[FoldoutGroup("Hit Settings", true, 0)]
	public LayerMask EnemyLayer;

	[FoldoutGroup("Hit Settings", true, 0)]
	public LayerMask BlockLayer;

	[SerializeField]
	[FoldoutGroup("Audio", 0)]
	[EventRef]
	protected string HitSound;

	[SerializeField]
	[FoldoutGroup("Audio", 0)]
	[EventRef]
	protected string VanishSound;

	[SerializeField]
	[FoldoutGroup("Audio", 0)]
	[EventRef]
	protected string FlightSound;

	private Vector2 _dir;

	private bool _vanishing;

	private bool _returning;

	private bool _blocked;

	private bool _isExplode;

	private UnityEngine.Animator _projectileAnimator;

	private EventInstance _rangeAttackFlightAudioInstance;

	public Entity Owner
	{
		get
		{
			return WeaponOwner;
		}
		set
		{
			WeaponOwner = value;
		}
	}

	public AttackArea AttackArea { get; set; }

	public RangeAttack RangeAttackAbility { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		_dir = ((WeaponOwner.Status.Orientation != 0) ? (-Vector2.right) : Vector2.right);
		_projectileAnimator = GetComponent<UnityEngine.Animator>();
		AttackArea = GetComponent<AttackArea>();
		AttackArea.Entity = WeaponOwner;
		RangeAttackAbility.ProjectileIsRunning = true;
		PlayFlightFx();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		_currentLifeTime += Time.deltaTime;
		switch (RangeAttackAbility.LastUnlockedSkillId)
		{
		case "RANGED_1":
			BasicMotion();
			break;
		case "RANGED_2":
			BoomerangMotion();
			break;
		case "RANGED_3":
			BoomerangMotion();
			break;
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if ((EnemyLayer.value & (1 << other.gameObject.layer)) > 0)
		{
			Attack(GetHit());
		}
		if ((BlockLayer.value & (1 << other.gameObject.layer)) > 0)
		{
			_blocked = true;
			Speed = 0f;
			Vanish(0f);
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if ((EnemyLayer.value & (1 << other.gameObject.layer)) <= 0)
		{
		}
	}

	public override void Attack(Hit weaponHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weaponHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	private Hit GetHit()
	{
		float damageAmount = RangeAttackDamageBySwordLevel.GetDamageBySwordLevel * Core.Logic.Penitent.Stats.RangedStrength.Final;
		Hit result = default(Hit);
		result.AttackingEntity = base.gameObject;
		result.DamageType = DamageArea.DamageType.Normal;
		result.DamageAmount = damageAmount;
		result.Force = 0f;
		result.HitSoundId = HitSound;
		return result;
	}

	private void BasicMotion()
	{
		if (_blocked)
		{
			Vanish(0f);
		}
		else if (_currentLifeTime >= Lifetime && !_vanishing)
		{
			Vanish(0.3f);
		}
		else
		{
			LinearMotion();
		}
	}

	private void BoomerangMotion()
	{
		if (_blocked)
		{
			Vanish(0f);
			ProjectileExplosion();
			return;
		}
		if (_currentLifeTime >= Lifetime && !_returning && !DOTween.IsTweening("Deceleration"))
		{
			Deceleration(0.1f, OnDecelerationFinish);
		}
		if (_returning)
		{
			_returningTime += Time.deltaTime;
			if (_returningTime >= Lifetime && !_vanishing)
			{
				Vanish(0.3f);
			}
			if (_vanishing)
			{
				LinearMotion();
			}
		}
		if (!_vanishing)
		{
			LinearMotion();
		}
	}

	private void LinearMotion()
	{
		if (!DOTween.IsTweening("Deceleration"))
		{
			Speed = Mathf.Min(Speed + Acceleration * Time.deltaTime, MaxSpeed);
		}
		if (!_returning)
		{
			base.transform.Translate(_dir * Speed * Time.deltaTime);
			return;
		}
		Vector3 vector = Core.Logic.Penitent.DamageArea.Center();
		vector.y = Core.Logic.Penitent.DamageArea.Center().y + 0.2f;
		base.transform.Translate((vector - base.transform.position).normalized * Speed * Time.deltaTime);
	}

	private void Deceleration(float decelerationTime, TweenCallback callback)
	{
		DOTween.To(delegate(float x)
		{
			Speed = x;
		}, Speed, 0f, decelerationTime).SetId("Deceleration").OnComplete(callback);
	}

	private void OnDecelerationFinish()
	{
		_returning = true;
		ProjectileExplosion();
	}

	private void ProjectileExplosion()
	{
		if (!_isExplode)
		{
			_isExplode = true;
			if (RangeAttackAbility.LastUnlockedSkillId.Equals("RANGED_3"))
			{
				RangeAttackAbility.InstantiateExplosion(base.transform.position);
			}
		}
	}

	public void PlayFlightFx()
	{
		if (!_rangeAttackFlightAudioInstance.isValid())
		{
			_rangeAttackFlightAudioInstance = Core.Audio.CreateEvent(FlightSound);
			_rangeAttackFlightAudioInstance.start();
		}
	}

	public void StopFlightFx()
	{
		if (_rangeAttackFlightAudioInstance.isValid())
		{
			_rangeAttackFlightAudioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_rangeAttackFlightAudioInstance.release();
			_rangeAttackFlightAudioInstance = default(EventInstance);
		}
	}

	public void Vanish(float vanishTime)
	{
		if (!(_projectileAnimator == null))
		{
			if (!_vanishing)
			{
				_vanishing = true;
			}
			_projectileAnimator.SetTrigger("VANISH");
			AttackArea.WeaponCollider.enabled = false;
			if (!DOTween.IsTweening("Deceleration"))
			{
				Deceleration(vanishTime, null);
			}
			Core.Audio.EventOneShotPanned(VanishSound, base.transform.position);
			StopFlightFx();
		}
	}

	public void ResetMotionStatus()
	{
		_currentLifeTime = 0f;
		_returningTime = 0f;
		Speed = 0f;
		_vanishing = false;
		_returning = false;
		_blocked = false;
		_isExplode = false;
		_dir = ((WeaponOwner.Status.Orientation != 0) ? (-Vector2.right) : Vector2.right);
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		Owner = Core.Logic.Penitent;
		RangeAttackAbility = Core.Logic.Penitent.RangeAttack;
		RangeAttackAbility.ProjectileIsRunning = true;
		if ((bool)AttackArea)
		{
			AttackArea.WeaponCollider.enabled = true;
		}
		ResetMotionStatus();
		PlayFlightFx();
	}

	public void Store()
	{
		RangeAttackAbility.ProjectileIsRunning = false;
		Destroy();
	}
}
