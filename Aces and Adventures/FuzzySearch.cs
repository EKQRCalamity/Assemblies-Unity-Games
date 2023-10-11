using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class FuzzySearch
{
	private const int CONSECUTIVE_INCREMENT = 100;

	private const int CONSECUTIVE_INCREMENT_HIGH = 130;

	private const int TRANSPOSE_PENALTY = -30;

	private const int MISSING_PENALTY = -30;

	public static char[] MatchSplit = new char[8] { ' ', '_', '-', '.', ',', '"', '\\', '/' };

	public const char WILD = '*';

	public const char EXCLUDE = '!';

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool _FuzzyMatchCheckWindow(char searchCharacter, string matchAgainst, int x, ref int transpositionFound, ref bool perfect, ref int consecutiveScoreIncrement)
	{
		bool result = (x > 0 && searchCharacter == matchAgainst[x - 1]) || (x < matchAgainst.Length - 1 && searchCharacter == matchAgainst[x + 1]);
		consecutiveScoreIncrement += -30 / ++transpositionFound;
		perfect = false;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool _FuzzyMatchCheckMissing(string searchString, ref int inputIndex, string matchAgainst, ref int x, ref int transpositionFound, ref int missingCharacterFound, ref bool perfect, ref int consecutiveScoreIncrement)
	{
		if (inputIndex >= searchString.Length - 1)
		{
			return false;
		}
		if (x < matchAgainst.Length - 1)
		{
			x++;
		}
		char c = searchString[++inputIndex];
		consecutiveScoreIncrement += -30 / ++missingCharacterFound;
		perfect = false;
		if (c != '*' && c != matchAgainst[x])
		{
			return _FuzzyMatchCheckWindow(c, matchAgainst, x, ref transpositionFound, ref perfect, ref consecutiveScoreIncrement);
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void _FuzzyMatchReset(ref int maxConsecutiveScore, ref int consecutiveScoreIncrement, ref int currentConsecutiveScore, ref int inputIndex, ref int transpositionFound, ref int missingCharacterFound, ref bool perfect)
	{
		maxConsecutiveScore = ((currentConsecutiveScore > maxConsecutiveScore) ? currentConsecutiveScore : maxConsecutiveScore);
		consecutiveScoreIncrement = 100;
		currentConsecutiveScore = 0;
		inputIndex = 0;
		transpositionFound = 0;
		missingCharacterFound = 0;
		perfect = true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int _FuzzyMatch(string searchString, string matchAgainst)
	{
		if (searchString.IsNullOrEmpty())
		{
			return 0;
		}
		int maxConsecutiveScore = 0;
		int currentConsecutiveScore = 0;
		int consecutiveScoreIncrement = 100;
		bool flag = false;
		int inputIndex = 0;
		int transpositionFound = 0;
		int missingCharacterFound = 0;
		bool perfect = true;
		for (int i = 0; i < matchAgainst.Length; i++)
		{
			char c = searchString[inputIndex];
			bool flag2 = char.IsLetterOrDigit(matchAgainst[i]);
			if ((c == '*' || c == matchAgainst[i] || (flag2 && (_FuzzyMatchCheckWindow(c, matchAgainst, i, ref transpositionFound, ref perfect, ref consecutiveScoreIncrement) || _FuzzyMatchCheckMissing(searchString, ref inputIndex, matchAgainst, ref i, ref transpositionFound, ref missingCharacterFound, ref perfect, ref consecutiveScoreIncrement)))) && consecutiveScoreIncrement > 0)
			{
				if (!flag && inputIndex == 0 && perfect)
				{
					consecutiveScoreIncrement = 130;
				}
				currentConsecutiveScore += consecutiveScoreIncrement;
				if (inputIndex == i)
				{
					currentConsecutiveScore += consecutiveScoreIncrement / 2;
				}
				if (++inputIndex == searchString.Length)
				{
					if (perfect)
					{
						return currentConsecutiveScore + currentConsecutiveScore + consecutiveScoreIncrement + (inputIndex == matchAgainst.Length).ToInt(currentConsecutiveScore);
					}
					_FuzzyMatchReset(ref maxConsecutiveScore, ref consecutiveScoreIncrement, ref currentConsecutiveScore, ref inputIndex, ref transpositionFound, ref missingCharacterFound, ref perfect);
				}
			}
			else
			{
				_FuzzyMatchReset(ref maxConsecutiveScore, ref consecutiveScoreIncrement, ref currentConsecutiveScore, ref inputIndex, ref transpositionFound, ref missingCharacterFound, ref perfect);
			}
			flag = flag2;
		}
		if (maxConsecutiveScore <= currentConsecutiveScore)
		{
			return currentConsecutiveScore;
		}
		return maxConsecutiveScore;
	}

	public static FuzzyStringType GetSearchStringType(this string searchString)
	{
		if (!searchString.HasVisibleCharacter())
		{
			return FuzzyStringType.Empty;
		}
		if (searchString.CanSortSearchResults())
		{
			return FuzzyStringType.Sortable;
		}
		return FuzzyStringType.Standard;
	}

	public static float FuzzyMatch(this string searchString, string matchAgainst, bool insureSearchStringLower = true)
	{
		if (insureSearchStringLower)
		{
			searchString = searchString.ToLower();
		}
		matchAgainst = matchAgainst.ToLower();
		float num = 0f;
		string[] array = searchString.Split(MatchSplit);
		string[] array2 = matchAgainst.Split(MatchSplit);
		for (int i = 0; i < array.Length; i++)
		{
			float num2 = 1f;
			string text = array[i];
			if (text.Length > 0 && text[0] == '!')
			{
				text = text.Remove(0, 1);
				num2 = -1f;
			}
			float num3 = 0f;
			for (int j = 0; j < array2.Length; j++)
			{
				string matchAgainst2 = array2[j];
				float num4 = (float)_FuzzyMatch(text, matchAgainst2) / Mathf.Pow(j + 1, 0.1f) * num2;
				if (!(Math.Abs(num4) < Math.Abs(num3)))
				{
					num3 = num4;
				}
			}
			num += num3;
		}
		return num;
	}

	public static PoolKeepItemListHandle<T> FuzzyMatchSort<T>(this string searchString, IEnumerable<T> itemsToSort, Func<T, string> toMatchAgainstString = null, bool sortOutputWhenSearchStringIsEmpty = false, int matchThresholdDenominator = 5, bool stableSort = false, IComparer<T> emptySearchComparer = null)
	{
		string effectiveSearchString;
		return searchString.FuzzyMatchSort(itemsToSort, out effectiveSearchString, toMatchAgainstString, sortOutputWhenSearchStringIsEmpty, matchThresholdDenominator, stableSort, emptySearchComparer);
	}

	public static PoolKeepItemListHandle<T> FuzzyMatchSort<T>(this string searchString, IEnumerable<T> itemsToSort, out string effectiveSearchString, Func<T, string> toMatchAgainstString = null, bool sortOutputWhenSearchStringIsEmpty = false, int matchThresholdDenominator = 5, bool stableSort = false, IComparer<T> emptySearchComparer = null)
	{
		searchString = searchString ?? "";
		searchString = searchString.ToLower();
		searchString = searchString.Trim();
		effectiveSearchString = searchString;
		PoolKeepItemListHandle<T> poolKeepItemListHandle = Pools.UseKeepItemList<T>();
		if (searchString.CanSortSearchResults())
		{
			poolKeepItemListHandle.CopyFrom(itemsToSort);
			if (sortOutputWhenSearchStringIsEmpty)
			{
				poolKeepItemListHandle.value.Sort(emptySearchComparer ?? Comparer<T>.Default);
			}
			return poolKeepItemListHandle;
		}
		toMatchAgainstString = toMatchAgainstString ?? ((Func<T, string>)((T t) => t.ToString()));
		PoolStructListHandle<Couple<float, T>> poolStructListHandle = Pools.UseStructList<Couple<float, T>>();
		List<Couple<float, T>> value = poolStructListHandle.value;
		float num = float.MaxValue;
		foreach (T item in itemsToSort)
		{
			float num2 = 0f - searchString.FuzzyMatch(toMatchAgainstString(item), insureSearchStringLower: false);
			num = ((num2 < num) ? num2 : num);
			value.Add(new Couple<float, T>(num2, item));
		}
		float num3 = ((num <= 0f) ? (num / (float)matchThresholdDenominator) : (num * (float)matchThresholdDenominator));
		for (int num4 = value.Count - 1; num4 >= 0; num4--)
		{
			if (value[num4].a > num3)
			{
				value.RemoveAt(num4);
			}
		}
		if (stableSort)
		{
			value.StableSort();
		}
		else
		{
			value.Sort();
		}
		List<T> value2 = poolKeepItemListHandle.value;
		foreach (Couple<float, T> item2 in poolStructListHandle)
		{
			value2.Add(item2.b);
		}
		return poolKeepItemListHandle;
	}

	public static T FuzzyMatchBestResult<T>(this string searchString, IEnumerable<T> itemsToSort, Func<T, string> toMatchAgainstString = null, bool sortOutputWhenSearchStringIsEmpty = false, int matchThresholdDenominator = 5, bool stableSort = false, IComparer<T> emptySearchComparer = null)
	{
		using PoolKeepItemListHandle<T> poolKeepItemListHandle = searchString.FuzzyMatchSort(itemsToSort, toMatchAgainstString, sortOutputWhenSearchStringIsEmpty, matchThresholdDenominator, stableSort, emptySearchComparer);
		return poolKeepItemListHandle.value.FirstOrDefault();
	}
}
