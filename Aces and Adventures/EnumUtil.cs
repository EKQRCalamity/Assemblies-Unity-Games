using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public static class EnumUtil
{
	public const int AggressiveInlining = 256;

	private static readonly Dictionary<Type, string[]> _EnumNames = new Dictionary<Type, string[]>();

	public static bool HasPrevious<T>(T value) where T : struct, IConvertible
	{
		return EnumUtil<T>.HasPrevious(value);
	}

	public static void Previous<T>(ref T value) where T : struct, IConvertible
	{
		value = EnumUtil<T>.Previous(value);
	}

	public static T Previous<T>(T value) where T : struct, IConvertible
	{
		return EnumUtil<T>.Previous(value);
	}

	public static void Previous<T>(ref T? value) where T : struct, IConvertible
	{
		if (value.HasValue)
		{
			T val = EnumUtil<T>.Previous(value.Value);
			value = ((CastTo<int>.From(val) != CastTo<int>.From(value.Value)) ? new T?(val) : null);
		}
	}

	public static bool HasNext<T>(T value) where T : struct, IConvertible
	{
		return EnumUtil<T>.HasNext(value);
	}

	public static void Next<T>(ref T value) where T : struct, IConvertible
	{
		value = EnumUtil<T>.Next(value);
	}

	public static void Adjust<T>(ref T value, int valueAdjustment) where T : struct, IConvertible
	{
		value = EnumUtil<T>.Adjust(value, valueAdjustment);
	}

	public static T Adjust<T>(T value, int valueAdjustment) where T : struct, IConvertible
	{
		return EnumUtil<T>.Adjust(value, valueAdjustment);
	}

	public static bool HasNext<T>(T? value) where T : struct, IConvertible
	{
		if (value.HasValue)
		{
			return EnumUtil<T>.HasNext(value.Value);
		}
		return false;
	}

	public static void Next<T>(ref T? value) where T : struct, IConvertible
	{
		if (value.HasValue)
		{
			T val = EnumUtil<T>.Next(value.Value);
			value = ((CastTo<int>.From(val) != CastTo<int>.From(value.Value)) ? new T?(val) : null);
		}
	}

	public static T Next<T>(T value) where T : struct, IConvertible
	{
		return EnumUtil<T>.Next(value);
	}

	public static T NextWrap<T>(T value) where T : struct, IConvertible
	{
		return EnumUtil<T>.NextWrap(value);
	}

	public static IEnumerable<T> EnumerateOutwardFrom<T>(T? value) where T : struct, IConvertible
	{
		return EnumUtil<T>.EnumerateOutwardFrom(value);
	}

	public static IEnumerable<T> EnumerateDescending<T>(T value) where T : struct, IConvertible
	{
		return EnumUtil<T>.EnumerateDescending(value);
	}

	public static T RandomFlag<T>(System.Random random, T flags) where T : struct, IConvertible
	{
		return EnumUtil<T>.RandomFlag(random, flags);
	}

	public static C RandomFlagConvert<T, C>(System.Random random, T flags) where T : struct, IConvertible where C : struct, IConvertible
	{
		return EnumUtil<T>.ConvertFromFlag<C>(RandomFlag(random, flags));
	}

	public static T Validate<T>(T value) where T : struct, IConvertible
	{
		return EnumUtil<T>.Validate(value);
	}

	public static void Validate<T>(ref T value) where T : struct, IConvertible
	{
		EnumUtil<T>.Validate(ref value);
	}

	public static void Validate<T>(ref T? value) where T : struct, IConvertible
	{
		if (value.HasValue)
		{
			value = Validate(value.Value);
		}
	}

	public static List<T> ValidateList<T>(List<T> list, bool removeInvalid = true) where T : struct, IConvertible
	{
		return EnumUtil<T>.ValidateList(list, removeInvalid);
	}

	public static void ValidateKeys<T, V>(ref Dictionary<T, V> dictionary) where T : struct, IConvertible
	{
		EnumUtil<T>.ValidateKeys(ref dictionary);
	}

	public static void ValidateValues<K, T>(ref Dictionary<K, T> dictionary) where T : struct, IConvertible
	{
		EnumUtil<T>.ValidateValues(ref dictionary);
	}

	public static void ValidatePairs<K, V>(ref Dictionary<K, V> dictionary) where K : struct, IConvertible where V : struct, IConvertible
	{
		EnumUtil<K>.ValidateKeys(ref dictionary);
		EnumUtil<V>.ValidateValues(ref dictionary);
	}

	public static bool IsValid<T>(T value) where T : struct, IConvertible
	{
		return EnumUtil<T>.IsValid(value);
	}

	public static bool IsInvalid<T>(T value) where T : struct, IConvertible
	{
		return EnumUtil<T>.IsInvalid(value);
	}

	public static string Name<T>(T value) where T : struct, IConvertible
	{
		return EnumUtil<T>.Name(value);
	}

	public static string Name<T>(T? value) where T : struct, IConvertible
	{
		if (!value.HasValue)
		{
			return "";
		}
		return EnumUtil<T>.Name(value.Value);
	}

	public static string FriendlyName<T>(T value, bool uppercase = true) where T : struct, IConvertible
	{
		return EnumUtil<T>.FriendlyName(value, uppercase);
	}

	public static string FriendlyName<T>(T? value, bool uppercase = true) where T : struct, IConvertible
	{
		if (!value.HasValue)
		{
			return "";
		}
		return EnumUtil<T>.FriendlyName(value.Value, uppercase);
	}

	public static string FriendlyNameSpaced<T>(T? value, bool uppercase = true) where T : struct, IConvertible
	{
		if (!value.HasValue)
		{
			return "";
		}
		return " " + EnumUtil<T>.FriendlyName(value.Value, uppercase);
	}

	public static string FriendlyNameSpacedAfter<T>(T? value, bool uppercase = true) where T : struct, IConvertible
	{
		if (!value.HasValue)
		{
			return "";
		}
		return EnumUtil<T>.FriendlyName(value.Value, uppercase) + " ";
	}

	public static string GetTooltip<T>(T value) where T : struct, IConvertible
	{
		return EnumUtil<T>.GetTooltip(value);
	}

	public static string ToRangeString<T>(T min, T max, string noun = "", byte nounSizePercent = 50, T? minRange = null, T? maxRange = null) where T : struct, IConvertible
	{
		return EnumUtil<T>.ToRangeString(min, max, noun, nounSizePercent, minRange, maxRange);
	}

	public static string FriendlyNameFlagRanges<T>(T flags, bool uppercase = true) where T : struct, IConvertible
	{
		return EnumUtil<T>.FriendlyNameFlagRanges(flags, uppercase);
	}

	public static T Add<T>(ref T a, T b) where T : struct, IConvertible
	{
		return a = EnumUtil<T>.Add(a, b);
	}

	public static T Add<T>(T a, T b) where T : struct, IConvertible
	{
		return EnumUtil<T>.Add(a, b);
	}

	public static T AddSmart<T>(T a, T b) where T : struct, IConvertible
	{
		return EnumUtil<T>.AddSmart(a, b);
	}

	public static void Subtract<T>(ref T value, T subtract) where T : struct, IConvertible
	{
		value = EnumUtil<T>.Subtract(value, subtract);
	}

	public static T Subtract<T>(T value, T subtract) where T : struct, IConvertible
	{
		return EnumUtil<T>.Subtract(value, subtract);
	}

	public static T SubtractSmart<T>(T value, T subtract) where T : struct, IConvertible
	{
		return EnumUtil<T>.SubtractSmart(value, subtract);
	}

	public static T SetFlag<T>(T value, T flagToSet, bool isOn) where T : struct, IConvertible
	{
		return EnumUtil<T>.SetFlag(value, flagToSet, isOn);
	}

	public static T SetFlag<T>(ref T value, T flagToSet, bool isOn) where T : struct, IConvertible
	{
		return value = EnumUtil<T>.SetFlag(value, flagToSet, isOn);
	}

	public static T ToggleFlag<T>(T value, T flagToToggle) where T : struct, IConvertible
	{
		return EnumUtil<T>.ToggleFlag(value, flagToToggle);
	}

	public static T Minimum<T>(T a, T b) where T : struct, IConvertible
	{
		return EnumUtil<T>.Minimum(a, b);
	}

	public static T? Minimum<T>(T? a, T? b) where T : struct, IConvertible
	{
		return EnumUtil<T>.Minimum(a, b);
	}

	public static T Maximum<T>(T a, T b) where T : struct, IConvertible
	{
		return EnumUtil<T>.Maximum(a, b);
	}

	public static T? Maximum<T>(T? a, T? b) where T : struct, IConvertible
	{
		return EnumUtil<T>.Maximum(a, b);
	}

	public static T Clamp<T>(T value, T min, T max) where T : struct, IConvertible
	{
		return EnumUtil<T>.Clamp(value, min, max);
	}

	public static T Clamp<T>(T value) where T : struct, IConvertible
	{
		return EnumUtil<T>.Clamp(value);
	}

	public static bool Equals<T>(T a, T b) where T : struct, IConvertible
	{
		return EnumUtil<T>.Equals(a, b);
	}

	public static bool Equals<T>(T? a, T? b) where T : struct, IConvertible
	{
		return EnumUtil<T>.Equals(a, b);
	}

	public static bool InRange<T>(T value, T min, T max) where T : struct, IConvertible
	{
		return EnumUtil<T>.InRange(value, min, max);
	}

	public static bool HasFlag<T>(T value, T flag) where T : struct, IConvertible
	{
		return EnumUtil<T>.HasFlag(value, flag);
	}

	public static bool HasFlagConvert<F, N>(F flags, N nonFlag) where F : struct, IConvertible where N : struct, IConvertible
	{
		return HasFlag(flags, CastTo<F>.From(1 << CastTo<int>.From(nonFlag)));
	}

	public static bool HasFlagConvert<F, N>(F flags, N? nonFlag) where F : struct, IConvertible where N : struct, IConvertible
	{
		if (nonFlag.HasValue)
		{
			return HasFlagConvert(flags, nonFlag.Value);
		}
		return false;
	}

	public static bool HasFlags<T>(T value, T flags) where T : struct, IConvertible
	{
		return EnumUtil<T>.HasFlags(value, flags);
	}

	public static bool FlagsIntersect<T>(T value, T flags) where T : struct, IConvertible
	{
		return EnumUtil<T>.FlagsIntersect(value, flags);
	}

	public static bool HasFlags64<T>(T value, T flags) where T : struct, IConvertible
	{
		return EnumUtil<T>.HasFlags64(value, flags);
	}

	public static bool HasAllFlags<T>(T value) where T : struct, IConvertible
	{
		return EnumUtil<T>.HasAllFlags(value);
	}

	public static uint FlagCount<T>(T flags) where T : struct, IConvertible
	{
		return EnumUtil<T>.FlagCount(flags);
	}

	public static uint FlagCount<T>(T? flags) where T : struct, IConvertible
	{
		if (!flags.HasValue)
		{
			return 0u;
		}
		return FlagCount(flags.Value);
	}

	public static EnumerateFlags<T> Flags<T>(T flags) where T : struct, IConvertible
	{
		return EnumUtil<T>.Flags(flags);
	}

	public static PoolKeepItemListHandle<C> FlagsConverted<T, C>(T flags) where T : struct, IConvertible where C : struct, IConvertible
	{
		PoolKeepItemListHandle<C> poolKeepItemListHandle = Pools.UseKeepItemList<C>();
		EnumerateFlags<T>.Enumerator enumerator = Flags(flags).GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			poolKeepItemListHandle.Add(EnumUtil<T>.ConvertFromFlag<C>(current));
		}
		return poolKeepItemListHandle;
	}

	public static T MinActiveFlag<T>(T flags) where T : struct, IConvertible
	{
		return EnumUtil<T>.MinActiveFlag(flags);
	}

	public static T MaxActiveFlag<T>(T flags) where T : struct, IConvertible
	{
		return EnumUtil<T>.MaxActiveFlag(flags);
	}

	public static T AllFlagsExcept<T>(T flagsToExclude) where T : struct, IConvertible
	{
		return EnumUtil<T>.AllFlagsExcept(flagsToExclude);
	}

	public static T ShiftFlags<T>(T flags, int shift) where T : struct, IConvertible
	{
		return EnumUtil<T>.ShiftFlags(flags, shift);
	}

	public static T[] ToArray<T>(T value) where T : struct, IConvertible
	{
		return EnumUtil<T>.ToArray(value);
	}

	public static T[] ToArray<T>(T a, T b) where T : struct, IConvertible
	{
		return EnumUtil<T>.ToArray(a, b);
	}

	public static GameObject GetResourceBlueprint<T>(T value) where T : struct, IConvertible
	{
		return EnumUtil<T>.GetResourceBlueprint(value);
	}

	public static string GetCategory<T>(T value) where T : struct, IConvertible
	{
		return EnumUtil<T>.GetCategory(value);
	}

	public static C GetCategorySafe<T, C>(T value) where T : struct, IConvertible where C : struct, IConvertible
	{
		return EnumUtil<T>.GetCategory<C>(value) ?? EnumUtil<C>.Min;
	}

	public static Dictionary<string, string> GetDefaultValueStringDictionary<T>(T defaultValue) where T : struct, IConvertible
	{
		int defaultInt = CastTo<int>.From(defaultValue);
		return EnumUtil<T>.Values.Select((T r) => new KeyValuePair<string, string>(r.ToString(), (CastTo<int>.From(r) == defaultInt) ? "" : null)).ToDictionary((KeyValuePair<string, string> p) => p.Key, (KeyValuePair<string, string> p) => p.Value);
	}

	private static string[] _SortEnumNames(Type enumType, UISortEnumAttribute sort)
	{
		return _SortEnumNames(sort?.type, enumType, Enum.GetNames(enumType));
	}

	private static string[] _SortEnumNames(UISortEnumType? sort, Type enumType, string[] names)
	{
		return sort switch
		{
			UISortEnumType.Alphabetical => names.OrderBy((string name) => name).ToArray(), 
			UISortEnumType.Value => names.OrderBy((string name) => Enum.Parse(enumType, name)).ToArray(), 
			_ => names, 
		};
	}

	public static string[] GetNames(Type enumType)
	{
		if (!_EnumNames.ContainsKey(enumType))
		{
			return _EnumNames[enumType] = _SortEnumNames(enumType, enumType.GetAttribute<UISortEnumAttribute>());
		}
		return _EnumNames[enumType];
	}
}
public static class EnumUtil<T> where T : struct, IConvertible
{
	public class EnumEqualityComparer : IEqualityComparer<T>
	{
		public bool Equals(T x, T y)
		{
			return CastTo<int>.From(x) == CastTo<int>.From(y);
		}

