using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameStepAddAbility : GameStep
{
	public const AddAbilityTypes TYPES = AddAbilityTypes.Standard | AddAbilityTypes.Buff | AddAbilityTypes.Debuff | AddAbilityTypes.Summon;

	private static readonly double NumberOfCopiesWeight = Math.Pow(4.0, 0.25);

	private AddAbilityTypes _types;

	private int _count;

	private float _survivalAbilityChance;

	private readonly Dictionary<DataRef<AbilityData>, int> _abilityCopiesInDeck = new Dictionary<DataRef<AbilityData>, int>();

	private readonly HashSet<DataRef<AbilityData>> _activeTraits = new HashSet<DataRef<AbilityData>>();

	private readonly HashSet<DataRef<AbilityData>> _possibleTraits = new HashSet<DataRef<AbilityData>>();

	private readonly HashSet<DataRef<AbilityData>> _selectedAbilityRefs = new HashSet<DataRef<AbilityData>>();

	private readonly List<Ability> _generatedAbilities = new List<Ability>();

	private ContentRefDefaults.PlayerClassData classData => ContentRef.Defaults[base.state.player.characterClass];

	public override bool canSafelyCancelStack => true;

	public GameStepAddAbility(AddAbilityTypes types = AddAbilityTypes.Standard | AddAbilityTypes.Buff | AddAbilityTypes.Debuff | AddAbilityTypes.Summon, int count = 3, float survivalAbilityChance = 0f)
	{
		_types = types;
		_count = Math.Max(1, count);
		_survivalAbilityChance = (classData.hasSurvivalAbilities ? (1f - (float)Math.Pow(1f - Mathf.Clamp01(survivalAbilityChance), 1f / (float)_count)) : 0f);
	}

	private bool _IsValid(DataRef<AbilityData> unlockedAbility)
	{
		if (unlockedAbility.data.type == AbilityData.Type.Buff)
		{
			return (_types == AddAbilityTypes.Buff) ^ classData.IsShortTermBuff(unlockedAbility);
		}
		return EnumUtil.HasFlagConvert(_types, unlockedAbility.data.type);
	}

	private void _CalculateCopiesInDeck()
	{
		foreach (Ability card in base.state.player.abilityDeck.GetCards())
		{
			if (card.data.category == AbilityData.Category.Ability && card.data.characterClass == base.state.player.characterClass)
			{
				_abilityCopiesInDeck[card.dataRef] = _abilityCopiesInDeck.GetValueOrDefault(card.dataRef) + 1;
			}
		}
	}

	private void _CalculateActiveTraits()
	{
		foreach (Ability card in base.state.player.abilityDeck.GetCards(Ability.Pile.HeroPassive))
		{
			if (card.data.characterClass == base.state.player.characterClass)
			{
				_activeTraits.Add(card.dataRef);
			}
		}
	}

	private void _CalculatePossibleTraits()
	{
		if (base.state.traitRuleset == TraitRuleset.Unrestricted)
		{
			return;
		}
		using PoolKeepItemHashSetHandle<DataRef<AbilityData>> poolKeepItemHashSetHandle = base.state.player.GetLockedLevelUpAbilities();
		foreach (Ability card in base.state.heroDeck.GetCards(HeroDeckPile.Draw))
		{
			if (card != null && card.isTrait && poolKeepItemHashSetHandle.Add(card.dataRef))
			{
				_possibleTraits.Add(card.dataRef);
			}
		}
	}

	private double _GetAbilityWeight(DataRef<AbilityData> ability)
	{
		return Math.Pow(NumberOfCopiesWeight, ability.data.rank.Max() - _abilityCopiesInDeck.GetValueOrDefault(ability)) * classData.GetWeight(_activeTraits, ability, _possibleTraits);
	}

	private void _GenerateAbility()
	{
		using PoolWRandomDHandle<DataRef<AbilityData>> poolWRandomDHandle = Pools.UseWRandomD<DataRef<AbilityData>>();
		bool num = base.state.random.Chance(_survivalAbilityChance);
		if (num)
		{
			_survivalAbilityChance = 0f;
		}
		IEnumerable<DataRef<AbilityData>> enumerable;
		if (!num)
		{
			IEnumerable<DataRef<AbilityData>> unlockedAbilities = base.state.unlockedAbilities;
			enumerable = unlockedAbilities;
		}
		else
		{
			enumerable = classData.SurvivalAbilities().Where(base.state.unlockedAbilities.Contains);
		}
		foreach (DataRef<AbilityData> item in enumerable)
		{
			if (!_selectedAbilityRefs.Contains(item) && _IsValid(item))
			{
				poolWRandomDHandle.value.Add(_GetAbilityWeight(item), item);
			}
		}
		DataRef<AbilityData> dataRef = poolWRandomDHandle.value.Random(base.state.random.NextDouble());
		if (!dataRef)
		{
			return;
		}
		foreach (DataRef<AbilityData> item2 in dataRef.GetUpgradeHierarchy(sort: false))
		{
			_selectedAbilityRefs.Add(item2);
		}
		_generatedAbilities.AddReturn(base.state.rewardDeck.Add(new Ability(dataRef, base.state.player), RewardPile.Draw) as Ability);
	}

	private void _OnAdventureClick(AdventureCard.Pile pile, ATarget card)
	{
		if (pile == AdventureCard.Pile.SelectionHand && card is Ability ability && _generatedAbilities.Contains(ability))
		{
			_generatedAbilities.Remove(ability);
			base.state.abilityDeck.Transfer(ability, Ability.Pile.Draw);
			base.finished = true;
		}
	}

	private void _OnStoneClick(Stone.Pile pile, Stone card)
	{
		if (pile == Stone.Pile.Cancel)
		{
			base.finished = true;
		}
	}

	protected override void OnFirstEnabled()
	{
		_CalculateCopiesInDeck();
		_CalculateActiveTraits();
		_CalculatePossibleTraits();
		for (int i = 0; i < _count; i++)
		{
			_GenerateAbility();
		}
	}

	protected override void OnEnable()
	{
		base.view.LogMessage(PickCount.AddAbility.Localize());
		base.state.adventureDeck.Transfer(_generatedAbilities, AdventureCard.Pile.SelectionHand);
		foreach (ATarget card in base.state.adventureDeck.GetCards(AdventureCard.Pile.SelectionHand))
		{
			card.view.RequestGlow(this, Colors.TARGET);
		}
		base.state.adventureDeck.layout.onPointerClick += _OnAdventureClick;
		base.state.stoneDeck.layout.onPointerClick += _OnStoneClick;
		MapCompassView instance = MapCompassView.Instance;
		if ((object)instance != null)
		{
			instance.canBeActiveWhileCancelIsActive = true;
		}
		base.view.stoneDeckLayout.SetLayout(Stone.Pile.Cancel, base.view.stoneDeckLayout.cancelFloating);
		base.state.buttonDeck.Layout<ButtonDeckLayout>().Activate(ButtonCardType.Skip);
		base.state.stoneDeck.Layout<StoneDeckLayout>()[StoneType.Cancel].view.RequestGlow(this, Colors.TARGET);
	}

	protected override IEnumerator Update()
	{
		while (!base.finished)
		{
			yield return null;
		}
	}

	protected override void OnDisable()
	{
		base.view.ClearMessage();
		base.state.adventureDeck.layout.onPointerClick -= _OnAdventureClick;
		base.state.stoneDeck.layout.onPointerClick -= _OnStoneClick;
		MapCompassView instance = MapCompassView.Instance;
		if ((object)instance != null)
		{
			instance.canBeActiveWhileCancelIsActive = false;
		}
		base.state.buttonDeck.Layout<ButtonDeckLayout>().Deactivate(ButtonCardType.Skip);
		base.view.stoneDeckLayout.RestoreLayoutToDefault(Stone.Pile.Cancel);
	}

	protected override void OnDestroy()
	{
		base.state.exileDeck.Transfer(_generatedAbilities, ExilePile.ClearGameState);
		AppendStep(new GameStepWait(0.666f, null, canSkip: false));
	}
}
