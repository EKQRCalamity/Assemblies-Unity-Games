using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.Damage;

public class EnemyDamageArea : DamageArea
{
	public delegate void EnemyDamagedEvent(GameObject damaged, Gameplay.GameControllers.Entities.Hit hit);

	public static EnemyDamagedEvent OnDamagedGlobal;

	private Enemy _enemyEntity;

	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	[SerializeField]
	private bool grantsFervour = true;

	public EnemyDamagedEvent OnDamaged;

	private int accumDamage;

	public bool GrantsFervour => grantsFervour;

	protected override void OnAwake()
	{
		base.OnAwake();
		_enemyEntity = (Enemy)base.OwnerEntity;
	}

	public void SetOwner(Enemy enemy)
	{
		_enemyEntity = enemy;
		base.OwnerEntity = enemy;
	}

	protected override void OnStart()
	{
		base.OnStart();
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
		SetPenitent();
	}

	private void SetPenitent()
	{
		if (!(Core.Logic.Penitent == null))
		{
			_penitent = Core.Logic.Penitent;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		DeltaRecoverTime += Time.deltaTime;
		if (!_penitent)
		{
			SetPenitent();
		}
	}

	private void LateUpdate()
	{
		if (accumDamage > 1)
		{
			Debug.LogErrorFormat("Accumulated Damage greater than 1! Was {0} for {1} ", accumDamage, _enemyEntity.name);
		}
		accumDamage = 0;
	}

	public void ClearAccumDamage()
	{
		accumDamage = 0;
	}

	public override void TakeDamage(Gameplay.GameControllers.Entities.Hit hit, bool force = false)
	{
		if (accumDamage > 0)
		{
			return;
		}
		base.TakeDamage(hit, force);
		accumDamage++;
		base.LastHit = hit;
		TakeDamageAmount(hit);
		if (OnDamagedGlobal != null)
		{
			OnDamagedGlobal(_enemyEntity.gameObject, hit);
		}
		if (OnDamaged != null)
		{
			OnDamaged(_enemyEntity.gameObject, hit);
		}
		if (_enemyEntity.Status.Dead)
		{
			if (damageAreaCollider.enabled)
			{
				damageAreaCollider.enabled = false;
			}
			if ((bool)_penitent)
			{
				_penitent.PenitentAttack.ResetCombo();
			}
		}
		else if (DeltaRecoverTime >= RecoverTime)
		{
			DeltaRecoverTime = 0f;
			_enemyEntity.entityCurrentState = EntityStates.Hurt;
		}
	}

	private void TakeDamageAmount(Gameplay.GameControllers.Entities.Hit hit)
	{
		if ((bool)_enemyEntity)
		{
			int num = (int)hit.DamageAmount;
			int num2 = ((!_enemyEntity.Status.Unattacable) ? num : 0);
			_enemyEntity.Damage(num2, hit.HitSoundId);
		}
	}
}
