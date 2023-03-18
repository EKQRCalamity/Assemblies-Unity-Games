using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.MeltedLady;

public class InkLady : FloatingLady, IDamageable
{
	public float BeamAttackTime = 2f;

	private EnemyBehaviour behaviour;

	public BossHomingLaserAttack Attack { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Attack = GetComponentInChildren<BossHomingLaserAttack>();
		behaviour = GetComponent<EnemyBehaviour>();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!behaviour.enabled)
		{
			behaviour.enabled = true;
		}
	}

	public void Damage(Hit hit)
	{
		base.DamageArea.TakeDamage(hit);
		base.ColorFlash.TriggerColorFlash();
		if (Status.Dead)
		{
			base.DamageArea.DamageAreaCollider.enabled = false;
			AnimatorInyector.Death();
			Attack.Clear();
		}
		else
		{
			AnimatorInyector.Hurt();
		}
		SleepTimeByHit(hit);
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}
}
