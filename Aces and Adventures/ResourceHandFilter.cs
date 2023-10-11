using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField]
[ProtoInclude(10, typeof(Count))]
[ProtoInclude(11, typeof(PokerHandFilter))]
public abstract class ResourceHandFilter
{
	[ProtoContract]
	[UIField]
	public class Count : ResourceHandFilter
	{
		private static readonly RangeByte DEFAULT_RANGE = new RangeByte(1, 5, 0, 5, 0, 0);

		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Hide)]
		private PlayingCard.Filter _filter;

		[ProtoMember(2)]
		[UIField]
		private RangeByte _range = DEFAULT_RANGE;

		private bool _rangeSpecified => _range != DEFAULT_RANGE;

		public override bool IsValidHand(IEnumerable<ResourceCard> hand)
		{
			return _range.InRangeSmart(hand.Count((ResourceCard c) => _filter.IsValid(c)));
		}

		public override string ToString()
		{
			return "If " + _range.ToRangeString(null, _filter.ToString(), 100) + " in hand";
		}
	}

	[ProtoContract]
	[UIField("Poker Hand", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	public class PokerHandFilter : ResourceHandFilter
	{
		[ProtoMember(1)]
		[UIField]
		[DefaultValue(PokerHandType.Pair)]
		private PokerHandType _pokerHand = PokerHandType.Pair;

		[ProtoMember(2)]
		[UIField(tooltip = "Will return true if hand potentially contains the poker hand. (This accounts for if the hand can potentially be wilded into the poker hand, and allows for additional, unused, cards to be in hand)")]
		private bool _contains;

		public override bool IsValidHand(IEnumerable<ResourceCard> hand)
		{
			return _pokerHand.IsValidHand(hand, !_contains);
		}

		public override string ToString()
		{
			if (!_contains)
			{
				return "If " + EnumUtil.FriendlyName(_pokerHand);
			}
			return "If " + EnumUtil.FriendlyName(_pokerHand) + " in hand";
		}
	}

	public abstract bool IsValidHand(IEnumerable<ResourceCard> hand);
}