		public int GetHashCode(T obj)
		{
			return CastTo<int>.From(obj);
		}
	}

	private static bool? _IsFlagsEnum;

	private static int? _sizeInBits;

	private static T[] _Values;

	private static HashSet<T> _ValuesHash;

	private static Dictionary<string, List<T>> _ValuesByCategory;

	private static Dictionary<int, string> _CategoryByValue;

	private static Dictionary<int, string> _CategoryPascalByValue;

	private static Dictionary<int, string> _Names;

	private static Dictionary<int, string> _FriendlyNames;

	private static Dictionary<int, string> _FriendlyNamesLowercase;

	private static Dictionary<int, T[]> _Arrays;

	private static EnumEqualityComparer _equalityComparer;

	private static string _ResourcePath;

	private static bool _ResourceCategoriesAreSubFolders;

	private static Dictionary<int, GameObject> _ResourceMap;

	private static Dictionary<string, T> _ValuesByName;

	private static Dictionary<string, T> _DefaultValues;

	private static Dictionary<int, string> _Tooltips;

	private static T? _AllFlags;

	public static bool IsFlagsEnum
	{
		get
		{
			bool? isFlagsEnum = _IsFlagsEnum;
			if (!isFlagsEnum.HasValue)
			{
				bool? flag = (_IsFlagsEnum = typeof(T).HasAttribute<FlagsAttribute>());
				return flag.Value;
			}
			return isFlagsEnum.GetValueOrDefault();
		}
	}

