using System;
using System.Collections.Generic;

public struct EnumerateFlagCombinations<T> where T : struct, IConvertible
{
	public struct Enumerator
	{
		private readonly T[] _enumValues;

		private int _current;

		private readonly int _count;

		public T Current => CastTo<T>.From(_current);

		public Enumerator(bool includeNone)
		{
			_enumValues = EnumUtil<T>.Values;
			_current = (includeNone ? (-1) : 0);
			_count = 1 << _enumValues.Length;
		}

		public bool MoveNext()
		{
			_current++;
			return _current < _count;
		}
	}

	private readonly bool _includeNone;

	public EnumerateFlagCombinations(bool includeNone)
	{
		_includeNone = includeNone;
	}

	public IEnumerable<T> Combinations()
	{
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(_includeNone);
	}
}
