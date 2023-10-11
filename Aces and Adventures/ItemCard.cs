using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[ProtoContract]
public class ItemCard : Ability
{
	[ProtoMember(1)]
	private AdventureCard.Item _item;

	[ProtoMember(2)]
	private bool _hasBeenUsedThisRound;

	public string purchaseCostDescription => _item.costDescription;

	public bool hasBeenUsedThisRound
	{
		get
		{
			return _hasBeenUsedThisRound;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _hasBeenUsedThisRound, value))
			{
				_SignalHasUsesRemaining(hasUsesRemaining);
			}
		}
	}

	public override bool hasUsesRemaining => !isOutOfUsesThisRound;

	public override bool isEncounterAbility => _item.isEncounterAbility;

	public bool isConsumable => _item.isConsumable;

	public ItemCardView itemView => base.view.GetComponent<ItemCardView>();

	public bool hasCost => purchaseCostDescription.HasVisibleCharacter();

	public bool isFoundItem
	{
		get
		{
			if (!hasCost)
			{
				return itemType == ItemCardType.Item;
			}
			return false;
		}
	}

	public override ItemCardType? itemType => _item?.type;

	protected override bool _canReact => !isOutOfUsesThisRound;

	public override bool isOutOfUsesThisRound => hasBeenUsedThisRound;

	public override IEnumerable<GameStep> selectedGameSteps
	{
		get
		{
			yield return new GameStepGeneric
			{
				onStart = (base.isTrait ? ((Action)delegate
				{
					base.gameState.player.AddTrait(this);
				}) : null),
				onEnd = delegate
				{
					itemView.onHasPurchaseCostChange?.Invoke(arg0: false);
				}
			};
		}
	}

	public override ResourceBlueprint<GameObject> blueprint => ItemCardView.Blueprint;

	public override Pile actPile
	{
		get
		{
			if (!base.isTrait)
			{
				return Pile.HeroAct;
			}
			return Pile.HeroPassive;
		}
	}

	private ItemCard()
	{
	}

	public ItemCard(AdventureCard.Item item, ACombatant owner = null)
	{
		_item = item;
		_Initialize(item.ability, owner);
	}

	public override void Consume()
	{
		if (!isConsumable && (hasBeenUsedThisRound = true))
		{
			base.owner.abilityDeck.Transfer(this, actPile);
		}
		else
		{
			base.gameState.adventureDeck.Transfer(this, AdventureCard.Pile.Discard);
		}
	}

	public override IEnumerable<AbilityKeyword> GetTagKeywords()
	{
		foreach (AbilityKeyword tagKeyword in base.GetTagKeywords())
		{
			if (tagKeyword.ShouldShowUpInItemTags())
			{
				yield return tagKeyword;
			}
		}
		if (base.isTrait)
		{
			yield return _item.type.GetTraitTag();
			yield break;
		}
		yield return isConsumable ? AbilityKeyword.AbilityTagConsumable : AbilityKeyword.AbilityTagUsable;
		yield return _item.type.GetActivatableTag();
	}

	public override string GetDisplayedTagString(Locale locale = null)
	{
		return GetDisplayedTags(locale).ToStringSmart(" ");
	}

	public override string GetLocalizedTagString()
	{
		return LocalizationSettings.StringDatabase.GetTable("Message")?.GetEntry(GenerateAbilityTagEntryMeta.GetKey(GetDisplayedTagString(LocalizationSettings.ProjectLocale)))?.Value ?? GetDisplayedTagString();
	}
}
