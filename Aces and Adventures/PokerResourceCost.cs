using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;
using TMPro;
using UnityEngine;

[ProtoContract]
[UIField("Poker Hand", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
public class PokerResourceCost : AResourceCosts
{
	private static readonly ResourceBlueprint<GameObject> _CostBlueprint = "GameState/Ability/Cost/PokerCost";

	[ProtoMember(1)]
	[UIField]
	[DefaultValue(PokerHandType.Pair)]
	private PokerHandType _pokerHand = PokerHandType.Pair;

	public PokerHandType pokerHand => _pokerHand;

	public override ResourceCardCounts cardCounts => (ResourceCardCounts)(1 << _pokerHand.Size());

	private PokerResourceCost()
	{
	}

	public PokerResourceCost(PokerHandType pokerHand)
	{
		_pokerHand = pokerHand;
	}

	public override PoolKeepItemListHandle<ResourceCard> GetActivationCards(IEnumerable<ResourceCard> availableCards)
	{
		return _pokerHand.GetActivationHand(availableCards);
	}

	public override AbilityPreventedBy? GetMissingResourceType(IEnumerable<ResourceCard> availableCards)
	{
		return _pokerHand.GetAbilityPreventedBy();
	}

	protected override IEnumerable<AbilityPreventedBy?> _GetUniqueAbilityPreventedBys()
	{
		yield return _pokerHand.GetAbilityPreventedBy();
	}

	protected override IEnumerable<GameObject> _GetUniqueCostIcons()
	{
		yield return Object.Instantiate((GameObject)_CostBlueprint).GetComponentInChildren<TextMeshProUGUI>().SetTextReturnLocalized(() => _pokerHand.ShortText())
			.transform.root.gameObject.GetOrAddComponent<LocalizedStringRef>().SetData(ResourceCostIconType.PokerHand.GetTooltip(), new(LocalizedVariableName, object)[1] { (LocalizedVariableName.Value, _pokerHand.Localize().SetArgumentsCloned(1)) }).gameObject;
	}

	protected override void _WildIntoCost(PoolKeepItemListHandle<ResourceCard> activationCards)
	{
		activationCards.value.GetCombatHand(EnumUtil<PokerHandType>.ConvertToFlag<PokerHandTypes>(_pokerHand), EnumUtil<PokerHandType>.Values).WildIntoPokerHand();
	}

	public override bool Equals(AResourceCosts other)
	{
		if (other is PokerResourceCost pokerResourceCost && _pokerHand == pokerResourceCost._pokerHand)
		{
			return base.Equals(other);
		}
		return false;
	}

	public override string ToString()
	{
		return $"Poker Hand: {_pokerHand}" + _additionalCosts.ToString().PreSpaceIfNotEmpty();
	}
}
