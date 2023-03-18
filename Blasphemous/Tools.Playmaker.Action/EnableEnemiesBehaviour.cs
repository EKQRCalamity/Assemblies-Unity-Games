using Gameplay.GameControllers.Entities;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Tools.PlayMaker.Action;

[ActionCategory(ActionCategory.Array)]
[HutongGames.PlayMaker.Tooltip("Enables de behaviour of a group of enemies.")]
public class EnableEnemiesBehaviour : FsmStateAction
{
	[HutongGames.PlayMaker.Tooltip("The set of enemies.")]
	[ArrayEditor(VariableType.GameObject, "", 0, 0, 65536)]
	public FsmArray EnemySpawners;

	[RequiredField]
	[UIHint(UIHint.Variable)]
	[HutongGames.PlayMaker.Tooltip("The Array Variable to use.")]
	public FsmArray array;

	public FsmBool EnableBehaviour;

	public override void Reset()
	{
		array = null;
	}

	public override void OnEnter()
	{
		EnableEnemies();
		Finish();
	}

	private void EnableEnemies()
	{
		if (this.array.Length < 1)
		{
			return;
		}
		Enemy[] array = Object.FindObjectsOfType<Enemy>();
		object[] values = this.array.Values;
		foreach (object obj in values)
		{
			Enemy[] array2 = array;
			foreach (Enemy enemy in array2)
			{
				if (enemy.SpawningId == obj.GetHashCode())
				{
					enemy.EnemyBehaviour.EnableBehaviourOnLoad = EnableBehaviour.Value;
					if (EnableBehaviour.Value)
					{
						enemy.EnemyBehaviour.StartBehaviour();
					}
					else
					{
						enemy.EnemyBehaviour.StopBehaviour();
					}
					break;
				}
			}
		}
	}
}
