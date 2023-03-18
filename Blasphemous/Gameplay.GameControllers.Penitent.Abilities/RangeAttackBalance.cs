using System;
using System.Collections.Generic;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

[CreateAssetMenu(fileName = "RangeAttackDamageBalance", menuName = "Blasphemous/RangeAttack")]
public class RangeAttackBalance : ScriptableObject
{
	[Serializable]
	public struct AttackDamage
	{
		public int SwordLevel;

		public float Damage;
	}

	[SerializeField]
	public List<AttackDamage> AttackDamagesByLevel;

	public float GetDamageBySwordLevel
	{
		get
		{
			float result = 0f;
			int num = (int)Core.SkillManager.GetCurrentMeaCulpa();
			num = ((num <= 0) ? 1 : num);
			foreach (AttackDamage item in AttackDamagesByLevel)
			{
				if (item.SwordLevel != num)
				{
					continue;
				}
				result = item.Damage;
				break;
			}
			if (num > AttackDamagesByLevel.Count)
			{
				result = AttackDamagesByLevel[AttackDamagesByLevel.Count - 1].Damage;
			}
			return result;
		}
	}
}
