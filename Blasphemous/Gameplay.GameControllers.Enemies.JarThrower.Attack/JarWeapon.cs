using FMODUnity;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.JarThrower.Attack;

public class JarWeapon : Weapon
{
	public float DamageAmount;

	[EventRef]
	public string ImpactSound;

	[EventRef]
	public string BlastSound;

	private Hit _jarImpactHit;

	public AttackArea AttackArea { get; private set; }

	public UnityEngine.Animator Animator { get; private set; }

	public StraightProjectile Projectile { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		AttackArea = GetComponentInChildren<AttackArea>();
		Animator = GetComponent<UnityEngine.Animator>();
		Projectile = GetComponent<StraightProjectile>();
		_jarImpactHit = new Hit
		{
			AttackingEntity = base.gameObject,
			DamageAmount = DamageAmount,
			DamageElement = DamageArea.DamageElement.Contact,
			DamageType = DamageArea.DamageType.Normal,
			Force = 0f,
			HitSoundId = ImpactSound
		};
		AttackArea.OnEnter += OnEnter;
	}

	private void OnEnter(object sender, Collider2DParam e)
	{
		if (e.Collider2DArg.CompareTag("Penitent"))
		{
			Attack(_jarImpactHit);
			return;
		}
		Animator.SetTrigger("CRASH");
		Core.Audio.PlaySfx(BlastSound);
		Projectile.velocity = Vector2.zero;
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	public void Recycle()
	{
		Destroy();
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
	}
}
