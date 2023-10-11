using System;

public class FlagSum<T> where T : struct, IConvertible
{
	private int[] _flagCounts = new int[32];

	static FlagSum()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new FlagSum<T>(), delegate(FlagSum<T> f)
		{
			f.Clear();
		});
	}

	public int AddFlags(T flags)
	{
		int num = 0;
		EnumerateFlags<T>.Enumerator enumerator = EnumUtil.Flags(flags).GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			num = Math.Max(num, ++_flagCounts[BitMask.TrailingZeros(CastTo<int>.From(current))]);
		}
		return num;
	}

	public void Clear()
	{
		for (int i = 0; i < _flagCounts.Length; i++)
		{
			_flagCounts[i] = 0;
		}
	}
}
