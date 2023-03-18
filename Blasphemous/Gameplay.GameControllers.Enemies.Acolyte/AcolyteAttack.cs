using Gameplay.GameControllers.Enemies.Acolyte.IA;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;

namespace Gameplay.GameControllers.Enemies.Acolyte;

public class AcolyteAttack : EnemyAttack
{
	private Acolyte _acolyte;

	private AttackArea _attackArea;

	private AcolyteBehaviour _behaviour;

	private bool _reverse;

	public bool TriggerAttack { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = GetComponentInChildren<Weapon>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_acolyte = GetComponentInParent<Acolyte>();
		_attackArea = _acolyte.GetComponentInChildren<AttackArea>();
		_behaviour = (AcolyteBehaviour)_acolyte.EnemyBehaviour;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (_attackArea.EnemyIsInAttackArea && _behaviour.IsAttackWindowOpen && !TriggerAttack)
		{
			TriggerAttack = true;
			CurrentWeaponAttack(DamageType);
		}
	}

	public override void CurrentWeaponAttack(DamageArea.DamageType damageType)
	{
		base.CurrentWeaponAttack(damageType);
		if ((bool)base.CurrentEnemyWeapon)
		{
			Hit hit = default(Hit);
			hit.AttackingEntity = _acolyte.gameObject;
			hit.DamageType = damageType;
			hit.DamageAmount = _acolyte.Stats.Strength.Final;
			hit.HitSoundId = HitSound;
			hit.Force = Force;
			Hit weapondHit = hit;
			base.CurrentEnemyWeapon.Attack(weapondHit);
		}
	}
}
