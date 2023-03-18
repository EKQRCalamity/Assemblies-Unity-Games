using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Gameplay.GameControllers.Penitent;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PietyMonster.Attack;

public class PietyBush : Weapon, IDamageable, IDirectAttack
{
	private Hit bushHit;

	public float Life = 7f;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	protected string HitSound;

	public float DamageAmount;

	public BoxCollider2D Collider { get; set; }

	public Animator Animator { get; set; }

	public ColorFlash Flash { get; set; }

	public AttackArea AttackArea { get; set; }

	public PietyMonster Owner { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		AttackArea = GetComponentInChildren<AttackArea>();
		Collider = GetComponent<BoxCollider2D>();
		Animator = GetComponentInChildren<Animator>();
		Flash = GetComponent<ColorFlash>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		if ((bool)Owner)
		{
			bushHit = GetHit();
		}
		Core.Audio.PlaySfxOnCatalog("PietatSpitGrow");
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Penitent") && Owner != null)
		{
			Gameplay.GameControllers.Penitent.Penitent componentInParent = other.GetComponentInParent<Gameplay.GameControllers.Penitent.Penitent>();
			if (componentInParent.Status.Unattacable)
			{
				ForcedAttackToTarget(componentInParent, bushHit);
				DestroyBush();
			}
			else
			{
				Attack(bushHit);
			}
		}
		if (other.gameObject.layer == LayerMask.NameToLayer("Enemy") && !other.CompareTag("NPC"))
		{
			DestroyBush();
		}
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
		DestroyBush();
	}

	public void SetOwner(PietyMonster pietyMonster)
	{
		Owner = pietyMonster;
		WeaponOwner = Owner;
		AttackArea.Entity = Owner;
	}

	private void ForcedAttackToTarget(Gameplay.GameControllers.Penitent.Penitent penitent, Hit rootAttack)
	{
		penitent.DamageArea.TakeDamage(rootAttack, force: true);
	}

	private Hit GetHit()
	{
		Hit result = default(Hit);
		result.AttackingEntity = base.gameObject;
		result.DamageAmount = DamageAmount;
		result.DamageType = DamageArea.DamageType.Normal;
		result.HitSoundId = HitSound;
		return result;
	}

	public void Damage(Hit hit)
	{
		Flash.TriggerColorFlash();
		Life -= hit.DamageAmount;
		Core.Audio.PlaySfxOnCatalog("PietatSpitHit");
		if (Life <= 0f)
		{
			if (Collider.enabled)
			{
				Collider.enabled = false;
			}
			Animator.SetTrigger("DESTROY");
			Core.Audio.PlaySfxOnCatalog("PietatSpitDestroyHit");
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public void DestroyBush()
	{
		if (Collider.enabled)
		{
			Collider.enabled = false;
		}
		Animator.SetTrigger("DESTROY");
	}

	public bool BleedOnImpact()
	{
		return false;
	}

	public bool SparkOnImpact()
	{
		return true;
	}

	public void CreateHit()
	{
	}

	public void SetDamage(int damage)
	{
		bushHit.DamageAmount = damage;
		DamageAmount = damage;
	}
}
