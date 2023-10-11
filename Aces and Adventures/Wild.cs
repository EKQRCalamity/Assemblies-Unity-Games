using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
public class Wild : AWild
{
	private PlayingCardTypes _wilds = PlayingCardTypes.TwoOfClubs | PlayingCardTypes.ThreeOfClubs | PlayingCardTypes.FourOfClubs | PlayingCardTypes.FiveOfClubs | PlayingCardTypes.SixOfClubs | PlayingCardTypes.SevenOfClubs | PlayingCardTypes.EightOfClubs | PlayingCardTypes.NineOfClubs | PlayingCardTypes.TenOfClubs | PlayingCardTypes.JackOfClubs | PlayingCardTypes.QueenOfClubs | PlayingCardTypes.KingOfClubs | PlayingCardTypes.AceOfClubs | PlayingCardTypes.TwoOfDiamonds | PlayingCardTypes.ThreeOfDiamonds | PlayingCardTypes.FourOfDiamonds | PlayingCardTypes.FiveOfDiamonds | PlayingCardTypes.SixOfDiamonds | PlayingCardTypes.SevenOfDiamonds | PlayingCardTypes.EightOfDiamonds | PlayingCardTypes.NineOfDiamonds | PlayingCardTypes.TenOfDiamonds | PlayingCardTypes.JackOfDiamonds | PlayingCardTypes.QueenOfDiamonds | PlayingCardTypes.KingOfDiamonds | PlayingCardTypes.AceOfDiamonds | PlayingCardTypes.TwoOfHearts | PlayingCardTypes.ThreeOfHearts | PlayingCardTypes.FourOfHearts | PlayingCardTypes.FiveOfHearts | PlayingCardTypes.SixOfHearts | PlayingCardTypes.SevenOfHearts | PlayingCardTypes.EightOfHearts | PlayingCardTypes.NineOfHearts | PlayingCardTypes.TenOfHearts | PlayingCardTypes.JackOfHearts | PlayingCardTypes.QueenOfHearts | PlayingCardTypes.KingOfHearts | PlayingCardTypes.AceOfHearts | PlayingCardTypes.TwoOfSpades | PlayingCardTypes.ThreeOfSpades | PlayingCardTypes.FourOfSpades | PlayingCardTypes.FiveOfSpades | PlayingCardTypes.SixOfSpades | PlayingCardTypes.SevenOfSpades | PlayingCardTypes.EightOfSpades | PlayingCardTypes.NineOfSpades | PlayingCardTypes.TenOfSpades | PlayingCardTypes.JackOfSpades | PlayingCardTypes.QueenOfSpades | PlayingCardTypes.KingOfSpades | PlayingCardTypes.AceOfSpades;

	[ProtoMember(1)]
	[DefaultValue(18014398509481980L)]
	private long _wildsShim
	{
		get
		{
			return (long)_wilds;
		}
		set
		{
			_wilds = (PlayingCardTypes)value;
		}
	}

	public Wild()
	{
	}

	public Wild(PlayingCardTypes wilds, PlayingCard.Filter? filter = null)
		: base(filter)
	{
		_wilds = wilds;
	}

	protected override void _Process(ref PlayingCardTypes output)
	{
		output |= _wilds;
	}

	public override IEnumerable<AbilityKeyword> GetKeywords()
	{
		yield return AbilityKeyword.Wild;
	}

