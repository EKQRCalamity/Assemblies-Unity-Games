using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameStepUpgradeAbility : GameStepSelectAbilityFromDeck
{
	private readonly Dictionary<DataRef<AbilityData>, DataRef<AbilityData>> _upgradeCache = new Dictionary<DataRef<AbilityData>, DataRef<AbilityData>>();

	protected override CardHandLayoutSettings _layoutSettingsOverride => base.view.adventureDeckLayout.inspectSmallSettings;

	protected override ContentRefDefaults.SelectAbilityData.SelectAbilityActions _selectedAbilityActions => ContentRef.Defaults.selectAbility.upgradeAbilityActions;

	public GameStepUpgradeAbility()
		: base(GameState.Instance.abilityDeck, MessageData.GameTooltips.UpgradeAbilityInDeck)
	{
	}

	private DataRef<AbilityData> _GetUpgrade(Ability ability)
	{
		if (ability.data.rank >= EnumUtil<AbilityData.Rank>.Max)
		{
			return null;
		}
		return _upgradeCache.GetValueOrDefault(ability.dataRef) ?? (_upgradeCache[ability.dataRef] = AbilityData.GetAbilities(base.state.player.characterClass).FirstOrDefault((DataRef<AbilityData> d) => ContentRef.Equal(d.data.upgradeOf, ability.dataRef)));
	}

	protected override bool? _CanSelectAbility(Ability ability)
	{
		return _GetUpgrade(ability);
	}

	protected override IEnumerable<GameStep> _GetStepsToRunAfterSelectActions()
	{
		yield return new GameStepGenericSimple(delegate
		{
			_selectedAbility.SetAbilityDataReference(_GetUpgrade(_selectedAbility));
		});
		yield return new GameStepWait(1f);
		yield return new GameStepGenericSimple(delegate
		{
			_selectedAbilityLayout.Add(_selectedAbility.view);
		});
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.view.abilityHoverOverTooltipTimeOverride = 0.5f;
		foreach (Ability ability in base.originalCards)
		{
			if (_CanSelectAbility(ability) == true)
			{
				CardTooltipLayout tooltipLayout = ability.abilityCard.tooltipLayout;
				tooltipLayout.onShow = (Action<Transform>)Delegate.Combine(tooltipLayout.onShow, (Action<Transform>)delegate(Transform container)
				{
					AbilityCardView abilityCardView = AbilityCardView.Create(new Ability(_GetUpgrade(ability), base.state.player, signalAbilityAdded: false).CopyResourceCostModifiersFrom(ability), container);
					abilityCardView.transform.SetWorldScale(Vector3.one);
					abilityCardView.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
					abilityCardView.frontIsVisible = true;
					abilityCardView.inputEnabled = false;
				});
			}
		}
	}

	protected override void OnDisable()
	{
		base.view.abilityHoverOverTooltipTimeOverride = null;
		foreach (Ability originalCard in base.originalCards)
		{
			originalCard.abilityCard.tooltipLayout.onShow = null;
		}
		base.OnDisable();
	}
}
