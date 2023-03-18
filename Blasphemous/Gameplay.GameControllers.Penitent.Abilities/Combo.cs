using System;
using Framework.FrameworkCore;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class Combo : Ability
{
	[Serializable]
	public struct ExecutionTier
	{
		[Range(1f, 100f)]
		public int MinExecutionTier;

		[Range(1f, 100f)]
		public int MaxExecutionTier;

		public ExecutionTier(int minExecutionTier, int maxExecutionTier)
		{
			MinExecutionTier = minExecutionTier;
			MaxExecutionTier = maxExecutionTier;
		}
	}

	[SerializeField]
	public ExecutionTier DefaulExecutionTier;

	[SerializeField]
	public ExecutionTier FirstUpgradeExecutionTier;

	[SerializeField]
	public ExecutionTier SecondUpgradeExecutionTier;

	public bool IsAvailable => CanExecuteSkilledAbility();

	public UnlockableSkill GetMaxSkill => GetLastUnlockedSkill();
}
