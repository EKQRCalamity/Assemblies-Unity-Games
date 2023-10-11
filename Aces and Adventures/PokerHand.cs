using System;
using System.Collections.Generic;

public class PokerHand : IComparable<PokerHand>
{
	public PokerHandType type;

	public List<PlayingCard> hand;

	public PokerHand(PokerHandType type, List<PlayingCard> hand)
	{
		this.type = type;
		this.hand = hand;
	}

	public PokerHand _Offset(int valueOffset)
	{
		if (valueOffset != 0)
		{
			for (int i = 0; i < hand.Count; i++)
			{
				hand[i] = hand[i].ShiftValue(valueOffset);
			}
		}
		return this;
	}

	public int CompareTo(PokerHand other)
	{
		if (this == other)
		{
			return 0;
		}
		if (other == null)
		{
			return 1;
		}
		int num = type - other.type;
		if (num != 0)
		{
			return num;
		}
		return type.CompareTo(this, other);
	}

	public override string ToString()
	{
		return string.Format("{0}: {1}, {2}: {3}", "type", type, "hand", string.Join(",", hand));
	}
}
