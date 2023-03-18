using System.Collections.Generic;
using System.Linq;
using Framework.FrameworkCore;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

[CreateAssetMenu(fileName = "AbilityDeactivator", menuName = "Blasphemous/Deactivator Skills")]
public class AbilityDeactivator : SerializedScriptableObject
{
	public List<Ability> deactivableAbilities = new List<Ability>();

	private List<Ability> abillityList = new List<Ability>();

	public void SetUp()
	{
		EnableAbilities(enableAbility: false);
	}

	private void EnableAbilities(bool enableAbility = true)
	{
		Penitent penitent = Core.Logic.Penitent;
		abillityList = penitent.GetComponentsInChildren<Ability>().ToList();
		foreach (Ability deactivableAbility in deactivableAbilities)
		{
			foreach (Ability abillity in abillityList)
			{
				if (deactivableAbility.GetType() == abillity.GetType())
				{
					abillity.enabled = enableAbility;
				}
			}
		}
	}
}
