using System;
using System.Collections.Generic;
using System.Text;

public static class BitMask
{
	public const int NOTHING = 0;

	public const int EVERYTHING = -1;

	public static readonly int First4Bits = 15;

	public static readonly int First8Bits = 255;

	public static readonly int First12Bits = 4095;

	public static readonly int First16Bits = 65535;

	public static readonly int First24Bits = 16777215;

	public static readonly int Last16Bits = ~First16Bits;

	private static readonly Dictionary<Type, int> _EnumFlags = new Dictionary<Type, int>();

	private static readonly Dictionary<Type, string[]> _EnumToStringCache = new Dictionary<Type, string[]>();

	public static bool All<T>(T bitmask, T flagsToCheck) where T : struct, IConvertible
	{
		int num = (int)(object)flagsToCheck;
		return ((int)(object)bitmask & num) == num;
	}

	public static bool Any<T>(T bitmask, T flagsToCheck) where T : struct, IConvertible
	{
		return ((int)(object)bitmask & (int)(object)flagsToCheck) != 0;
	}

	public static bool Only<T>(T bitmask, T flagsToCheck) where T : struct, IConvertible
	{
		int num = (int)(object)bitmask;
		if (num == 0)
		{
			return false;
		}
		num &= _GetAllEnumFlags(typeof(T));
		return (num ^ (int)(object)flagsToCheck) == 0;
	}

	public static bool None<T>(T bitmask) where T : struct, IConvertible
	{
		int num = (int)(object)bitmask;
		if (num == 0)
		{
			return true;
		}
		return (num & _GetAllEnumFlags(typeof(T))) == 0;
	}

	public static bool AllExcept<T>(T bitmask, T flagsToCheck) where T : struct, IConvertible
	{
		int num = (int)(object)bitmask;
		if (num == 0)
		{
			return false;
		}
		int num2 = _GetAllEnumFlags(typeof(T));
		num &= num2;
		return (num ^ (int)(object)flagsToCheck) == num2;
	}

	public static T Add<T>(T bitmask, T flagsToAdd) where T : struct, IConvertible
	{
		return (T)(object)((int)(object)bitmask | (int)(object)flagsToAdd);
	}

	public static T Subtract<T>(T bitmask, T flagsToSubtract) where T : struct, IConvertible
	{
		return (T)(object)((int)(object)bitmask & ~(int)(object)flagsToSubtract);
	}

	public static bool IsEverything<T>(T bitmask) where T : struct, IConvertible
	{
		return (int)(object)bitmask == -1;
	}

	public static bool IsNothing<T>(T bitmask) where T : struct, IConvertible
	{
		return (int)(object)bitmask == 0;
	}

	public static T Clamp<T>(T bitmask) where T : struct, IConvertible
	{
		return (T)(object)((int)(object)bitmask & _GetAllEnumFlags(typeof(T)));
	}

	public static T ToFlag<T>(int nonFlagEnumValue) where T : struct, IConvertible
	{
		return (T)(object)(int)Math.Pow(2.0, nonFlagEnumValue);
	}

	public static string ToString(Type enumType, object bitmask)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = (int)Convert.ChangeType(bitmask, typeof(int));
		string[] array = _GetCachedEnumStrings(enumType);
		int num2 = (int)Math.Log((int)Convert.ChangeType(Enum.Parse(enumType, array[0]), typeof(int)), 2.0);
		int num3 = array.Length;
		for (int i = 0; i < num3; i++)
		{
			int num4 = 1 << i + num2;
			if ((num & num4) != 0)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(array[i]);
			}
		}
		if (stringBuilder.Length == 0)
		{
			stringBuilder.Append("0");
		}
		return stringBuilder.ToString();
	}

	public static ulong Rotate(ulong bitMask, int rotation)
	{
		rotation = ((rotation >= 0) ? (rotation % 64) : (64 + rotation % 64));
		return (bitMask << rotation) | (bitMask >> 64 - rotation);
	}

	public static uint Rotate(uint bitMask, int rotation)
	{
		rotation = ((rotation >= 0) ? (rotation % 32) : (32 + rotation % 32));
		return (bitMask << rotation) | (bitMask >> 32 - rotation);
	}

	public static ushort Rotate(ushort bitMask, int rotation)
	{
		rotation = ((rotation >= 0) ? (rotation % 16) : (16 + rotation % 16));
		return (ushort)((bitMask << rotation) | (bitMask >> 16 - rotation));
	}

	public static byte Rotate(byte bitMask, int rotation)
	{
		rotation = ((rotation >= 0) ? (rotation % 8) : (8 + rotation % 8));
		return (byte)((bitMask << rotation) | (bitMask >> 8 - rotation));
	}

	public static int LeadingZeros(int x)
	{
		x |= x >> 1;
		x |= x >> 2;
		x |= x >> 4;
		x |= x >> 8;
		x |= x >> 16;
		x -= (x >> 1) & 0x55555555;
		x = ((x >> 2) & 0x33333333) + (x & 0x33333333);
		x = ((x >> 4) + x) & 0xF0F0F0F;
		x += x >> 8;
		x += x >> 16;
		return 32 - (x & 0x3F);
	}

	public static int TrailingZeros(int x)
	{
		return 32 - (LeadingZeros(x) + 1);
	}

	private static int _AllFlags(int numFlags)
	{
		int num = 0;
		for (int i = 0; i < numFlags; i++)
		{
			num |= 1 << i;
		}
		return num;
	}

	private static int _GetAllEnumFlags(Type type)
	{
		if (!_EnumFlags.ContainsKey(type))
		{
			_EnumFlags.Add(type, _AllFlags(Enum.GetValues(type).Length));
		}
		return _EnumFlags[type];
	}

	private static string[] _GetCachedEnumStrings(Type type)
	{
		if (!_EnumToStringCache.ContainsKey(type))
		{
			_EnumToStringCache.Add(type, Enum.GetNames(type));
		}
		return _EnumToStringCache[type];
	}
}
