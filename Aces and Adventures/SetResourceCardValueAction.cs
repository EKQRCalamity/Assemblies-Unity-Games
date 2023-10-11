using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField("Set Resource Card Value", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Resource Card")]
public class SetResourceCardValueAction : AResourceAction
{
	[ProtoMember(1)]
	[UIField]
	[UIHorizontalLayout("T")]
	private PlayingCardValue? _value;

	[ProtoMember(2)]
	[UIField]
	[UIHorizontalLayout("T")]
	private PlayingCardSuit? _suit;

	[ProtoMember(3)]
	[UIField(collapse = UICollapseType.Hide, order = 1u)]
	private PlayingCard.Filter _filter;

	private AWild _wild;

	public AWild wild => _wild ?? (_wild = _CreateWildModification());

	protected override bool _canTick => false;

	private bool _filterSpecified => _filter;

	private SetResourceCardValueAction()
	{
	}

	public SetResourceCardValueAction(PlayingCardValue? value, PlayingCardSuit? suit)
	{
		_value = value;
		_suit = suit;
	}

	private AWild _CreateWildModification()
	{
		PlayingCardValue? value = _value;
		PlayingCardSuit? suit;
		if (value.HasValue)
		{
			PlayingCardValue valueOrDefault = value.GetValueOrDefault();
			suit = _suit;
			if (suit.HasValue)
			{
				PlayingCardSuit valueOrDefault2 = suit.GetValueOrDefault();
				return new Wild(valueOrDefault.Values().ToPlayingCardTypes(valueOrDefault2.Suits()), _filter);
			}
		}
		suit = _suit;
		if (suit.HasValue)
		{
			PlayingCardSuit valueOrDefault3 = suit.GetValueOrDefault();
			return new WildSuit(valueOrDefault3.Suits(), _filter);
		}
		value = _value;
		if (value.HasValue)
		{
			PlayingCardValue valueOrDefault4 = value.GetValueOrDefault();
			return new WildRemapValue(valueOrDefault4.Values(), _filter);
		}
		return new Wild(PlayingCardTypes.TwoOfClubs | PlayingCardTypes.ThreeOfClubs | PlayingCardTypes.FourOfClubs | PlayingCardTypes.FiveOfClubs | PlayingCardTypes.SixOfClubs | PlayingCardTypes.SevenOfClubs | PlayingCardTypes.EightOfClubs | PlayingCardTypes.NineOfClubs | PlayingCardTypes.TenOfClubs | PlayingCardTypes.JackOfClubs | PlayingCardTypes.QueenOfClubs | PlayingCardTypes.KingOfClubs | PlayingCardTypes.AceOfClubs | PlayingCardTypes.TwoOfDiamonds | PlayingCardTypes.ThreeOfDiamonds | PlayingCardTypes.FourOfDiamonds | PlayingCardTypes.FiveOfDiamonds | PlayingCardTypes.SixOfDiamonds | PlayingCardTypes.SevenOfDiamonds | PlayingCardTypes.EightOfDiamonds | PlayingCardTypes.NineOfDiamonds | PlayingCardTypes.TenOfDiamonds | PlayingCardTypes.JackOfDiamonds | PlayingCardTypes.QueenOfDiamonds | PlayingCardTypes.KingOfDiamonds | PlayingCardTypes.AceOfDiamonds | PlayingCardTypes.TwoOfHearts | PlayingCardTypes.ThreeOfHearts | PlayingCardTypes.FourOfHearts | PlayingCardTypes.FiveOfHearts | PlayingCardTypes.SixOfHearts | PlayingCardTypes.SevenOfHearts | PlayingCardTypes.EightOfHearts | PlayingCardTypes.NineOfHearts | PlayingCardTypes.TenOfHearts | PlayingCardTypes.JackOfHearts | PlayingCardTypes.QueenOfHearts | PlayingCardTypes.KingOfHearts | PlayingCardTypes.AceOfHearts | PlayingCardTypes.TwoOfSpades | PlayingCardTypes.ThreeOfSpades | PlayingCardTypes.FourOfSpades | PlayingCardTypes.FiveOfSpades | PlayingCardTypes.SixOfSpades | PlayingCardTypes.SevenOfSpades | PlayingCardTypes.EightOfSpades | PlayingCardTypes.NineOfSpades | PlayingCardTypes.TenOfSpades | PlayingCardTypes.JackOfSpades | PlayingCardTypes.QueenOfSpades | PlayingCardTypes.KingOfSpades | PlayingCardTypes.AceOfSpades, _filter);
	}

	protected override void _Apply(ActionContext context, ResourceCard resourceCard)
	{
		resourceCard.AddWildModification(wild);
		if (_filter.IsValid(resourceCard))
		{
			if (_value.HasValue)
			{
				resourceCard.value = _value.Value;
			}
			if (_suit.HasValue)
			{
				resourceCard.suit = _suit.Value;
			}
		}
	}

	protected override void _Unapply(ActionContext context, ResourceCard resourceCard)
	{
		resourceCard.RemoveWildModification(wild);
	}

	public override IEnumerable<AbilityKeyword> GetKeywords(AbilityData abilityData)
	{
		foreach (AbilityKeyword keyword in base.GetKeywords(abilityData))
		{
			yield return keyword;
		}
		yield return AbilityKeyword.Wild;
	}

	protected override string _ToStringUnique()
	{
		return "Set value to<b>" + _value.HasValue.ToText(EnumUtil.FriendlyName(_value)).PreSpaceIfNotEmpty() + _suit.HasValue.ToText(_value.HasValue.ToText("of ") + EnumUtil.FriendlyName(_suit)).PreSpaceIfNotEmpty() + "</b> for" + (_filter ? $" <size=66%>({_filter})</size>" : "");
	}
}