	public static int SizeInBits
	{
		get
		{
			int? sizeInBits = _sizeInBits;
			if (!sizeInBits.HasValue)
			{
				int? num = (_sizeInBits = SizeInBytes.Get<T>() * 8);
				return num.Value;
			}
			return sizeInBits.GetValueOrDefault();
		}
	}

	public static T[] Values => _Values ?? (_Values = Enum.GetValues(typeof(T)).Cast<T>().ToArray());

	public static HashSet<T> ValuesHash => _ValuesHash ?? (_ValuesHash = Values.ToHash());

	public static Dictionary<string, List<T>> ValuesByCategory
	{
		get
		{
			if (_ValuesByCategory == null)
			{
				_ValuesByCategory = new Dictionary<string, List<T>>();
				T[] values = Values;
				foreach (T val in values)
				{
					UICategoryAttribute attribute = (val as Enum).GetAttribute<UICategoryAttribute>();
					string key = ((attribute != null) ? attribute.category : "Miscellaneous");
					if (!_ValuesByCategory.ContainsKey(key))
					{
						_ValuesByCategory.Add(key, new List<T>());
					}
					_ValuesByCategory[key].Add(val);
				}
			}
			return _ValuesByCategory;
		}
	}

	private static Dictionary<int, string> CategoryByValue => _CategoryByValue ?? (_CategoryByValue = ValuesByCategory.SelectMany((KeyValuePair<string, List<T>> pair) => pair.Value.Select((T v) => new Couple<int, string>(CastTo<int>.From(v), pair.Key))).ToDictionary((Couple<int, string> c) => c.a, (Couple<int, string> c) => c.b));

