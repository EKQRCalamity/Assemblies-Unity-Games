using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField]
public class WildRange : AWild
{
	private static readonly RangeInt DEFAULT_RANGE = new RangeInt(-1, 1, -12, 12, 1);

	[ProtoMember(1)]
	[UIField]
	public readonly RangeInt range = DEFAULT_RANGE;

	private bool rangeSpecified => range != DEFAULT_RANGE;

	private WildRange()
	{
	}

	private WildRange(int min, int max)
	{
		range = new RangeInt(min, max);
	}

	protected override void _Process(ref PlayingCardTypes output)
	{
		PlayingCardValues playingCardValues = output.Values();
		PlayingCardValues playingCardValues2 = playingCardValues;
		for (int i = range.min; i <= range.max; i++)
		{
			if (i != 0)
			{
				playingCardValues2 |= EnumUtil.ShiftFlags(playingCardValues, i);
			}
		}
		output = playingCardValues2.ToPlayingCardTypes(output.Suits());
	}

	public override IEnumerable<AbilityKeyword> GetKeywords()
	{
		if (range.min == -range.max)
		{
			yield return AbilityKeyword.WildRange;
		}
	}

	public override string ToString()
	{
		return "Wild Range " + range.ToRangeString(null, "", 50).Trim() + base._filterString;
	}

	public override bool Equals(AWild other)
	{
		if (base.Equals(other) && other is WildRange wildRange)
		{
			return wildRange.range == range;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int hashCode = base.GetHashCode();
		RangeInt rangeInt = range;
		return hashCode ^ rangeInt.GetHashCode();
	}

	public override AWild CombineWith(AWild other)
	{
		if (!(other is WildRange wildRange))
		{
			return null;
		}
		if (!_filter && !wildRange._filter)
		{
			return new WildRange(range.min + wildRange.range.min, range.max + wildRange.range.max);
		}
		return null;
	}
}
