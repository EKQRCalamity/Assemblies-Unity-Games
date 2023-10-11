using System;
using System.Collections.Generic;

public static class PlayingCardExtensions
{
	public static PlayingCardValue Shift(this PlayingCardValue value, int shift)
	{
		return (PlayingCardValue)Math.Min(14, Math.Max(2, (int)(value + shift)));
	}

	public static PlayingCardValueAceLow Shift(this PlayingCardValueAceLow value, int shift)
	{
		return (PlayingCardValueAceLow)Math.Min(14, Math.Max(1, (int)(value + shift)));
	}

	public static PlayingCardValues Values(this PlayingCardValue value)
	{
		return (PlayingCardValues)(1 << (int)value);
	}

	public static PlayingCardValue Value(this PlayingCardType card)
	{
		return (PlayingCardValue)((int)(card - 2) % 13 + 2);
	}

	public static PlayingCardSuit Suit(this PlayingCardType card)
	{
		return (PlayingCardSuit)((int)(card - 2) / 13);
	}

	public static IEnumerable<PlayingCardValue> Values(this PlayingCardValues values)
	{
		for (int x = 2; x <= 14; x++)
		{
			if (((uint)values & (uint)(1 << x)) != 0)
			{
				yield return (PlayingCardValue)x;
			}
		}
	}

	public static PlayingCardTypes ToPlayingCardTypes(this PlayingCardValues values, PlayingCardSuits suits)
	{
		PlayingCardTypes playingCardTypes = (PlayingCardTypes)0L;
		for (int i = 0; i <= 3; i++)
		{
			if (((uint)suits & (uint)(1 << i)) != 0)
			{
				playingCardTypes = (PlayingCardTypes)((long)playingCardTypes | ((long)values << 13 * i));
			}
		}
		return playingCardTypes;
	}

	public static int Range(this PlayingCardValuesAceLow values)
	{
		return (int)Math.Log(((int)values).HighestBit(), 2.0) - (int)Math.Log(((int)values).LowestBit(), 2.0);
	}

	public static int LowestBit(this int x)
	{
		return ~(x & (x - 1)) & x;
	}

	public static int HighestBit(this int x)
	{
		int result = x;
		while (x != 0)
		{
			result = x;
			x &= x - 1;
		}
		return result;
	}

	public static IEnumerable<PlayingCardSuit> Suits(this PlayingCardSuits suits)
	{
		for (int x = 0; x <= 3; x++)
		{
			if (((uint)suits & (uint)(1 << x)) != 0)
			{
				yield return (PlayingCardSuit)x;
			}
		}
	}

	public static PlayingCardSuits Suits(this PlayingCardSuit suit)
	{
		return (PlayingCardSuits)(1 << (int)suit);
	}

	public static PlayingCardTypes ToPlayingCardTypes(this PlayingCardSuits suits, PlayingCardValues values)
	{
		return values.ToPlayingCardTypes(suits);
	}

	public static IEnumerable<PlayingCardType> Cards(this PlayingCardTypes cards)
	{
		for (int x = 2; x <= 53; x++)
		{
			if (((ulong)cards & (ulong)(1L << x)) != 0L)
			{
				yield return (PlayingCardType)x;
			}
		}
	}

	public static IEnumerable<PlayingCard> PlayingCards(this PlayingCardTypes cards)
	{
		foreach (PlayingCardType item in cards.Cards())
		{
			yield return item;
		}
	}

	public static PlayingCardValues Values(this PlayingCardTypes cards)
	{
		PlayingCardValues playingCardValues = (PlayingCardValues)0;
		for (int i = 0; i < 4; i++)
		{
			playingCardValues = (PlayingCardValues)((int)playingCardValues | ((int)((long)cards >> i * 13) & 0x7FFC));
		}
		return playingCardValues;
	}

	public static PlayingCardSuits Suits(this PlayingCardTypes cards)
	{
		PlayingCardSuits playingCardSuits = (PlayingCardSuits)0;
		for (int i = 0; i < 4; i++)
		{
			if (((uint)(int)((long)cards >> i * 13) & 0x7FFCu) != 0)
			{
				playingCardSuits = (PlayingCardSuits)((int)playingCardSuits | (1 << i));
			}
		}
		return playingCardSuits;
	}
}