	private static Dictionary<int, string> CategoryPascalByValue => _CategoryPascalByValue ?? (_CategoryPascalByValue = ValuesByCategory.SelectMany((KeyValuePair<string, List<T>> pair) => pair.Value.Select((T v) => new Couple<int, string>(CastTo<int>.From(v), pair.Key.PascalCaseFromFriendly()))).ToDictionary((Couple<int, string> c) => c.a, (Couple<int, string> c) => c.b));

	private static Dictionary<int, string> Names => _Names ?? (_Names = Values.ToDictionary(CastTo<int>.From, (T v) => v.ToString()));

	private static Dictionary<int, string> FriendlyNames => _FriendlyNames ?? (_FriendlyNames = Values.ToDictionarySafeFast(CastTo<int>.From, (T v) => v.ToString().FriendlyFromCamelOrPascalCase()));

	private static Dictionary<int, string> FriendlyNamesLowercase => _FriendlyNamesLowercase ?? (_FriendlyNamesLowercase = Values.ToDictionarySafeFast(CastTo<int>.From, (T v) => v.ToString().FriendlyFromCamelOrPascalCase().ToLower()));

	private static Dictionary<int, T[]> Arrays => _Arrays ?? (_Arrays = new Dictionary<int, T[]>());

	public static EnumEqualityComparer equalityComparer => _equalityComparer ?? (_equalityComparer = new EnumEqualityComparer());

	private static Dictionary<int, GameObject> ResourceMap
	{
		get
		{
			if (_ResourceMap == null)
			{
				_ResourceMap = new Dictionary<int, GameObject>();
				ResourceEnumAttribute attribute = typeof(T).GetAttribute<ResourceEnumAttribute>();
				_ResourcePath = attribute.resourcePath;
				_ResourceCategoriesAreSubFolders = attribute.categoriesAreSubFolders;
			}
			return _ResourceMap;
		}
	}

	private static Dictionary<string, T> ValuesByName => _ValuesByName ?? (_ValuesByName = Values.ToDictionary((T v) => v.ToString(), (T v) => v));

	private static Dictionary<string, T> DefaultValues
	{
		get
		{
			if (_DefaultValues == null)
			{
				_DefaultValues = new Dictionary<string, T>();
				T[] values = Values;
				foreach (T val in values)
				{
					DefaultEnumValueAttribute attribute = (val as Enum).GetAttribute<DefaultEnumValueAttribute>();
					if (attribute != null && !_DefaultValues.ContainsKey(attribute.category))
					{
						_DefaultValues.Add(attribute.category, val);
					}
				}
			}
			return _DefaultValues;
		}
	}

	private static Dictionary<int, string> Tooltips => _Tooltips ?? (_Tooltips = Values.ToDictionary(CastTo<int>.From, (T v) => (v as Enum).GetAttribute<UITooltipAttribute>().GetTooltip()));

	public static T NoFlags => CastTo<T>.From(0);

	public static T AllFlags
	{
		get
		{
			T? allFlags = _AllFlags;
			if (!allFlags.HasValue)
			{
				T? val = (_AllFlags = CastTo<T>.From(Values.Aggregate(0L, (long current, T value) => current | CastTo<long>.From(value))));
				return val.Value;
			}
			return allFlags.GetValueOrDefault();
		}
	}

	public static T Min => Values[0];

	public static T Mid => Values[Values.Length / 2];

	public static T Max => Values[Values.Length - 1];

	public static void Warmup()
	{
		CastTo<T>.From(CastTo<int>.From(Min));
		CastTo<long>.From(Min);
		CastTo<uint>.From(Min);
	}

	public static T AllFlagsExcept(T flagsToExclude)
	{
		return Subtract(AllFlags, flagsToExclude);
	}

	public static T AllFlagsGreaterThanOrEqualToConvert<C>(C correspondingNonFlag) where C : struct, IConvertible
	{
		return CastTo<T>.From(~((1 << CastTo<int>.From(correspondingNonFlag)) - 1));
	}

