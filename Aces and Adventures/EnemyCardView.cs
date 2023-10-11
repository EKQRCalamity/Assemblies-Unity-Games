using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyCardView : CombatantCardView
{
	public new static readonly ResourceBlueprint<GameObject> Blueprint = "GameState/EnemyCardView";

	public Enemy enemy => base.target as Enemy;

	private void _OnTraitChanged(ACombatant entity, Ability ability)
	{
		if (entity == enemy)
		{
			onDescriptionChange?.Invoke(enemy.description);
		}
	}

	protected override IEnumerable<string> _GetTooltips()
	{
		using PoolKeepItemHashSetHandle<AbilityKeyword> tooltips = Pools.UseKeepItemHashSet<AbilityKeyword>();
		foreach (DataRef<AbilityData> trait in enemy.AllTraits())
		{
			yield return trait.data.description;
			foreach (AbilityKeyword item in trait.data.IncludedKeywords())
			{
				if (tooltips.Add(item))
				{
					string tooltip = item.GetTooltip();
					if (tooltip != null)
					{
						yield return tooltip;
					}
				}
			}
		}
	}

	protected override void _OnOffenseChange(int previousOffense, int offense)
	{
		if (base.combatant.activeCombatType == CombatType.Attack)
		{
			ActiveCombat activeCombat = base.combatant.gameState.activeCombat;
			if (activeCombat == null || !activeCombat.createdByAction)
			{
				return;
			}
		}
		base._OnOffenseChange(previousOffense, offense);
	}

	protected override void _OnDefenseChange(int previousDefense, int defense)
	{
		if (base.combatant.activeCombatType == CombatType.Defense)
		{
			ActiveCombat activeCombat = base.combatant.gameState.activeCombat;
			if (activeCombat == null || !activeCombat.createdByAction)
			{
				return;
			}
		}
		base._OnDefenseChange(previousDefense, defense);
	}

	protected override void _OnCardChange()
	{
		base._OnCardChange();
		if (enemy != null)
		{
			enemy.gameState.onTraitAdded += _OnTraitChanged;
			enemy.gameState.onTraitRemoved += _OnTraitChanged;
			_OnTraitChanged(enemy, null);
		}
	}

	protected override void _UnregisterCombatant(ACombatant combatant)
	{
		Enemy obj = (Enemy)combatant;
		obj.gameState.onTraitAdded -= _OnTraitChanged;
		obj.gameState.onTraitRemoved -= _OnTraitChanged;
		base._UnregisterCombatant(combatant);
	}

	protected override void _OnShieldChange(int previousShield, int shield)
	{
		base._OnShieldChange(previousShield, shield);
		if ((previousShield <= 0) ^ (shield <= 0))
		{
			base.combatText.GetComponent<TextMeshProUGUI>().margin = new Vector4(0f, 0f, 0f, _shieldDice ? 168 : 0);
		}
	}

	public void ShowTraitsWithoutFadingInactive()
	{
		if (enemy != null)
		{
			onDescriptionChange?.Invoke(enemy.GetTraitDescription(fadeOutInactiveTraits: false));
		}
	}
}
