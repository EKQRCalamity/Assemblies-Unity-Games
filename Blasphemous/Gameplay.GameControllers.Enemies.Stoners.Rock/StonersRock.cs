using FMODUnity;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Stoners.Audio;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Stoners.Rock;

public class StonersRock : Weapon, IDamageable
{
	private UnityEngine.Animator _animator;

	public Enemy AttackingEntity;

	public StonersRockAudio Audio;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string hitSound;

	public bool IsBroken;

	private Rigidbody2D RigidBody;

	public AttackArea AttackArea { get; private set; }

	public void Damage(Hit hit)
	{
		Core.Audio.PlaySfxOnCatalog("StonersRockHit");
		if (!IsBroken)
		{
			BreakRock();
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		AttackArea = GetComponentInChildren<AttackArea>();
		_animator = GetComponentInChildren<UnityEngine.Animator>();
		Audio = GetComponentInChildren<StonersRockAudio>();
		RigidBody = GetComponent<Rigidbody2D>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea.OnEnter += AttackAreaOnEnter;
	}

	private void AttackAreaOnEnter(object sender, Collider2DParam collider2DParam)
	{
		GameObject gameObject = collider2DParam.Collider2DArg.gameObject;
		if (gameObject.CompareTag("Penitent"))
		{
			float @base = AttackingEntity.Stats.Strength.Base;
			Hit hit = default(Hit);
			hit.AttackingEntity = AttackingEntity.gameObject;
			hit.DamageType = DamageArea.DamageType.Normal;
			hit.DamageAmount = @base;
			hit.HitSoundId = hitSound;
			Hit weapondHit = hit;
			Attack(weapondHit);
		}
		BreakRock();
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	private void BreakRock()
	{
		if (!(_animator == null))
		{
			_animator.SetTrigger("BREAK");
			if (!IsBroken)
			{
				IsBroken = true;
			}
			if (AttackArea.WeaponCollider.enabled)
			{
				AttackArea.WeaponCollider.enabled = false;
			}
			RigidBody.velocity = Vector2.zero;
			Audio.BrokenRock();
		}
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	public void SetOwner(Enemy enemy)
	{
		WeaponOwner = (AttackingEntity = enemy);
	}

	private void OnDestroy()
	{
		AttackArea.OnEnter -= AttackAreaOnEnter;
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
