using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using Gameplay.GameControllers.Penitent.Attack;
using UnityEngine;

namespace Framework.Inventory;

public class BidirectionalParryBeadEffect : ObjectEffect
{
	private PenitentSword _penitentSword { get; set; }

	private Parry _parryAbility { get; set; }

	private bool IsEquiped { get; set; }

	private void LoadDependencies()
	{
		_penitentSword = (PenitentSword)Core.Logic.Penitent.PenitentAttack.CurrentPenitentWeapon;
		_parryAbility = Core.Logic.Penitent.Parry;
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
		LevelManager.OnBeforeLevelLoad += BeforeLevelLoad;
	}

	private void OnPlayerSpawn(Penitent penitent)
	{
		LoadDependencies();
		_penitentSword.OnParry += OnParry;
	}

	private void BeforeLevelLoad(Level oldlevel, Level newlevel)
	{
		if ((bool)_penitentSword)
		{
			_penitentSword.OnParry -= OnParry;
		}
	}

	private void OnParry(object param)
	{
		if (IsEquiped)
		{
			Entity entity = (Entity)param;
			Vector3 position = entity.transform.position;
			FaceToEnemy(position);
		}
	}

	protected override bool OnApplyEffect()
	{
		IsEquiped = true;
		if ((bool)_penitentSword)
		{
			_penitentSword.CheckParryEnemyDirection = false;
		}
		return true;
	}

	protected override void OnRemoveEffect()
	{
		base.OnRemoveEffect();
		if ((bool)_penitentSword)
		{
			_penitentSword.CheckParryEnemyDirection = true;
		}
		IsEquiped = false;
	}

	private static void FaceToEnemy(Vector3 enemyPosition)
	{
		Core.Logic.Penitent.SetOrientationbyHit(enemyPosition);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
		LevelManager.OnBeforeLevelLoad -= BeforeLevelLoad;
	}
}
