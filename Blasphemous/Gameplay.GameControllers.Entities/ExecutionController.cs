using System;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

public class ExecutionController
{
	private List<Enemy> _killedEnemyList;

	private int _currentPointer;

	public bool CurrentEnemyCanBeExecuted { get; private set; }

	public ExecutionController()
	{
		_killedEnemyList = new List<Enemy>();
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		Entity.Death += OnEnemyDeath;
		Enemy.OnExecutionFired = (Core.SimpleEvent)Delegate.Combine(Enemy.OnExecutionFired, new Core.SimpleEvent(OnExecutionFired));
	}

	private void OnExecutionFired()
	{
		ClearKilledEntityList();
		SetCurrentPointer();
		CurrentEnemyCanBeExecuted = false;
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		SetCurrentPointer();
	}

	private void OnEnemyDeath(Entity entity)
	{
		AddKilledEnemyToList(entity);
		SetExecutionFlag();
	}

	private void SetExecutionFlag()
	{
		int count = _killedEnemyList.Count;
		if (count != 0 && count >= _currentPointer - 1)
		{
			CurrentEnemyCanBeExecuted = true;
		}
	}

	private void AddKilledEnemyToList(Entity entity)
	{
		if (entity.IsExecutable)
		{
			Enemy item = (Enemy)entity;
			if (!_killedEnemyList.Contains(item))
			{
				_killedEnemyList.Add(item);
			}
		}
	}

	public void ClearKilledEntityList()
	{
		if (_killedEnemyList.Count > 0)
		{
			_killedEnemyList.Clear();
		}
	}

	public void SetCurrentPointer()
	{
		Combo combo = Core.Logic.Penitent.PenitentAttack.Combo;
		if (combo.IsAvailable)
		{
			UnlockableSkill getMaxSkill = combo.GetMaxSkill;
			if (getMaxSkill.id.Equals("COMBO_1"))
			{
				_currentPointer = UnityEngine.Random.Range(combo.FirstUpgradeExecutionTier.MinExecutionTier, combo.FirstUpgradeExecutionTier.MaxExecutionTier);
			}
			else
			{
				_currentPointer = UnityEngine.Random.Range(combo.SecondUpgradeExecutionTier.MinExecutionTier, combo.SecondUpgradeExecutionTier.MaxExecutionTier);
			}
		}
		else
		{
			Combo.ExecutionTier defaulExecutionTier = combo.DefaulExecutionTier;
			_currentPointer = UnityEngine.Random.Range(defaulExecutionTier.MinExecutionTier, defaulExecutionTier.MaxExecutionTier);
		}
	}
}
