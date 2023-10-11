using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField]
[ProtoInclude(10, typeof(WildRange))]
[ProtoInclude(11, typeof(WildSuit))]
[ProtoInclude(12, typeof(Wild))]
[ProtoInclude(13, typeof(WildRemapValue))]
[ProtoInclude(14, typeof(JokerWild))]
public abstract class AWild : IEquatable<AWild>
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide, order = 1u)]
	protected PlayingCard.Filter _filter;

	protected string _filterString
	{
		get
		{
			object s;
			if (!_filter)
			{
				s = "";
			}
			else
			{
				PlayingCard.Filter filter = _filter;
				s = " " + filter.ToString() + " Only";
			}
			return ((string)s).SizeIfNotEmpty();
		}
	}

	protected AWild()
	{
	}

	protected AWild(PlayingCard.Filter? filter)
	{
		_filter = filter.GetValueOrDefault();
	}

	protected abstract void _Process(ref PlayingCardTypes output);

	public virtual IEnumerable<AbilityKeyword> GetKeywords()
	{
		yield break;
	}

	public virtual bool Equals(AWild other)
	{
		if (other != null)
		{
			return other._filter == _filter;
		}
		return false;
	}

	public virtual AWild CombineWith(AWild other)
	{
		return null;
	}

	public void Process(ref PlayingCardTypes cards)
	{
		if (_filter.AreValid(cards))
		{
			_Process(ref cards);
		}
	}

	public sealed override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((AWild)obj);
	}

	public override int GetHashCode()
	{
		return GetType().GetHashCode();
	}
}