	public static uint FlagCount(T flags)
	{
		uint num = CastTo<uint>.From(flags);
		num -= (num >> 1) & 0x55555555;
		num = (num & 0x33333333) + ((num >> 2) & 0x33333333);
		return ((num + (num >> 4)) & 0xF0F0F0F) * 16843009 >> 24;
	}

	public static bool HasFlag(T value, T flag)
	{
		return (CastTo<int>.From(value) & CastTo<int>.From(flag)) != 0;
	}

	public static bool HasFlags(T value, T flags)
	{
		int num = CastTo<int>.From(flags);
		return (CastTo<int>.From(value) & num) == num;
	}

	public static bool FlagsIntersect(T value, T flags)
	{
		return (CastTo<int>.From(value) & CastTo<int>.From(flags)) != 0;
	}

	public static bool HasFlags64(T value, T flags)
	{
		long num = CastTo<long>.From(flags);
		return (CastTo<long>.From(value) & num) == num;
	}

	public static bool HasAllFlags(T value)
	{
		return HasFlags(value, AllFlags);
	}

	public static T Add(T a, T b)
	{
		return CastTo<T>.From(CastTo<int>.From(a) | CastTo<int>.From(b));
	}

	public static T AddSmart(T a, T b)
	{
		if (!IsFlagsEnum)
		{
			return CastTo<T>.From(CastTo<int>.From(a) + CastTo<int>.From(b));
		}
		return Add(a, b);
	}

	public static T Subtract(T value, T subtract)
	{
		return CastTo<T>.From(CastTo<int>.From(value) & ~CastTo<int>.From(subtract));
	}

	public static T SubtractSmart(T value, T subtract)
	{
		if (!IsFlagsEnum)
		{
			return CastTo<T>.From(CastTo<int>.From(value) - CastTo<int>.From(subtract));
		}
		return Subtract(value, subtract);
	}

	public static T SetFlag(T value, T flagToSet, bool isOn)
	{
		if (!isOn)
		{
			return Subtract(value, flagToSet);
		}
		return Add(value, flagToSet);
	}

	public static T ToggleFlag(T value, T flagToToggle)
	{
		return CastTo<T>.From(CastTo<int>.From(value) ^ CastTo<int>.From(flagToToggle));
	}

	public static T Minimum(T a, T b)
	{
		return CastTo<T>.From(Math.Min(CastTo<int>.From(a), CastTo<int>.From(b)));
	}

	public static T? Minimum(T? a, T? b)
	{
		if (!a.HasValue || !b.HasValue)
		{
			return a ?? b;
		}
		return Minimum(a.Value, b.Value);
	}

	public static T Maximum(T a, T b)
	{
		return CastTo<T>.From(Math.Max(CastTo<int>.From(a), CastTo<int>.From(b)));
	}

	public static T? Maximum(T? a, T? b)
	{
		if (!a.HasValue || !b.HasValue)
		{
			return a ?? b;
		}
		return CastTo<T>.From(Math.Max(CastTo<int>.From(a.Value), CastTo<int>.From(b.Value)));
	}

	public static T Clamp(T value, T min, T max)
	{
		return CastTo<T>.From(Mathf.Clamp(CastTo<int>.From(value), CastTo<int>.From(min), CastTo<int>.From(max)));
	}

	public static T Clamp(T value)
	{
		return Clamp(value, Min, Max);
	}

	public static T Round(float f)
	{
		return CastTo<T>.From(Mathf.Clamp(Mathf.RoundToInt(f), CastTo<int>.From(Min), CastTo<int>.From(Max)));
	}

	public static bool Equals(T a, T b)
	{
		return CastTo<int>.From(a) == CastTo<int>.From(b);
	}

	public static bool Equals(T? a, T? b)
	{
		if (!a.HasValue || !b.HasValue)
		{
			return a.HasValue == b.HasValue;
		}
		return CastTo<int>.From(a.Value) == CastTo<int>.From(b.Value);
	}

	public static bool InRange(T value, T min, T max)
	{
		int num = CastTo<int>.From(value);
		if (num >= CastTo<int>.From(min))
		{
			return num <= CastTo<int>.From(max);
		}
		return false;
	}

	public static bool HasNext(T current)
	{
		return CastTo<int>.From(current) != CastTo<int>.From(Max);
	}

	public static T Next(T current)
	{
		return Values[Math.Min(Values.Length - 1, CastTo<int>.From(current) + 1)];
	}

	public static T NextWrap(T current)
	{
		T result = Next(current);
		if (result.Equals(current))
		{
			return Min;
		}
		return result;
	}

	public static C NextConvert<C>(T flags, C current) where C : struct, IConvertible
	{
		int num = CastTo<int>.From(Max);
		int num2 = CastTo<int>.From(flags);
		int num3 = CastTo<int>.From(1 << CastTo<int>.From(current));
		while (num3 < num)
		{
			num3 <<= 1;
			if ((num3 & num2) != 0)
			{
				return CastTo<C>.From(BitMask.TrailingZeros(num3));
			}
		}
		return current;
	}

	public static C NextConvertWrap<C>(T flags, C current) where C : struct, IConvertible
	{
		int num = CastTo<int>.From(Max);
		int num2 = CastTo<int>.From(flags);
		int num3 = CastTo<int>.From(1 << CastTo<int>.From(current));
		int num4 = num3;
		do
		{
			num3 <<= 1;
			if (num3 > num)
			{
				num3 = CastTo<int>.From(Min);
			}
			if ((num3 & num2) != 0)
			{
				return CastTo<C>.From(BitMask.TrailingZeros(num3));
			}
		}
		while (num3 != num4);
		return current;
	}

