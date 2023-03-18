using Gameplay.GameControllers.Bosses.PietyMonster.IA;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PietyMonster.Attack;

public class PietyStompAttack : EnemyAttack
{
	public PietyMonsterBehaviour PietyMonsterBehaviour;

	public float SafeTimeOnStompAttackArea = 0.5f;

	private float _remainSafeTimeOnStompAttackArea;

	public Weapon PietyHoof;

	public float DamageAmount = 25f;

	private PietyMonster _pietyMonster;

	public Vector2 AttackDamageAreaOffset;

	public Vector2 AttackDamageAreaSize;

	[Range(0f, 2f)]
	public float DamageFactor = 1f;

	public Vector2 DefaultDamageAreaSize { get; private set; }

	public Vector2 DefaultDamageAreaOffset { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		_pietyMonster = (PietyMonster)base.EntityOwner;
		base.CurrentEnemyWeapon = PietyHoof;
	}

	protected override void OnStart()
	{
		base.OnStart();
		BoxCollider2D boxCollider2D = (BoxCollider2D)_pietyMonster.DamageArea.DamageAreaCollider;
		DefaultDamageAreaSize = new Vector2(boxCollider2D.size.x, boxCollider2D.size.y);
		DefaultDamageAreaOffset = new Vector2(boxCollider2D.offset.x, boxCollider2D.offset.y);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		CheckTargetOnRange();
	}

	private void CheckTargetOnRange()
	{
		if (PietyMonsterBehaviour.TargetOnRange)
		{
			_remainSafeTimeOnStompAttackArea -= Time.deltaTime;
			if (_remainSafeTimeOnStompAttackArea <= 0f)
			{
				_remainSafeTimeOnStompAttackArea = SafeTimeOnStompAttackArea;
				if (!PietyMonsterBehaviour.ReadyToAttack)
				{
					PietyMonsterBehaviour.ReadyToAttack = true;
				}
			}
		}
		else
		{
			PietyMonsterBehaviour.ReadyToAttack = false;
			if (PietyMonsterBehaviour.Attacking)
			{
				PietyMonsterBehaviour.Attacking = false;
			}
			_remainSafeTimeOnStompAttackArea = SafeTimeOnStompAttackArea;
		}
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		if (!(base.CurrentEnemyWeapon == null) && !_pietyMonster.Status.Dead)
		{
			Hit hit = default(Hit);
			hit.AttackingEntity = _pietyMonster.gameObject;
			hit.DamageType = DamageType;
			hit.DamageAmount = DamageAmount;
			hit.Force = Force;
			hit.HitSoundId = HitSound;
			Hit weapondHit = hit;
			base.CurrentEnemyWeapon.Attack(weapondHit);
		}
	}

	public void SetDamageAreaOffset(Vector2 newDamageAreaOffset)
	{
		_pietyMonster.DamageArea.DamageAreaCollider.offset = newDamageAreaOffset;
	}

	public void SetDamageAreaSize(Vector2 newDamageAreaSize)
	{
		BoxCollider2D boxCollider2D = (BoxCollider2D)_pietyMonster.DamageArea.DamageAreaCollider;
		boxCollider2D.size = newDamageAreaSize;
	}
}