	public override string ToString()
	{
		return ((_wilds != (PlayingCardTypes.TwoOfClubs | PlayingCardTypes.ThreeOfClubs | PlayingCardTypes.FourOfClubs | PlayingCardTypes.FiveOfClubs | PlayingCardTypes.SixOfClubs | PlayingCardTypes.SevenOfClubs | PlayingCardTypes.EightOfClubs | PlayingCardTypes.NineOfClubs | PlayingCardTypes.TenOfClubs | PlayingCardTypes.JackOfClubs | PlayingCardTypes.QueenOfClubs | PlayingCardTypes.KingOfClubs | PlayingCardTypes.AceOfClubs | PlayingCardTypes.TwoOfDiamonds | PlayingCardTypes.ThreeOfDiamonds | PlayingCardTypes.FourOfDiamonds | PlayingCardTypes.FiveOfDiamonds | PlayingCardTypes.SixOfDiamonds | PlayingCardTypes.SevenOfDiamonds | PlayingCardTypes.EightOfDiamonds | PlayingCardTypes.NineOfDiamonds | PlayingCardTypes.TenOfDiamonds | PlayingCardTypes.JackOfDiamonds | PlayingCardTypes.QueenOfDiamonds | PlayingCardTypes.KingOfDiamonds | PlayingCardTypes.AceOfDiamonds | PlayingCardTypes.TwoOfHearts | PlayingCardTypes.ThreeOfHearts | PlayingCardTypes.FourOfHearts | PlayingCardTypes.FiveOfHearts | PlayingCardTypes.SixOfHearts | PlayingCardTypes.SevenOfHearts | PlayingCardTypes.EightOfHearts | PlayingCardTypes.NineOfHearts | PlayingCardTypes.TenOfHearts | PlayingCardTypes.JackOfHearts | PlayingCardTypes.QueenOfHearts | PlayingCardTypes.KingOfHearts | PlayingCardTypes.AceOfHearts | PlayingCardTypes.TwoOfSpades | PlayingCardTypes.ThreeOfSpades | PlayingCardTypes.FourOfSpades | PlayingCardTypes.FiveOfSpades | PlayingCardTypes.SixOfSpades | PlayingCardTypes.SevenOfSpades | PlayingCardTypes.EightOfSpades | PlayingCardTypes.NineOfSpades | PlayingCardTypes.TenOfSpades | PlayingCardTypes.JackOfSpades | PlayingCardTypes.QueenOfSpades | PlayingCardTypes.KingOfSpades | PlayingCardTypes.AceOfSpades)) ? ("Wild to " + EnumUtil.FriendlyNameFlagRanges(_wilds)) : "Wild") + base._filterString;
	}

	public override bool Equals(AWild other)
	{
		if (base.Equals(other) && other is Wild wild)
		{
			return wild._wilds == _wilds;
		}
		return false;
	}

	public override AWild CombineWith(AWild other)
	{
		if (_wilds == (PlayingCardTypes.TwoOfClubs | PlayingCardTypes.ThreeOfClubs | PlayingCardTypes.FourOfClubs | PlayingCardTypes.FiveOfClubs | PlayingCardTypes.SixOfClubs | PlayingCardTypes.SevenOfClubs | PlayingCardTypes.EightOfClubs | PlayingCardTypes.NineOfClubs | PlayingCardTypes.TenOfClubs | PlayingCardTypes.JackOfClubs | PlayingCardTypes.QueenOfClubs | PlayingCardTypes.KingOfClubs | PlayingCardTypes.AceOfClubs | PlayingCardTypes.TwoOfDiamonds | PlayingCardTypes.ThreeOfDiamonds | PlayingCardTypes.FourOfDiamonds | PlayingCardTypes.FiveOfDiamonds | PlayingCardTypes.SixOfDiamonds | PlayingCardTypes.SevenOfDiamonds | PlayingCardTypes.EightOfDiamonds | PlayingCardTypes.NineOfDiamonds | PlayingCardTypes.TenOfDiamonds | PlayingCardTypes.JackOfDiamonds | PlayingCardTypes.QueenOfDiamonds | PlayingCardTypes.KingOfDiamonds | PlayingCardTypes.AceOfDiamonds | PlayingCardTypes.TwoOfHearts | PlayingCardTypes.ThreeOfHearts | PlayingCardTypes.FourOfHearts | PlayingCardTypes.FiveOfHearts | PlayingCardTypes.SixOfHearts | PlayingCardTypes.SevenOfHearts | PlayingCardTypes.EightOfHearts | PlayingCardTypes.NineOfHearts | PlayingCardTypes.TenOfHearts | PlayingCardTypes.JackOfHearts | PlayingCardTypes.QueenOfHearts | PlayingCardTypes.KingOfHearts | PlayingCardTypes.AceOfHearts | PlayingCardTypes.TwoOfSpades | PlayingCardTypes.ThreeOfSpades | PlayingCardTypes.FourOfSpades | PlayingCardTypes.FiveOfSpades | PlayingCardTypes.SixOfSpades | PlayingCardTypes.SevenOfSpades | PlayingCardTypes.EightOfSpades | PlayingCardTypes.NineOfSpades | PlayingCardTypes.TenOfSpades | PlayingCardTypes.JackOfSpades | PlayingCardTypes.QueenOfSpades | PlayingCardTypes.KingOfSpades | PlayingCardTypes.AceOfSpades))
		{
			return this;
		}
		if (!(other is Wild wild))
		{
			return null;
		}
		if (_filter.Contains(wild._filter) && EnumUtil.HasFlags64(_wilds, wild._wilds))
		{
			return this;
		}
		if (wild._filter.Contains(_filter) && EnumUtil.HasFlags64(wild._wilds, _wilds))
		{
			return wild;
		}
		return null;
	}
}