	public static T Adjust(T current, int valueAdjustment)
	{
		return Values[Math.Max(0, Math.Min(Values.Length - 1, CastTo<int>.From(current) + valueAdjustment))];
	}

	public static bool HasPrevious(T current)
	{
		return CastTo<int>.From(current) != CastTo<int>.From(Min);
	}

	public static T Previous(T current)
	{
		return Values[Math.Max(0, CastTo<int>.From(current) - 1)];
	}

	public static T PreviousWrap(T current)
	{
		T result = Previous(current);
		if (result.Equals(current))
		{
			return Max;
		}
		return result;
	}

	public static C PreviousConvert<C>(T flags, C current) where C : struct, IConvertible
	{
		int num = CastTo<int>.From(Min);
		int num2 = CastTo<int>.From(flags);
		int num3 = CastTo<int>.From(1 << CastTo<int>.From(current));
		while (num3 > num)
		{
			num3 >>= 1;
			if ((num3 & num2) != 0)
			{
				return CastTo<C>.From(BitMask.TrailingZeros(num3));
			}
		}
		return current;
	}

	public static IEnumerable<T> EnumerateOutwardFrom(T? startValue)
	{
		if (!startValue.HasValue)
		{
			yield break;
		}
		yield return startValue.Value;
		T? previous = startValue;
		T? next = startValue;
		do
		{
			EnumUtil.Previous(ref previous);
			if (previous.HasValue)
			{
				yield return previous.Value;
			}
			EnumUtil.Next(ref next);
			if (next.HasValue)
			{
				yield return next.Value;
			}
		}
		while (next.HasValue || previous.HasValue);
	}

	public static IEnumerable<T> EnumerateDescending(T? startValue = null)
	{
		T current = startValue ?? Max;
		while (true)
		{
			yield return current;
			if (HasPrevious(current))
			{
				current = Previous(current);
				continue;
			}
			break;
		}
	}

	public static T Random(System.Random random, T? min = null, T? max = null)
	{
		return Values[random.Next(min.HasValue ? CastTo<int>.From(min) : 0, max.HasValue ? (CastTo<int>.From(max) + 1) : Values.Length)];
	}

