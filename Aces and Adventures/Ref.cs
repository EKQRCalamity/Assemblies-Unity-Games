using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField]
public class Ref<T> : IEquatable<Ref<T>>
{
	[ProtoMember(1)]
	[UIField]
	public T value;

	private Ref()
	{
	}

	public Ref(T value)
	{
		this.value = value;
	}

	public override string ToString()
	{
		object obj = value?.ToString();
		if (obj == null)
		{
			obj = "Null Ref<T>";
		}
		return (string)obj;
	}

	public bool Equals(Ref<T> other)
	{
		if (other != null)
		{
			return EqualityComparer<T>.Default.Equals(other.value, value);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is Ref<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return value?.GetHashCode() ?? int.MinValue;
	}

	public static implicit operator T(Ref<T> reference)
	{
		return reference.value;
	}
}
