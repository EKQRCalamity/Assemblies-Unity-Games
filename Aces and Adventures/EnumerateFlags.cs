using System;
using System.Collections.Generic;

public struct EnumerateFlags<T> where T : struct, IConvertible
{
	public struct Enumerator
	{
		private readonly T[] _enumValues;

		private readonly int _flags;

		private int _current;

		private sbyte _count;

		public T Current => CastTo<T>.From(_current);

		public Enumerator(T[] enumValues, int flags)
		{
			_enumValues = enumValues;
			_flags = flags;
			_current = 0;
			_count = -1;
		}

		public bool MoveNext()
		{
			bool flag;
			do
			{
				_current += _current;
				_current = ((_current > 0) ? _current : CastTo<int>.From(EnumUtil<T>.Min));
				_count++;
				flag = _count < _enumValues.Length;
			}
			while (flag && (_flags & _current) == 0);
			return flag;
		}
	}

	private readonly T[] _enumValues;

	private readonly int _flags;

	public EnumerateFlags(T flags)
	{
		_enumValues = EnumUtil<T>.Values;
		_flags = CastTo<int>.From(flags);
	}

	public IEnumerable<T> Flags()
	{
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(_enumValues, _flags);
	}
}