	public static T RandomFlag(System.Random random, T flags)
	{
		using PoolKeepItemListHandle<T> poolKeepItemListHandle = Pools.UseKeepItemList<T>();
		EnumerateFlags<T>.Enumerator enumerator = Flags(flags).GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			poolKeepItemListHandle.Add(current);
		}
		IList<T> list;
		if (poolKeepItemListHandle.Count <= 0)
		{
			IList<T> values = Values;
			list = values;
		}
		else
		{
			IList<T> values = poolKeepItemListHandle.value;
			list = values;
		}
		return random.Item(list);
	}

	public static T RotateFlags(T flags, int rotation)
	{
		return SizeInBits switch
		{
			8 => CastTo<T>.From(BitMask.Rotate(CastTo<byte>.From(flags), rotation)), 
			16 => CastTo<T>.From(BitMask.Rotate(CastTo<ushort>.From(flags), rotation)), 
			32 => CastTo<T>.From(BitMask.Rotate(CastTo<uint>.From(flags), rotation)), 
			64 => CastTo<T>.From(BitMask.Rotate(CastTo<ulong>.From(flags), rotation)), 
			_ => flags, 
		};
	}

	public static T ShiftFlags(T flags, int shift)
	{
		if (shift != 0)
		{
			return CastTo<T>.From((shift > 0) ? ((CastTo<long>.From(flags) << shift) & CastTo<long>.From(AllFlags)) : ((CastTo<long>.From(flags) >> -shift) & CastTo<long>.From(AllFlags)));
		}
		return flags;
	}

	public static int? DistanceToNearest(T flags, T relativeTo)
	{
		uint num = CastTo<uint>.From(flags);
		if (num == 0)
		{
			return null;
		}
		uint num2 = CastTo<uint>.From(relativeTo);
		if ((num & num2) != 0)
		{
			return 0;
		}
		int num3 = SizeInBits / 2;
		for (int i = 1; i <= num3; i++)
		{
			if ((num & CastTo<uint>.From(RotateFlags(relativeTo, i))) != 0 || (num & CastTo<uint>.From(RotateFlags(relativeTo, -i))) != 0)
			{
				return i;
			}
		}
		return null;
	}

	public static int FlagsWithinDistance(T flags, T relativeTo, int distance)
	{
		uint num = CastTo<uint>.From(flags);
		if (num == 0)
		{
			return 0;
		}
		int num2 = 0;
		uint num3 = CastTo<uint>.From(relativeTo);
		if ((num & num3) != 0)
		{
			num2++;
		}
		for (int i = 1; i <= distance; i++)
		{
			if ((num & CastTo<uint>.From(RotateFlags(relativeTo, i))) != 0 || (num & CastTo<uint>.From(RotateFlags(relativeTo, -i))) != 0)
			{
				num2++;
			}
		}
		return num2;
	}

	public static int ContiguousFlagsWithinDistance(T flags, T relativeTo, int distance)
	{
		uint num = CastTo<uint>.From(flags);
		if (num == 0)
		{
			return 0;
		}
		int num2 = 0;
		int num3 = 0;
		for (int i = -distance; i <= distance; i++)
		{
			if ((num & CastTo<uint>.From(RotateFlags(relativeTo, i))) != 0)
			{
				num3 = Math.Max(num3, ++num2);
			}
			else
			{
				num2 = 0;
			}
		}
		return num3;
	}

	public static T MinActiveFlag(T flags)
	{
		uint num = CastTo<uint>.From(flags);
		return CastTo<T>.From(~(num & (num - 1)) & num);
	}

	public static T MaxActiveFlag(T flags)
	{
		uint num = CastTo<uint>.From(flags);
		uint s = num;
		while (num != 0)
		{
			s = num;
			num &= num - 1;
		}
		return CastTo<T>.From(s);
	}

	public static EnumerateFlags<T> Flags(T flags)
	{
		return new EnumerateFlags<T>(flags);
	}

	public static C ConvertFromFlag<C>(T singleFlag) where C : struct, IConvertible
	{
		return CastTo<C>.From(BitMask.TrailingZeros(CastTo<int>.From(singleFlag)));
	}

	public static C ConvertToFlag<C>(T nonFlagValue) where C : struct, IConvertible
	{
		return CastTo<C>.From(1 << CastTo<int>.From(nonFlagValue));
	}

	public static C ConvertToFlags<C>(IEnumerable<T> nonFlagValues) where C : struct, IConvertible
	{
		int num = 0;
		foreach (T nonFlagValue in nonFlagValues)
		{
			num |= CastTo<int>.From(ConvertToFlag<C>(nonFlagValue));
		}
		return CastTo<C>.From(num);
	}

	public static IEnumerable<T> FlagsConverted<F>(F flags) where F : struct, IConvertible
	{
		EnumerateFlags<F>.Enumerator enumerator = EnumUtil<F>.Flags(flags).GetEnumerator();
		while (enumerator.MoveNext())
		{
			F current = enumerator.Current;
			yield return EnumUtil<F>.ConvertFromFlag<T>(current);
		}
	}

	public static T FromInt(int value)
	{
		return CastTo<T>.From(value);
	}

	public static int ToInt(T value)
	{
		return CastTo<int>.From(value);
	}

	public static bool IsValid(T value)
	{
		return ValuesHash.Contains(value);
	}

	public static bool IsInvalid(T value)
	{
		return !IsValid(value);
	}

	public static T Validate(T value)
	{
		if (!IsValid(value))
		{
			return Values[0];
		}
		return value;
	}

	public static void Validate(ref T value)
	{
		value = Validate(value);
	}

	public static List<T> ValidateList(List<T> list, bool removeInvalid = true)
	{
		if (list == null)
		{
			return null;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (!IsValid(list[num]))
			{
				if (removeInvalid)
				{
					list.RemoveAt(num);
				}
				else
				{
					list[num] = Validate(list[num]);
				}
			}
		}
		return list;
	}

	public static void ValidateKeys<V>(ref Dictionary<T, V> dictionary)
	{
		dictionary = dictionary ?? new Dictionary<T, V>();
		foreach (KeyValuePair<T, V> item in dictionary.EnumeratePairsSafe())
		{
			if (!IsValid(item.Key))
			{
				dictionary.Remove(item.Key);
				T key = Validate(item.Key);
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, item.Value);
				}
			}
		}
	}

	public static void ValidateValues<K>(ref Dictionary<K, T> dictionary)
	{
		dictionary = dictionary ?? new Dictionary<K, T>();
		foreach (K item in dictionary.EnumerateKeysSafe())
		{
			dictionary[item] = Validate(dictionary[item]);
		}
	}

	public static string Name(T value)
	{
		return Names.GetValueOrDefault(CastTo<int>.From(value));
	}

	public static string FriendlyName(T value, bool uppercase = true)
	{
		int num = CastTo<int>.From(value);
		Dictionary<int, string> dictionary = (uppercase ? FriendlyNames : FriendlyNamesLowercase);
		if (!IsFlagsEnum)
		{
			return dictionary.GetValueOrDefault(num);
		}
		if (!dictionary.ContainsKey(num))
		{
			dictionary[num] = ((num == 0) ? "None" : ((num == CastTo<int>.From(AllFlags)) ? "All" : (from t in Flags(value).Flags()
				select t.ToString().FriendlyFromCamelOrPascalCase().ToLowerIf(!uppercase)).ToStringSmart()));
		}
		return dictionary[num];
	}

	public static string GetCategory(T value)
	{
		return CategoryByValue[CastTo<int>.From(value)];
	}

	public static C? GetCategory<C>(T value) where C : struct, IConvertible
	{
		return EnumUtil<C>.TryParse(CategoryPascalByValue[CastTo<int>.From(value)]);
	}

	public static Dictionary<T, C> CreateCategoryMap<C>() where C : struct, IConvertible
	{
		return Values.ToDictionary((T t) => t, (T t) => GetCategory<C>(t) ?? EnumUtil<C>.Min);
	}

	public static string GetTooltip(T value)
	{
		return Tooltips[CastTo<int>.From(value)];
	}

	public static string ToRangeString(T min, T max, string noun = "", byte nounSizePercent = 50, T? minRange = null, T? maxRange = null)
	{
		minRange = minRange ?? Min;
		maxRange = maxRange ?? Max;
		int num = CastTo<int>.From(min);
		int num2 = CastTo<int>.From(max);
		int num3 = CastTo<int>.From(minRange.Value);
		int num4 = CastTo<int>.From(maxRange.Value);
		if (num == num3 && num2 == num4)
		{
			return "";
		}
		if (nounSizePercent != 100 && noun != "")
		{
			noun = "<size=" + nounSizePercent + "%>" + noun + "</size>";
		}
		if (num == num2)
		{
			return FriendlyName(min) + noun;
		}
		if (num2 == num4)
		{
			return FriendlyName(min) + "+" + noun;
		}
		if (num == num3)
		{
			return FriendlyName(max) + "-" + noun;
		}
		return "[" + FriendlyName(min) + "~" + FriendlyName(max) + "]" + noun;
	}

	public static string FriendlyNameFlagRanges(T flags, bool uppercase = true)
	{
		if (Equals(flags, AllFlags))
		{
			return "All";
		}
		if (Equals(flags, NoFlags))
		{
			return "None";
		}
		int minInt = CastTo<int>.From(Min);
		int maxInt = CastTo<int>.From(Max);
		int num = CastTo<int>.From(flags);
		string output = "";
		T? minRange = null;
		T? maxRange = null;
		int count = 0;
		T[] values = Values;
		foreach (T val in values)
		{
			if ((CastTo<int>.From(val) & num) == 0)
			{
				if (minRange.HasValue)
				{
					AppendRange();
					count = 0;
					minRange = (maxRange = null);
				}
				continue;
			}
			int num2 = count + 1;
			count = num2;
			T valueOrDefault = minRange.GetValueOrDefault();
			if (!minRange.HasValue)
			{
				valueOrDefault = val;
				minRange = valueOrDefault;
			}
			maxRange = val;
		}
		if (minRange.HasValue)
		{
			AppendRange();
		}
		return output;
		void AppendRange()
		{
			if (output.Length > 0)
			{
				output += ", ";
			}
			output += RangeToString();
		}
		string RangeToString()
		{
			if (count == 1)
			{
				return FriendlyName(minRange.Value, uppercase);
			}
			int num3 = CastTo<int>.From(minRange);
			int num4 = CastTo<int>.From(maxRange);
			if (num3 == minInt)
			{
				return FriendlyName(maxRange.Value, uppercase) + "-";
			}
			if (num4 == maxInt)
			{
				return FriendlyName(minRange.Value, uppercase) + "+";
			}
			if (count == 2)
			{
				return FriendlyName(minRange.Value, uppercase) + ", " + FriendlyName(maxRange.Value, uppercase);
			}
			return FriendlyName(minRange.Value, uppercase) + " - " + FriendlyName(maxRange.Value, uppercase);
		}
	}

	public static T GetDefaultValue(string defaultCategory = "")
	{
		if (!DefaultValues.ContainsKey(defaultCategory))
		{
			return Min;
		}
		return DefaultValues[defaultCategory];
	}

	public static T GetDefaultValue<C>(C category) where C : struct, IConvertible
	{
		return GetDefaultValue(EnumUtil.Name(category));
	}

	public static T[] ToArray(T value)
	{
		int key = CastTo<int>.From(value);
		if (!Arrays.ContainsKey(key))
		{
			Dictionary<int, T[]> arrays = Arrays;
			T[] obj = new T[1] { value };
			T[] result = obj;
			arrays[key] = obj;
			return result;
		}
		return Arrays[key];
	}

	public static T[] ToArray(T a, T b)
	{
		int key = CastTo<int>.From(Max) + ((1 << CastTo<int>.From(a)) | (1 << CastTo<int>.From(b)));
		if (!Arrays.ContainsKey(key))
		{
			Dictionary<int, T[]> arrays = Arrays;
			T[] obj = new T[2] { a, b };
			T[] result = obj;
			arrays[key] = obj;
			return result;
		}
		return Arrays[key];
	}

	public static GameObject GetResourceBlueprint(T value)
	{
		int key = CastTo<int>.From(value);
		if (!ResourceMap.ContainsKey(key))
		{
			string text = _ResourcePath;
			if (_ResourceCategoriesAreSubFolders && GetCategory(value) != "Miscellaneous")
			{
				text = text + GetCategory(value) + "/";
			}
			ResourceMap.Add(key, Resources.Load<GameObject>(text + FriendlyName(value)));
		}
		return ResourceMap[key];
	}

	public static C GetResource<C>(T value) where C : Component
	{
		return GetResourceBlueprint(value).GetComponent<C>();
	}

	public static void LoadAllResourceBlueprints()
	{
		if (ResourceMap.Count != Values.Length)
		{
			T[] values = Values;
			for (int i = 0; i < values.Length; i++)
			{
				GetResourceBlueprint(values[i]);
			}
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void ValidateResourceBlueprints()
	{
		T[] values = Values;
		foreach (T value in values)
		{
			UnityEngine.Debug.Log("Validating Resource for: " + FriendlyName(value));
			try
			{
				GetResourceBlueprint(value);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError("Failed to get resource for: " + FriendlyName(value) + "\n" + ex.Message);
			}
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void ValidateFriendlyNames()
	{
		T[] values = Values;
		for (int i = 0; i < values.Length; i++)
		{
			UnityEngine.Debug.Log(FriendlyName(values[i]));
		}
	}

	public static T? TryParse(string enumValueName)
	{
		if (!ValuesByName.ContainsKey(enumValueName))
		{
			return null;
		}
		return ValuesByName[enumValueName];
	}

	public static T ParseToNearest(string enumValueName)
	{
		return enumValueName?.FuzzyMatchBestResult(Values, (T v) => FriendlyName(v)) ?? default(T);
	}

	public static Int2 Range()
	{
		return new Int2(CastTo<int>.From(Min), CastTo<int>.From(Max));
	}
}
