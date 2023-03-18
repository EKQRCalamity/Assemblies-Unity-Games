using System;
using System.Collections.Generic;
using Gameplay.GameControllers.Entities;
using Tools.Level;
using UnityEngine;

namespace Gameplay.GameControllers.Combat;

[RequireComponent(typeof(Collider2D))]
public class CombatFinisher : MonoBehaviour
{
	private List<Entity> enemies = new List<Entity>();

	[SerializeField]
	[Range(0f, 2f)]
	private float refreshRate = 0.5f;

	[SerializeField]
	private GameObject[] activate = new GameObject[0];

	private void Start()
	{
		InvokeRepeating("Refresh", 0f, refreshRate);
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (col.CompareTag("NPC"))
		{
			Entity componentInParent = col.GetComponentInParent<Entity>();
			if (componentInParent != null && !enemies.Contains(componentInParent))
			{
				enemies.Add(componentInParent);
			}
		}
	}

	private void Refresh()
	{
		RemoveDeadEnemies();
		if (enemies.Count == 0)
		{
			FinishLogic();
			CancelInvoke();
		}
	}

	private void RemoveDeadEnemies()
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies != null && enemies[i].Dead)
			{
				enemies.Remove(enemies[i]);
			}
		}
	}

	private void FinishLogic()
	{
		for (int i = 0; i < activate.Length; i++)
		{
			if (!(activate[i] == null))
			{
				IActionable[] components = activate[i].GetComponents<IActionable>();
				Array.ForEach(components, delegate(IActionable actionable)
				{
					actionable.Use();
				});
			}
		}
	}
}
