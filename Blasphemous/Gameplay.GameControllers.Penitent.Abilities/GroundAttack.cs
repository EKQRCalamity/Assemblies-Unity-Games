using System.Collections;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class GroundAttack : LegacyAbility
{
	[SerializeField]
	private float damageDelay = 1f;

	[SerializeField]
	private CollisionSensor affectedArea;

	protected override void OnAbilityStart()
	{
		Debug.Log("Ground Attack");
		entity.Animator.SetTrigger("ATTACK");
		StartCoroutine(DamageDelay());
	}

	private IEnumerator DamageDelay()
	{
		yield return new WaitForSeconds(damageDelay);
		Entity[] affectedEntities = affectedArea.GetTouchedEntities();
		for (int i = 0; i < affectedEntities.Length; i++)
		{
			affectedEntities[i].Damage(entity.Stats.Strength.Base, string.Empty);
		}
	}
}
