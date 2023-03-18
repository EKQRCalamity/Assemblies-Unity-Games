using System;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.DataContainer;

[CreateAssetMenu(fileName = "EnemiesBalance", menuName = "Blasphemous/Enemies Balance")]
public class EnemiesBalance : ScriptableObject
{
	[Serializable]
	public struct EnemyBalance
	{
		[SerializeField]
		[InlineEditor(InlineEditorModes.LargePreview)]
		public GameObject Enemy;

		[SerializeField]
		[Range(0f, 1000f)]
		public float LifeBase;

		[SerializeField]
		[Range(0f, 1000f)]
		public float StrengthBase;

		[SerializeField]
		[Range(0f, 1000f)]
		public float PurgePoints;

		[SerializeField]
		[Range(0f, 1000f)]
		public float ContactDamage;
	}

	[TabGroup("TabGroup", "Ground Enemies", false, 0)]
	public EnemyBalance[] GroundEnemies;

	[TabGroup("TabGroup", "Flying Enemies", false, 0)]
	public EnemyBalance[] FlyingEnemies;

	[ButtonGroup("Controls", 0)]
	[Button(ButtonSizes.Medium)]
	private void Load()
	{
		LoadResources(GroundEnemies);
		LoadResources(FlyingEnemies);
	}

	[ButtonGroup("Controls", 0)]
	[Button(ButtonSizes.Medium)]
	private void Apply()
	{
		ApplyChangesOnResources(GroundEnemies);
		ApplyChangesOnResources(FlyingEnemies);
	}

	private void LoadResources(EnemyBalance[] enemyBalances)
	{
		for (int i = 0; i < enemyBalances.Length; i++)
		{
			if (enemyBalances[i].Enemy == null)
			{
				continue;
			}
			Enemy componentInChildren = enemyBalances[i].Enemy.GetComponentInChildren<Enemy>();
			if ((bool)componentInChildren)
			{
				EnemyAttack componentInChildren2 = componentInChildren.GetComponentInChildren<EnemyAttack>();
				EntityStats stats = componentInChildren.Stats;
				enemyBalances[i].LifeBase = stats.LifeBase;
				enemyBalances[i].StrengthBase = stats.StrengthBase;
				enemyBalances[i].PurgePoints = componentInChildren.purgePointsWhenDead;
				if ((bool)componentInChildren2)
				{
					enemyBalances[i].ContactDamage = componentInChildren2.ContactDamageAmount;
				}
			}
		}
	}

	private void ApplyChangesOnResources(EnemyBalance[] enemyBalances)
	{
	}
}
