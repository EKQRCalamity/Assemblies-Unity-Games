using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public struct HotKey : IEquatable<HotKey>, IComparable<HotKey>
{
	[ProtoMember(1)]
	[UIField]
	public readonly KeyModifiers modifiers;

	[ProtoMember(2)]
	[UIField]
	public readonly KeyCode key;

	public HotKey(KeyCode key)
		: this((KeyModifiers)0, key)
	{
	}

	public HotKey(KeyModifiers modifiers, KeyCode key)
	{
		this.modifiers = modifiers;
		this.key = key;
	}

	public static implicit operator KeyModifiers(HotKey hotKey)
	{
		return hotKey.modifiers;
	}

	public static implicit operator KeyCode(HotKey hotKey)
	{
		return hotKey.key;
	}

	public bool Equals(HotKey other)
	{
		if (key == other.key)
		{
			return modifiers == other.modifiers;
		}
		return false;
	}

	public int CompareTo(HotKey other)
	{
		int num = EnumUtil.FlagCount(other.modifiers).CompareTo(EnumUtil.FlagCount(modifiers));
		if (num != 0)
		{
			return num;
		}
		int num2 = modifiers - other.modifiers;
		if (num2 != 0)
		{
			return num2;
		}
		return key - other.key;
	}

	public override bool Equals(object obj)
	{
		if (obj is HotKey)
		{
			return Equals((HotKey)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)key ^ ((int)modifiers << 16);
	}

	public override string ToString()
	{
		string text = modifiers.GetText();
		if (!text.IsNullOrEmpty())
		{
			return text + "+" + EnumUtil.FriendlyName(key);
		}
		return EnumUtil.FriendlyName(key);
	}
}
