using FMODUnity;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.CowardTrapper.AI;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.CowardTrapper.Attack;

public class CowardTrap : Weapon, IDamageable
{
	[Tooltip("Destroy the trap after the destruction time")]
	public bool UseLifeTime = true;

	public float SelfDestructTime = 4f;

	private float _lifeTime;

	private bool _destroyed;

	protected CowardTrapperBehaviour CowardTrapperBehaviour;

	[EventRef]
	public string HitSound;

	[EventRef]
	public string GrowSound;

	[EventRef]
	public string DamageSound;

	[EventRef]
	public string PlayerDamage;

	public AttackArea AttackArea { get; private set; }

	public UnityEngine.Animator Animator { get; private set; }

	private Hit GetHit
	{
		get
		{
			Hit result = default(Hit);
			result.DamageAmount = WeaponOwner.Stats.Strength.Final;
			result.AttackingEntity = base.gameObject;
			result.DamageType = DamageArea.DamageType.Normal;
			result.HitSoundId = PlayerDamage;
			result.Unnavoidable = true;
			return result;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea = base.AttackAreas[0];
		Animator = GetComponent<UnityEngine.Animator>();
		AttackArea.OnEnter += OnEnter;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (UseLifeTime)
		{
			_lifeTime += Time.deltaTime;
		}
		if (_lifeTime >= SelfDestructTime)
		{
			Dispose();
		}
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		if (AttackArea == null)
		{
			AttackArea = GetComponentInChildren<AttackArea>();
		}
		AttackArea.WeaponCollider.enabled = true;
		_lifeTime = 0f;
		_destroyed = false;
		Core.Audio.PlaySfx(GrowSound);
	}

	private void OnEnter(object sender, Collider2DParam e)
	{
		if (e.Collider2DArg.CompareTag("Penitent"))
		{
			e.Collider2DArg.GetComponentInParent<IDamageable>().Damage(GetHit);
			Dispose();
		}
	}

	private void Dispose()
	{
		if (!_destroyed)
		{
			_destroyed = true;
			Core.Audio.PlaySfx(HitSound);
			Animator.SetTrigger("DESTROY");
			CowardTrapperBehaviour.RemoveTrap(this);
			AttackArea.WeaponCollider.enabled = false;
		}
	}

	public void SetOwner(Entity owner)
	{
		CowardTrapperBehaviour = owner.GetComponent<CowardTrapperBehaviour>();
		AttackArea.Entity = owner;
		WeaponOwner = owner;
	}

	public void Damage(Hit hit)
	{
		Core.Audio.PlaySfx(DamageSound);
		Dispose();
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public bool BleedOnImpact()
	{
		return false;
	}

	public bool SparkOnImpact()
	{
		return false;
	}
}
