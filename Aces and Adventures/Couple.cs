using System;
using System.Collections.Generic;

public struct Couple<T, V> : IEquatable<Couple<T, V>>, IComparable<Couple<T, V>>
{
	public static IComparer<T> AComparer;

	public static IComparer<V> BComparer;

	public readonly T a;

	public readonly V b;

	static Couple()
	{
		AComparer = ((typeof(T).ImplementsInterface(typeof(IComparable)) || typeof(T).ImplementsInterface(typeof(IComparable<T>))) ? Comparer<T>.Default : null);
		BComparer = ((typeof(V).ImplementsInterface(typeof(IComparable)) || typeof(V).ImplementsInterface(typeof(IComparable<V>))) ? Comparer<V>.Default : null);
	}

	public Couple(T a, V b)
	{
		this.a = a;
		this.b = b;
	}

	public Couple<T, V> SetA(T newA)
	{
		return new Couple<T, V>(newA, b);
	}

	public Couple<T, V> SetB(V newB)
	{
		return new Couple<T, V>(a, newB);
	}

	public static bool operator ==(Couple<T, V> a, Couple<T, V> b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Couple<T, V> a, Couple<T, V> b)
	{
		return !a.Equals(b);
	}

	public static implicit operator T(Couple<T, V> couple)
	{
		return couple.a;
	}

	public static implicit operator V(Couple<T, V> couple)
	{
		return couple.b;
	}

	public static implicit operator KeyValuePair<T, V>(Couple<T, V> couple)
	{
		return new KeyValuePair<T, V>(couple.a, couple.b);
	}

	public static implicit operator Couple<T, V>(KeyValuePair<T, V> pair)
	{
		return new Couple<T, V>(pair.Key, pair.Value);
	}

	public override bool Equals(object obj)
	{
		if (obj is Couple<T, V> couple)
		{
			return couple.Equals(this);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num;
		if (a == null)
		{
			num = 0;
		}
		else
		{
			T val = a;
			num = val.GetHashCode() & BitMask.First16Bits;
		}
		int num2;
		if (b == null)
		{
			num2 = 0;
		}
		else
		{
			V val2 = b;
			num2 = val2.GetHashCode() & BitMask.First16Bits;
		}
		return num ^ (num2 << 16);
	}

	public override string ToString()
	{
		return $"({a}, {b})";
	}

	public bool Equals(Couple<T, V> other)
	{
		if (EqualityComparer<T>.Default.Equals(a, other.a))
		{
			return EqualityComparer<V>.Default.Equals(b, other.b);
		}
		return false;
	}

	public int CompareTo(Couple<T, V> other)
	{
		int num = ((AComparer != null) ? AComparer.Compare(a, other.a) : 0);
		if (num == 0)
		{
			if (BComparer == null)
			{
				return 0;
			}
			return BComparer.Compare(b, other.b);
		}
		return num;
	}
}
