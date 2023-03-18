using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PietyMonster.Animation;

[RequireComponent(typeof(BoxCollider2D))]
public class BossBodyBarrier : MonoBehaviour
{
	private Enemy _enemy;

	private Collider2D _targetCollider;

	private float _targetColliderWidth;

	private Entity _targetEntity;

	private Transform _targetTransform;

	public LayerMask TargetLayerMask;

	public BoxCollider2D BossBodyBarrierCollider { get; private set; }

	public bool AvoidCollision { get; set; }

	private void Awake()
	{
		BossBodyBarrierCollider = GetComponent<BoxCollider2D>();
	}

	private void Start()
	{
		_enemy = GetComponentInParent<Enemy>();
		if ((bool)_enemy)
		{
			_enemy.OnDeath += EnemyOnEntityDie;
		}
	}

	public void EnableCollider()
	{
		if (!BossBodyBarrierCollider.enabled)
		{
			BossBodyBarrierCollider.enabled = true;
		}
	}

	public void DisableCollider()
	{
		if (BossBodyBarrierCollider.enabled)
		{
			BossBodyBarrierCollider.enabled = false;
		}
	}

	private void EnemyOnEntityDie()
	{
		DisableCollider();
	}

	private void OnDestroy()
	{
		if ((bool)_enemy)
		{
			_enemy.OnDeath -= EnemyOnEntityDie;
		}
	}
}
