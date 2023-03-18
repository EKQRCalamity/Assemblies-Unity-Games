using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PietyMonster.Attack;

public class PietyRoot : Weapon, IDamageable
{
	private SpriteRenderer _spriteRenderer;

	public LayerMask TargetLayer;

	[Tooltip("Damage factor based on entity damage base amount.")]
	[Range(0f, 1f)]
	public float DamageFactor;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	protected string HitSound;

	private AttackArea AttackArea { get; set; }

	public PietyRootsManager Manager { get; set; }

	public bool LaunchAttack { get; set; }

	public bool CanAttack { get; set; }

	public Animator Animator { get; set; }

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	protected override void OnAwake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea = GetComponentInChildren<AttackArea>();
		Animator = GetComponent<Animator>();
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if ((TargetLayer.value & (1 << other.gameObject.layer)) > 0 && !LaunchAttack && CanAttack)
		{
			LaunchAttack = true;
			Hit hit = default(Hit);
			hit.AttackingEntity = Manager.PietyMonster.gameObject;
			hit.DamageAmount = Manager.RootDamage;
			hit.DamageType = DamageArea.DamageType.Normal;
			hit.HitSoundId = HitSound;
			Hit weapondHit = hit;
			Attack(weapondHit);
		}
	}

	public void DisableSpriteRenderer()
	{
		if (!(_spriteRenderer == null) && _spriteRenderer.enabled)
		{
			_spriteRenderer.enabled = false;
		}
	}

	public void EnableSpriteRenderer()
	{
		if (!(_spriteRenderer == null) && !_spriteRenderer.enabled)
		{
			_spriteRenderer.enabled = true;
		}
	}

	public void AllowAttack()
	{
		if (!CanAttack)
		{
			CanAttack = true;
		}
	}

	public void DisallowAttack()
	{
		if (CanAttack)
		{
			CanAttack = !CanAttack;
		}
	}

	private void OnDisable()
	{
		if (LaunchAttack)
		{
			LaunchAttack = !LaunchAttack;
		}
	}

	public void Damage(Hit hit)
	{
		Core.Audio.PlaySfxOnCatalog("PietatSpitHit");
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public void PlayAttack()
	{
		if (!(Manager == null))
		{
			Manager.PietyMonster.Audio.RootAttack();
		}
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
