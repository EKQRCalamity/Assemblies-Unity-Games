using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class ModifiableFlags<T> where T : struct, IConvertible
{
	[ProtoContract]
	public struct Modification : IEquatable<Modification>
	{
		[ProtoMember(1)]
		public readonly EnumFlagFunction function;

		[ProtoMember(2)]
		public readonly T flags;

		public Modification(EnumFlagFunction function, T flags)
		{
			this.function = function;
			this.flags = flags;
		}

		public bool Equals(Modification other)
		{
			if (function == other.function)
			{
				return EnumUtil.Equals(flags, other.flags);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Modification other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((int)function * 397) ^ CastTo<int>.From(flags);
		}
	}

	[ProtoMember(1, OverwriteList = true)]
	private List<Modification> _modifications;

	private T? _cachedValue;

	private List<Modification> modifications => _modifications ?? (_modifications = new List<Modification>());

	public T value
	{
		get
		{
			T valueOrDefault = _cachedValue.GetValueOrDefault();
			if (!_cachedValue.HasValue)
			{
				valueOrDefault = _CalculateValue();
				_cachedValue = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	private ModifiableFlags()
	{
	}

	public ModifiableFlags(T? defaultValue = null)
	{
		if (defaultValue.HasValue)
		{
			modifications.Add(new Modification(EnumFlagFunction.Add, defaultValue.Value));
		}
	}

	private T _CalculateValue()
	{
		using PoolKeepItemDictionaryHandle<T, int> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<T, int>();
		foreach (Modification modification in modifications)
		{
			EnumerateFlags<T>.Enumerator enumerator2 = EnumUtil<T>.Flags(modification.flags).GetEnumerator();
			while (enumerator2.MoveNext())
			{
				T current2 = enumerator2.Current;
				poolKeepItemDictionaryHandle.value.Increment(current2, (modification.function == EnumFlagFunction.Add) ? 1 : (-1));
			}
		}
		T val = EnumUtil<T>.NoFlags;
		foreach (KeyValuePair<T, int> item in poolKeepItemDictionaryHandle.value)
		{
			if (item.Value > 0)
			{
				val = EnumUtil.Add(val, item.Key);
			}
		}
		return val;
	}

	public Modification AddModification(EnumFlagFunction function, T flags)
	{
		_cachedValue = null;
		return modifications.AddReturn(new Modification(function, flags));
	}

	public void RemoveModification(EnumFlagFunction function, T flags)
	{
		if (modifications.Remove(new Modification(function, flags)))
		{
			_cachedValue = null;
		}
	}

	public static implicit operator T(ModifiableFlags<T> flags)
	{
		return flags.value;
	}
}
