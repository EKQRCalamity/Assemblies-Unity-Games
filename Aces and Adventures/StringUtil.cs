using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Diacritics.Extensions;
using UnityEngine;

public static class StringUtil
{
	public const string PERCENT_FORMAT = "#;-#;0";

	public const string FLOAT_SMALL_PERCENT_FORMAT = "#.#;-#.#;0";

	public const string LORUM_IPSUM = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

	private const char COLLAPSE_SEPARATOR_CHAR = '\u00a0';

	public static readonly string COLLAPSE_SEPARATOR = '\u00a0'.ToString();

	private static readonly TextBuilder Builder = new TextBuilder();

	private static char[] _PrintableASCII;

	private static char[] _PascalCaseFromFriendlySplitChars = new char[11]
	{
		' ', '_', '(', ')', '/', '\\', ':', '|', '.', '?',
		'!'
	};

	private static char[] _FriendlyFromLowerCaseUnderscoreCharData = new char[1] { ' ' };

	private static readonly CultureInfo _EnglishCultureInfo = new CultureInfo("en-US");

	private static ReplaceRegex _ReplaceToHtmlRegex;

	private static StringBuilder _TagBuilder;

	private static HashSet<char> _ValidTagCharacters;

	private static HashSet<string> _DistinctTagHash;

	private static readonly Dictionary<int, string> _IntStringMap = new Dictionary<int, string>();

	private static readonly string[] ENUM_FLAGS_SPLIT = new string[1] { ", " };

	private static TextBuilder _ParseBuilder;

	private static TextBuilder _VarBuilder;

	private static char[] _VarChars = new char[3];

	private static char[] PrintableASCII => Enumerable.Range(32, 95).Select(Convert.ToChar).ToArray();

	private static ReplaceRegex ReplaceToHtmlRegex => _ReplaceToHtmlRegex ?? (_ReplaceToHtmlRegex = new ReplaceRegex(new Dictionary<string, string>
	{
		{ "'", "&#39;" },
		{ "‘", "&#39;" },
		{ "’", "&#39;" },
		{ "-", "&ndash;" },
		{ "–", "&ndash;" },
		{ "—", "&ndash;" },
		{ "―", "&ndash;" },
		{ "…", "..." }
	}, RegexOptions.Compiled));

	private static StringBuilder TagBuilder => _TagBuilder ?? (_TagBuilder = new StringBuilder());

	private static HashSet<char> ValidTagCharacters => _ValidTagCharacters ?? (_ValidTagCharacters = new HashSet<char>(PrintableASCII.Where((char c) => char.IsLetterOrDigit(c) || FuzzySearch.MatchSplit.Contains(c))));

	private static HashSet<string> DistinctTagHash => _DistinctTagHash ?? (_DistinctTagHash = new HashSet<string>(StringComparer.OrdinalIgnoreCase));

	private static TextBuilder ParseBuilder => _ParseBuilder ?? (_ParseBuilder = new TextBuilder(clearOnToString: true));

	private static TextBuilder VarBuilder => _VarBuilder ?? (_VarBuilder = new TextBuilder(clearOnToString: true));

	public static string Indent(this string s, int tabCount = 1)
	{
		return new string('\t', tabCount) + s;
	}

	public static string IndentSpaces(this string s, int spaceCount = 3)
	{
		return new string(' ', spaceCount) + s;
	}

	public static char Last(this StringBuilder sb)
	{
		if (sb.Length <= 0)
		{
			return ' ';
		}
		return sb[sb.Length - 1];
	}

	public static string FriendlyFromCamelOrPascalCase(this string s)
	{
		if (s.IsNullOrEmpty())
		{
			return s;
		}
		StringBuilder stringBuilder = new StringBuilder(s.Length);
		bool flag = false;
		bool flag2 = true;
		bool flag3 = false;
		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];
			if (flag)
			{
				if (!char.IsLetterOrDigit(c))
				{
					stringBuilder.Space();
					continue;
				}
				bool flag4 = flag2;
				if ((flag2 != (flag2 = char.IsUpper(c)) && flag2) | (flag3 != (flag3 = char.IsDigit(c)) && ((flag3 && !flag4) || (!flag3 && flag2))))
				{
					stringBuilder.Space();
				}
				stringBuilder.Append(s[i]);
			}
			else if (flag = char.IsLetterOrDigit(c))
			{
				stringBuilder.Append(char.ToUpper(c));
			}
		}
		return stringBuilder.ToString();
	}

	public static string PascalCaseFromFriendly(this string s)
	{
		if (s.IsNullOrEmpty())
		{
			return s;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = s.Split(_PascalCaseFromFriendlySplitChars);
		foreach (string s2 in array)
		{
			stringBuilder.Append(s2.ToPascalCase());
		}
		return stringBuilder.ToString();
	}

	public static string FriendlyFromCamelOrPascalCasePrefix(this string s)
	{
		for (int i = 0; i < s.Length; i++)
		{
			if (char.IsLetterOrDigit(s[i]))
			{
				return s.Substring(0, i);
			}
		}
		return s;
	}

	public static string FriendlyFromLowerCaseUnderscore(this string s, bool checkForCamelAndPascalCase = false)
	{
		StringBuilder stringBuilder = new StringBuilder(s.Length);
		stringBuilder.RemoveMultipleSequentialChars(s.Replace('_', ' '), 1, _FriendlyFromLowerCaseUnderscoreCharData);
		stringBuilder.Trim(trimStart: true, trimEnd: true, _FriendlyFromLowerCaseUnderscoreCharData);
		stringBuilder.ToUpperCaseWords(_FriendlyFromLowerCaseUnderscoreCharData);
		if (!checkForCamelAndPascalCase)
		{
			return stringBuilder.ToString();
		}
		return stringBuilder.ToString().FriendlyFromCamelOrPascalCase();
	}

	public static string AnimationClipNameToFriendly(this string animationClipName)
	{
		return animationClipName.Split(new char[3] { '#', '$', '&' }, 2)[0].Replace("(", "").Replace(")", "").FriendlyFromLowerCaseUnderscore(checkForCamelAndPascalCase: true);
	}

	public static string ToTitleCase(this string s, CultureInfo culture = null)
	{
		return (culture ?? _EnglishCultureInfo).TextInfo.ToTitleCase(s);
	}

	public static string ToPascalCase(this string s)
	{
		if (s.IsNullOrEmpty())
		{
			return null;
		}
		if (char.IsUpper(s[0]))
		{
			return s;
		}
		return s.ReplaceAtIndex(0, char.ToUpper(s[0]));
	}

	public static string ToLowerIf(this string s, bool condition)
	{
		if (!condition)
		{
			return s;
		}
		return s.ToLower();
	}

	private static string _FirstCase(this string s, bool upper = true)
	{
		if (s.IsNullOrEmpty())
		{
			return s;
		}
		if (upper ? char.IsUpper(s[0]) : char.IsLower(s[0]))
		{
			return s;
		}
		bool flag = false;
		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];
			switch (c)
			{
			case '<':
				flag = true;
				break;
			case '>':
				flag = false;
				break;
			}
			if (flag)
			{
				continue;
			}
			if (char.IsLetter(c))
			{
				if (!(upper ^ char.IsUpper(c)))
				{
					return s;
				}
				return s.ReplaceAtIndex(i, upper ? char.ToUpper(c) : char.ToLower(c));
			}
			if (char.IsDigit(c))
			{
				return s;
			}
		}
		return s;
	}

	public static string FirstUpper(this string s)
	{
		return s._FirstCase();
	}

	public static string FirstUpperIf(this string s, bool upper)
	{
		if (!upper)
		{
			return s;
		}
		return s.FirstUpper();
	}

	public static string FirstLower(this string s)
	{
		return s._FirstCase(upper: false);
	}

	public static string AlternateCasing(this string s)
	{
		if (!s.HasVisibleCharacter())
		{
			return s;
		}
		return s.GetCasing() switch
		{
			StringCase.Lower => s.FirstUpper(), 
			StringCase.Upper => s.FirstLower(), 
			StringCase.Title => s.ToLower(), 
			_ => s, 
		};
	}

	public static string FixIncorrectCapitalization(this string s, IList<string> properNouns, bool debug = false)
	{
		if (s.IsNullOrEmpty())
		{
			return s;
		}
		string text = s;
		int num = 0;
		while (true)
		{
			if (num < text.Length)
			{
				char c = text[num];
				if (c == '.' || c == '?' || c == '!')
				{
					break;
				}
				num++;
				continue;
			}
			return s;
		}
		StringBuilder stringBuilder = new StringBuilder(s.Length);
		bool flag = true;
		bool flag2 = true;
		bool flag3 = false;
		bool flag4 = false;
		int x;
		for (x = 0; x < s.Length; x++)
		{
			char c2 = s[x];
			if ((c2 != '<' || !(flag3 = true)) && (c2 != '>' || (flag3 = false)) && (c2 != '{' || !(flag4 = true)) && (c2 != '}' || (flag4 = false)) && !(flag3 || flag4))
			{
				if (char.IsUpper(c2))
				{
					if (!flag2)
					{
						goto IL_01f1;
					}
					if (!MatchesAtIndex(s, "The", x) || x >= s.Length - 4 || properNouns == null || !properNouns.Any((string n) => MatchesAtIndex(s, n, x + 4)))
					{
						string text2 = properNouns?.Where((string n) => MatchesAtIndex(s, n, x)).MaxBy((string n) => n.Length);
						if (text2 == null)
						{
							goto IL_01f1;
						}
						stringBuilder.Append(text2);
						x += text2.Length;
						if (x == s.Length)
						{
							break;
						}
						c2 = s[x];
					}
				}
				goto IL_01fd;
			}
			stringBuilder.Append(c2);
			continue;
			IL_01fd:
			stringBuilder.Append(c2);
			flag2 = !char.IsLetterOrDigit(c2);
			if (c2 == '.' || c2 == '?' || c2 == '!' || c2 == ':')
			{
				flag = true;
			}
			else if (!flag2)
			{
				flag = false;
			}
			continue;
			IL_01f1:
			if (!flag)
			{
				c2 = char.ToLower(c2);
			}
			goto IL_01fd;
		}
		string text3 = stringBuilder.ToString();
		if (debug && text3 != s)
		{
			Debug.Log("PROPER NOUNS not accounted for found in text: " + s + " -> " + text3);
		}
		return text3;
	}

	public static bool MatchesAtIndex(string largerString, string substringMatch, int startIndex, bool ignoreCase = false)
	{
		int num = startIndex + substringMatch.Length;
		if (num > largerString.Length)
		{
			return false;
		}
		int num2 = 0;
		int num3 = startIndex;
		while (num3 < num)
		{
			char c = largerString[num3];
			char c2 = substringMatch[num2];
			if (ignoreCase)
			{
				c = char.ToUpperInvariant(c);
				c2 = char.ToUpperInvariant(c2);
			}
			if (c != c2)
			{
				return false;
			}
			num3++;
			num2++;
		}
		if (num != largerString.Length)
		{
			return !char.IsLetter(largerString[num]);
		}
		return true;
	}

	public static HashSet<string> FindHtmlEntities(this string s)
	{
		HashSet<string> hashSet = new HashSet<string>();
		int startIndex = 0;
		while (true)
		{
			int num = s.IndexOf('&', startIndex);
			if (num < 0)
			{
				break;
			}
			int num2 = s.IndexOf(';', num);
			if (num2 < 0)
			{
				break;
			}
			startIndex = num2 + 1;
			int num3 = num + 1;
			while (true)
			{
				if (num3 < num2)
				{
					if (char.IsWhiteSpace(s[num3]))
					{
						break;
					}
					num3++;
					continue;
				}
				hashSet.Add(s.Substring(num, num2 - num + 1));
				break;
			}
		}
		return hashSet;
	}

	public static string Pluralize(this string s, int num)
	{
		if (num > 1 || num == 0)
		{
			return s + "s";
		}
		return s;
	}

	public static string PluralizeParentheses(this string s, int num)
	{
		if (num > 1 || num == 0)
		{
			return s + "(s)";
		}
		return s;
	}

	public static bool IsNullOrEmpty(this string s)
	{
		return string.IsNullOrEmpty(s);
	}

	public static string ReplaceNullOrEmpty(this string s, string replaceWith = "NULL")
	{
		if (!s.IsNullOrEmpty())
		{
			return s;
		}
		return replaceWith;
	}

	public static bool HasVisibleCharacter(this string s)
	{
		if (s != null)
		{
			for (int i = 0; i < s.Length; i++)
			{
				if (!char.IsWhiteSpace(s[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool HasLetter(this string s)
	{
		if (s == null)
		{
			return false;
		}
		for (int i = 0; i < s.Length; i++)
		{
			if (char.IsLetter(s[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static string ParseNullSource(this string s)
	{
		if (!(s == " "))
		{
			return s;
		}
		return "";
	}

	public static bool CanSortSearchResults(this string s)
	{
		if (s != null)
		{
			for (int i = 0; i < s.Length; i++)
			{
				if (!char.IsWhiteSpace(s[i]) && s[i] != '*')
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool IsNumeric(this string s)
	{
		if (s.IsNullOrEmpty())
		{
			return false;
		}
		for (int i = 0; i < s.Length; i++)
		{
			if (!char.IsDigit(s[i]) && !char.IsWhiteSpace(s[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsAlphanumeric(this string s, bool allowSpaces = false)
	{
		if (s.IsNullOrEmpty())
		{
			return false;
		}
		foreach (char c in s)
		{
			if (!char.IsLetterOrDigit(c) && (!allowSpaces || c != ' '))
			{
				return false;
			}
		}
		return true;
	}

	public static KeyValuePair<string, V> PairWith<V>(this string s, V pairWith)
	{
		return new KeyValuePair<string, V>(s, pairWith);
	}

	public static KeyValuePair<string, Action> Action(this string s, Action action)
	{
		return new KeyValuePair<string, Action>(s, action);
	}

	public static string RemoveFromEnd(this string s, int count)
	{
		return s.Remove(s.Length - count);
	}

	public static string RemoveFromEnd(this string s, char c)
	{
		if (!s.IsNullOrEmpty())
		{
			if (s[s.Length - 1] != c)
			{
				return s;
			}
			return s.RemoveFromEnd(1);
		}
		return s;
	}

	public static string MaxLengthOf(this string s, int maxLength, string truncatedAppend = "...")
	{
		if (s.Length <= maxLength + truncatedAppend.Length)
		{
			return s;
		}
		return s.RemoveFromEnd(s.Length - maxLength) + truncatedAppend;
	}

	public static string ReplaceAtIndex(this string s, int index, char newCharacter)
	{
		char[] array = s.ToCharArray();
		array[index] = newCharacter;
		return new string(array);
	}

	public static string ReplaceWhere(this string s, Func<char, bool> replace, char replaceWith)
	{
		using PoolHandle<StringBuilder> poolHandle = Pools.Use<StringBuilder>();
		StringBuilder value = poolHandle.value;
		for (int i = 0; i < s.Length; i++)
		{
			value.Append(replace(s[i]) ? replaceWith : s[i]);
		}
		return value.ToString();
	}

	public static string ReplaceWords(this string s, string replace, string replaceWith, StringComparison comparison = StringComparison.Ordinal)
	{
		int startIndex = 0;
		while (true)
		{
			int num = s.IndexOf(replace, startIndex, comparison);
			if (num < 0)
			{
				break;
			}
			if (num == 0 || char.IsWhiteSpace(s[num - 1]))
			{
				s = s.Remove(num, replace.Length);
				s = s.Insert(num, replaceWith);
				startIndex = num + replaceWith.Length;
			}
			else
			{
				startIndex = num + replace.Length;
			}
		}
		return s;
	}

	public static string ReplaceManyWords(this string s, Dictionary<string, string> map, StringComparison comparison = StringComparison.Ordinal, Func<char, bool> isValidPreWord = null, Func<char, bool> isValidPostWord = null, ICollection<string> protectedTerms = null)
	{
		if (s == null || map.IsNullOrEmpty())
		{
			return s;
		}
		if (protectedTerms == null)
		{
			protectedTerms = new string[0];
		}
		int startIndex = 0;
		while (true)
		{
			KeyValuePair<string, int> keyValuePair = GetNextMatch(map.Keys);
			string key = keyValuePair.Key;
			if (key == null)
			{
				break;
			}
			int value = keyValuePair.Value;
			KeyValuePair<string, int> keyValuePair2 = GetNextMatch(protectedTerms);
			if (keyValuePair2.Key.HasVisibleCharacter() && ((keyValuePair2.Value >= value && keyValuePair2.Value <= value + key.Length) || (value >= keyValuePair2.Value && value <= keyValuePair2.Value + keyValuePair2.Key.Length) || keyValuePair2.Value + keyValuePair2.Key.Length < value))
			{
				startIndex = keyValuePair2.Value + keyValuePair2.Key.Length;
				continue;
			}
			string text = map[key];
			s = s.Remove(value, key.Length).Insert(value, text);
			startIndex = value + text.Length;
		}
		return s;
		KeyValuePair<string, int> GetNextMatch(IEnumerable<string> terms)
		{
			return (from term in terms
				select new KeyValuePair<string, int>(term, s.IndexOf(term, startIndex, comparison)) into p
				where p.Value >= 0 && (p.Value == 0 || IsValidPreWord(p.Key, s[p.Value - 1])) && (p.Value + p.Key.Length == s.Length || IsValidPostWord(p.Key, s[p.Value + p.Key.Length]))
				select p).MinOrDefault(delegate(KeyValuePair<string, int> a, KeyValuePair<string, int> b)
			{
				int num = a.Value.CompareTo(b.Value);
				return (num == 0) ? b.Key.Length.CompareTo(a.Key.Length) : num;
			});
		}
		bool IsValidPostWord(string term, char c)
		{
			Func<char, bool> func = isValidPostWord;
			if (func == null)
			{
				if (char.IsLetter(c))
				{
					return !char.IsLetter(term[term.Length - 1]);
				}
				return true;
			}
			return func(c);
		}
		bool IsValidPreWord(string term, char c)
		{
			Func<char, bool> func2 = isValidPreWord;
			if (func2 == null)
			{
				if (char.IsLetter(c))
				{
					return !char.IsLetter(term[0]);
				}
				return true;
			}
			return func2(c);
		}
	}

	public static string ProcessTextBetween(this string s, Func<string, string> processText, string start = "{", string end = "}", StringComparison comparison = StringComparison.Ordinal)
	{
		if (s.IsNullOrEmpty())
		{
			return s;
		}
		StringBuilder stringBuilder = new StringBuilder(s.Length);
		int num = 0;
		while (true)
		{
			string text = s.Substring(start, end, comparison, null, num, "");
			if (text == null)
			{
				break;
			}
			int num2 = s.IndexOf(start, num, comparison) + start.Length - num;
			stringBuilder.Append(s.Substring(num, num2));
			stringBuilder.Append(processText(text));
			stringBuilder.Append(end);
			num = s.IndexOf(end, num + num2, comparison) + end.Length;
		}
		if (num < s.Length)
		{
			stringBuilder.Append(s.Substring(num, s.Length - num));
		}
		return stringBuilder.ToString();
	}

	public static bool ContainsWord(this string s, string word, StringComparison comparison = StringComparison.Ordinal, Func<char, bool> isValidPreWord = null, Func<char, bool> isValidPostWord = null)
	{
		if (s.IsNullOrEmpty() || word.IsNullOrEmpty())
		{
			return false;
		}
		if (isValidPreWord == null)
		{
			isValidPreWord = (char.IsLetter(word[0]) ? ((Func<char, bool>)((char c) => !char.IsLetter(c))) : ((Func<char, bool>)((char c) => true)));
		}
		if (isValidPostWord == null)
		{
			isValidPostWord = (char.IsLetter(word[word.Length - 1]) ? ((Func<char, bool>)((char c) => !char.IsLetter(c))) : ((Func<char, bool>)((char c) => true)));
		}
		int num = 0;
		while (true)
		{
			int num2 = s.IndexOf(word, num, comparison);
			if (num2 < 0)
			{
				break;
			}
			num = num2 + word.Length;
			if ((num2 == 0 || isValidPreWord(s[num2 - 1])) && (num == s.Length || isValidPostWord(s[num])))
			{
				return true;
			}
		}
		return false;
	}

	public static string RemoveRichText(this string s)
	{
		if (s.IsNullOrEmpty())
		{
			return "";
		}
		return Regex.Replace(s, "<.*?>", string.Empty, RegexOptions.Compiled);
	}

	public static string ToHtmlText(this string s)
	{
		return s.RemoveRichText().ReplaceHtmlCharacters();
	}

	public static string ReplaceHtmlCharacters(this string s)
	{
		return ReplaceToHtmlRegex.Replace(s);
	}

	public static string GetTextBetween(this string s, string start, string end, string delimiter = " ")
	{
		return Regex.Matches(s, start + "(.*?)" + end).Cast<Match>().ToStringSmart((Match match) => match.Value, delimiter);
	}

	public static IEnumerable<string> GetTextsBetween(this string s, string start, string end, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
	{
		PoolKeepItemListHandle<string> list = Pools.UseKeepItemList<string>();
		s.ProcessTextBetween((string input) => list.AddReturn(input), start, end, comparison);
		return list.AsEnumerable();
	}

	public static IEnumerable<string> GetDiacriticAndNonDiacriticStrings(this string s)
	{
		yield return s;
		string text = s.RemoveDiacritics();
		if (text != s)
		{
			yield return text;
		}
	}

	public static bool HasExcessSpacing(this string s)
	{
		if (s.IsNullOrEmpty())
		{
			return false;
		}
		for (int i = 1; i < s.Length; i++)
		{
			if (s[i] == ' ' && s[i - 1] == ' ')
			{
				return true;
			}
		}
		return false;
	}

	public static string ToPercentString(this float f)
	{
		return (f * 100f).ToString((Math.Abs(f) < 0.01f) ? "#.#;-#.#;0" : "#;-#;0") + "%";
	}

	public static string ToRangePercent(float min, float max, string delimiter = "~")
	{
		Builder.Clear();
		Builder.RangeF(min, max, isPercent: true, delimiter);
		return Builder.ToString();
	}

	public static string ToRange(float min, float max, string delimiter = "~")
	{
		Builder.Clear();
		Builder.RangeF(min, max, isPercent: false, delimiter);
		return Builder.ToString();
	}

	public static string ToRange(int min, int max, string delimiter = "~")
	{
		Builder.Clear();
		Builder.Range(min, max, delimiter);
		return Builder.ToString();
	}

	public static string PrependSign(int value)
	{
		return ((value > 0) ? "+" : ((value < 0) ? "-" : "")) + Math.Abs(value);
	}

	public static string SetRedundantStringNull(this string s, string redundantWith = "")
	{
		if (s == null)
		{
			return null;
		}
		s = s.Trim();
		if (s == "")
		{
			return null;
		}
		if (s == redundantWith)
		{
			return null;
		}
		return s;
	}

	public static string ToStringRoundZero(this float f, string format)
	{
		int? num = null;
		foreach (char c in format)
		{
			if (num.HasValue)
			{
				if (c == ';')
				{
					break;
				}
				num++;
			}
			else if (c == '.')
			{
				num = 0;
			}
		}
		if (num.HasValue)
		{
			f = MathUtil.DeltaSnap(f, 0f, Mathf.Pow(10f, -num.Value));
		}
		return f.ToString(format);
	}

	public static int TryParse(this int i, string value)
	{
		if (!int.TryParse(value, out var result))
		{
			return i;
		}
		return result;
	}

	public static float TryParse(this float f, string value)
	{
		if (!float.TryParse(value, out var result))
		{
			return f;
		}
		return result;
	}

	public static float TryParse(this float f, string value, out bool success)
	{
		success = float.TryParse(value, out var result);
		if (!success)
		{
			return f;
		}
		return result;
	}

	public static float ParseInvariantF(string value)
	{
		return float.Parse(value, CultureInfo.InvariantCulture);
	}

	public static byte ParseInvariantB(string value)
	{
		return byte.Parse(value, CultureInfo.InvariantCulture);
	}

	public static Vector2 ParseInvariantVector2(string value)
	{
		string[] array = value.Split(',');
		return new Vector2(ParseInvariantF(array[0]), ParseInvariantF(array[1]));
	}

	public static Int2 ParseInvariantInt2(string value)
	{
		string[] array = value.Split(',');
		return new Int2(int.Parse(array[0], CultureInfo.InvariantCulture), int.Parse(array[1], CultureInfo.InvariantCulture));
	}

	public static List<int> ToIntegers(this string stringContainingIntegers)
	{
		return (from s in Regex.Split(stringContainingIntegers, "\\D+")
			where !s.IsNullOrEmpty()
			select s).Select(int.Parse).ToList();
	}

	public static string GetListIndexString(this string s)
	{
		if (s.IsNullOrEmpty() || !char.IsDigit(s[0]))
		{
			return s;
		}
		for (int i = 1; i < s.Length; i++)
		{
			if (!char.IsDigit(s[i]))
			{
				return s.Substring(0, i);
			}
		}
		return s;
	}

	public static string GetPersistedCollapseString(this string s)
	{
		s = s.GetListIndexString();
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == '\u00a0')
			{
				return s.Substring(0, i);
			}
		}
		return s;
	}

	public static void Space(this StringBuilder sb)
	{
		if (sb.Last() != ' ')
		{
			sb.Append(' ');
		}
	}

	public static string SpaceIfNotEmpty(this string s)
	{
		if (!s.IsNullOrEmpty() && !char.IsWhiteSpace(s[s.Length - 1]))
		{
			return s + " ";
		}
		return s;
	}

	public static string PreSpaceIfNotEmpty(this string s)
	{
		if (!s.IsNullOrEmpty())
		{
			return " " + s;
		}
		return s;
	}

	public static string PreSpaceIf(this string s, bool preSpace)
	{
		if (!preSpace)
		{
			return s;
		}
		return s.PreSpaceIfNotEmpty();
	}

	public static string NewLineIfNotEmpty(this string s)
	{
		if (!s.IsNullOrEmpty())
		{
			return s + "\n";
		}
		return s;
	}

	public static string SizeIfNotEmpty(this string s, int size = 66)
	{
		if (!s.IsNullOrEmpty())
		{
			return $"<size={size}%>{s}</size>";
		}
		return s;
	}

	public static string SizeIf(this string s, bool condition, int size = 66)
	{
		if (!condition)
		{
			return s;
		}
		return $"<size={size}%>{s}</size>";
	}

	public static string NoBreakIfNotEmpty(this string s)
	{
		if (!s.IsNullOrEmpty())
		{
			return "<nobr>" + s + "</nobr>";
		}
		return s;
	}

	public static string ItalicIfNotEmpty(this string s)
	{
		if (!s.IsNullOrEmpty())
		{
			return "<i>" + s + "</i>";
		}
		return s;
	}

	public static string BoldIfNotEmpty(this string s)
	{
		if (!s.IsNullOrEmpty())
		{
			return "<b>" + s + "</b>";
		}
		return s;
	}

	public static string AppendIfNotEmpty(this string s, string append)
	{
		if (!s.IsNullOrEmpty())
		{
			return s + append;
		}
		return s;
	}

	public static string LineHeightIfNotEmpty(this string s, int lineHeightPercent)
	{
		if (!s.IsNullOrEmpty())
		{
			return s + $"<line-height={lineHeightPercent}%>";
		}
		return s;
	}

	public static string GetUniqueKey(this string s, Func<object, bool> excludedValues)
	{
		if (excludedValues == null || !excludedValues(s))
		{
			return s;
		}
		int num = 0;
		string text;
		do
		{
			int num2 = ++num;
			text = s + " " + num2;
		}
		while (excludedValues(text));
		return text;
	}

	public static string ToText(this bool? b, string trueText, string falseText)
	{
		if (!b.HasValue)
		{
			return "";
		}
		if (!b.Value)
		{
			return falseText;
		}
		return trueText;
	}

	public static string ToText(this bool b, string trueText, string falseText = "")
	{
		if (!b)
		{
			return falseText;
		}
		return trueText;
	}

	public static string ToURI(this string s)
	{
		return IOUtil.ToURI(s);
	}

	public static string ToTagString(this StringBuilder sb, string s, HashSet<string> distinctTagHash = null)
	{
		if (s.IsNullOrEmpty())
		{
			return "";
		}
		distinctTagHash = distinctTagHash ?? DistinctTagHash;
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];
			if (char.IsWhiteSpace(c))
			{
				c = ' ';
			}
			if (!ValidTagCharacters.Contains(c) && !char.IsLetter(c))
			{
				continue;
			}
			flag = flag || !FuzzySearch.MatchSplit.Contains(c);
			if (flag)
			{
				bool flag3 = FuzzySearch.MatchSplit.Contains(c);
				if (!(flag3 && flag2))
				{
					sb.Append(c);
					flag2 = flag3;
				}
			}
		}
		sb.Trim(trimStart: false, trimEnd: true, FuzzySearch.MatchSplit);
		string text = sb.ToString();
		sb.Clear();
		string[] array = text.Split(FuzzySearch.MatchSplit);
		foreach (string item in array)
		{
			distinctTagHash.Add(item);
		}
		foreach (string item2 in distinctTagHash)
		{
			sb.Append(item2).Append(" ");
		}
		if (sb.Length > 0)
		{
			sb.Remove(sb.Length - 1, 1);
		}
		string result = sb.ToString();
		sb.Clear();
		distinctTagHash.Clear();
		return result;
	}

	public static string ToTagString(this string s)
	{
		if (s.IsNullOrEmpty())
		{
			return "";
		}
		return TagBuilder.ToTagString(s);
	}

	public static string ToStringClear(this StringBuilder sb)
	{
		string result = sb.ToString();
		sb.Clear();
		return result;
	}

	public static void CopyToClipboard(this string s)
	{
		TextEditor textEditor = new TextEditor();
		textEditor.text = s;
		textEditor.SelectAll();
		textEditor.Copy();
	}

	public static string AppendSpace(this string s)
	{
		if (s.IsNullOrEmpty() || char.IsWhiteSpace(s[s.Length - 1]))
		{
			return s;
		}
		return s + " ";
	}

	public static string ToStringPooled(this int i)
	{
		return _IntStringMap.GetValueOrDefault(i) ?? (_IntStringMap[i] = i.ToString());
	}

	public static bool ContainsWithPrecedingCharacter(this string s, string contains, char precedingCharacter, StringComparison comparison = StringComparison.Ordinal)
	{
		int? num = s?.IndexOf(contains, comparison);
		if (num.HasValue)
		{
			int valueOrDefault = num.GetValueOrDefault();
			if (valueOrDefault > 0)
			{
				return s[valueOrDefault - 1] == precedingCharacter;
			}
		}
		return false;
	}

	public static string Reverse(this string s)
	{
		char[] array = s.ToCharArray();
		Array.Reverse(array);
		return new string(array);
	}

	public static string Swizzle(this string s)
	{
		StringBuilder stringBuilder = new StringBuilder(s.Length);
		int num = s.Length - 1;
		for (int i = 0; i < num; i += 2)
		{
			stringBuilder.Append(s[i + 1]);
			stringBuilder.Append(s[i]);
		}
		if (s.Length % 2 != 0)
		{
			stringBuilder.Append(s[s.Length - 1]);
		}
		return stringBuilder.ToString();
	}

	public static string RemoveWhiteSpace(this string s)
	{
		StringBuilder stringBuilder = new StringBuilder(s.Length);
		foreach (char c in s)
		{
			if (!char.IsWhiteSpace(c))
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static string Replace(this string input, string oldValue, string newValue, StringComparison comparisonType)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (input.Length == 0)
		{
			return input;
		}
		if (oldValue == null)
		{
			throw new ArgumentNullException("oldValue");
		}
		if (oldValue.Length == 0)
		{
			throw new ArgumentException("String cannot be of zero length.");
		}
		StringBuilder stringBuilder = new StringBuilder(input.Length);
		bool flag = string.IsNullOrEmpty(newValue);
		int num = 0;
		int num2;
		while ((num2 = input.IndexOf(oldValue, num, comparisonType)) >= 0)
		{
			int num3 = num2 - num;
			if (num3 != 0)
			{
				stringBuilder.Append(input, num, num3);
			}
			if (!flag)
			{
				stringBuilder.Append(newValue);
			}
			num = num2 + oldValue.Length;
			if (num == input.Length)
			{
				return stringBuilder.ToString();
			}
		}
		stringBuilder.Append(input, num, input.Length - num);
		return stringBuilder.ToString();
	}

	public static string ReplaceSplit(this string input, string oldValue, string newValue, StringComparison comparison, StringSplitPosition position)
	{
		if (position != 0)
		{
			return input.ReplaceLast(oldValue, newValue, comparison);
		}
		return input.ReplaceFirst(oldValue, newValue, comparison);
	}

	public static string Wrap(this string input, string textToWrap, string prefix, string postfix, StringComparison comparisonType)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (input.Length == 0)
		{
			return input;
		}
		if (textToWrap == null)
		{
			throw new ArgumentNullException("textToWrap");
		}
		if (textToWrap.Length == 0)
		{
			throw new ArgumentException("String cannot be of zero length.");
		}
		StringBuilder stringBuilder = new StringBuilder(input.Length);
		int num = 0;
		int num2;
		while ((num2 = input.IndexOf(textToWrap, num, comparisonType)) >= 0)
		{
			int num3 = num2 - num;
			if (num3 != 0)
			{
				stringBuilder.Append(input, num, num3);
			}
			stringBuilder.Append(prefix);
			stringBuilder.Append(input, num2, textToWrap.Length);
			stringBuilder.Append(postfix);
			num = num2 + textToWrap.Length;
			if (num == input.Length)
			{
				return stringBuilder.ToString();
			}
		}
		stringBuilder.Append(input, num, input.Length - num);
		return stringBuilder.ToString();
	}

	public static List<(string text, StringSplitPosition position)> Split(this string input, string[] delimiters, StringComparison comparisonType, int count = 0)
	{
		List<(string, StringSplitPosition)> list = new List<(string, StringSplitPosition)>();
		int startSearchIndex = 0;
		do
		{
			KeyValuePair<string, int?> keyValuePair = (from p in delimiters.Select(delegate(string delimiter)
				{
					int num2 = input.IndexOf(delimiter, startSearchIndex, comparisonType);
					return new KeyValuePair<string, int?>(delimiter, (num2 >= 0) ? new int?(num2) : null);
				})
				where p.Value.HasValue
				select p).MinOrDefault((KeyValuePair<string, int?> a, KeyValuePair<string, int?> b) => (a.Value != b.Value) ? ((!(a.Value < b.Value)) ? 1 : (-1)) : 0);
			int? value = keyValuePair.Value;
			if (!value.HasValue)
			{
				break;
			}
			int valueOrDefault = value.GetValueOrDefault();
			if (valueOrDefault > startSearchIndex)
			{
				list.Add((input.Substring(startSearchIndex, valueOrDefault - startSearchIndex), StringSplitPosition.BeforeLastDelimiter));
			}
			startSearchIndex = valueOrDefault + keyValuePair.Key.Length;
		}
		while (count <= 0 || list.Count < count - 1);
		int num = input.Length - startSearchIndex;
		if (num > 0)
		{
			list.Add((input.Substring(startSearchIndex, num), StringSplitPosition.AfterLastDelimiter));
		}
		return list;
	}

	public static string Substring(this string input, string startAfter, string endBefore, StringComparison comparison = StringComparison.Ordinal, string outputOnFailure = null, int indexToBeginSearchAt = 0, string outputOnEmptyMatch = null)
	{
		if (input == null || input.Length < startAfter.Length + endBefore.Length)
		{
			return outputOnFailure;
		}
		int num = input.IndexOf(startAfter, indexToBeginSearchAt, comparison);
		if (num >= 0)
		{
			num += startAfter.Length;
			int num2 = input.IndexOf(endBefore, num, comparison);
			if (num2 < 0)
			{
				return outputOnFailure;
			}
			int num3 = num2 - num;
			if (num3 == 0)
			{
				return outputOnEmptyMatch;
			}
			if (num3 <= 0)
			{
				return outputOnFailure;
			}
			return input.Substring(num, num3);
		}
		return outputOnFailure;
	}

	public static StringCase GetCasing(this string s, bool allowTitleCase = true)
	{
		StringCase result = StringCase.Lower;
		bool flag = false;
		bool flag2 = true;
		int num = 0;
		bool flag3 = false;
		foreach (char c in s)
		{
			switch (c)
			{
			case '<':
				flag3 = true;
				break;
			case '>':
				flag3 = false;
				continue;
			}
			if (flag3)
			{
				continue;
			}
			if (char.IsLetterOrDigit(c))
			{
				if (char.IsUpper(c))
				{
					if (flag2)
					{
						num++;
					}
					if (num == 1)
					{
						if (!allowTitleCase)
						{
							return StringCase.Upper;
						}
						result = StringCase.Upper;
					}
					else if (num > 1)
					{
						return StringCase.Title;
					}
				}
				else if (!flag)
				{
					return StringCase.Lower;
				}
				flag = true;
			}
			else if (flag)
			{
				flag2 = char.IsWhiteSpace(c);
			}
			if (c == '.' || c == '?' || c == '!')
			{
				num = 0;
				flag2 = true;
			}
		}
		return result;
	}

	public static string SetCasing(this string s, StringCase casing, CultureInfo culture)
	{
		return casing switch
		{
			StringCase.Lower => s.FirstLower(), 
			StringCase.Upper => s.FirstUpper(), 
			StringCase.Title => s.ToTitleCase(culture), 
			_ => s, 
		};
	}

	public static string PrefixToLength(this string s, char prefix, int length)
	{
		int num = length - s.Length;
		if (num <= 0)
		{
			return s;
		}
		char[] array = new char[length];
		for (int i = 0; i < num; i++)
		{
			array[i] = prefix;
		}
		for (int j = 0; j < s.Length; j++)
		{
			array[j + num] = s[j];
		}
		return new string(array);
	}

	public static string ToHash128(this string s)
	{
		return Hash128.Compute(s).ToString();
	}

	public static bool HasTrimmedStart(this string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			return !char.IsWhiteSpace(s[0]);
		}
		return true;
	}

	public static bool HasTrimmedEnd(this string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			return !char.IsWhiteSpace(s[s.Length - 1]);
		}
		return true;
	}

	public static string CopyTrimming(this string s, string copyTrimmingFrom)
	{
		if (s.IsNullOrEmpty())
		{
			return s;
		}
		bool num = s.HasTrimmedStart();
		bool flag = copyTrimmingFrom.HasTrimmedStart();
		if (num != flag)
		{
			s = (flag ? s.TrimStart() : (copyTrimmingFrom[0] + s));
		}
		bool num2 = s.HasTrimmedEnd();
		bool flag2 = copyTrimmingFrom.HasTrimmedEnd();
		if (num2 != flag2)
		{
			s = (flag2 ? s.TrimEnd() : (s + copyTrimmingFrom[copyTrimmingFrom.Length - 1]));
		}
		return s;
	}

	public static string InsureBracketClosures(this string s, char openBracket = '{', char closeBracket = '}')
	{
		if (s.IsNullOrEmpty())
		{
			return s;
		}
		StringBuilder stringBuilder = new StringBuilder(s.Length);
		int num = 0;
		foreach (char c in s)
		{
			if (c == openBracket)
			{
				num++;
			}
			else if (c == closeBracket && --num < 0)
			{
				continue;
			}
			stringBuilder.Append(c);
		}
		for (int j = 0; j < num; j++)
		{
			stringBuilder.Append(closeBracket);
		}
		return stringBuilder.ToString();
	}

	public static string RemoveWhiteSpacesExceptThoseAfter(this string s, HashSet<char> keepSpacesAfter, bool preserveSpacesAroundRomanLetters = true)
	{
		if (s.IsNullOrEmpty())
		{
			return s;
		}
		StringBuilder stringBuilder = new StringBuilder(s.Length);
		char c = '\0';
		foreach (char c2 in s)
		{
			if (!char.IsWhiteSpace(c2) || c2 == '\n' || keepSpacesAfter.Contains(c) || (preserveSpacesAroundRomanLetters && char.IsLetter(c)))
			{
				stringBuilder.Append(c2);
			}
			c = c2;
		}
		return stringBuilder.ToString();
	}

	public static string CreateFileFilter(IEnumerable<string> strings)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string @string in strings)
		{
			stringBuilder.Append("*");
			stringBuilder.Append(@string);
			stringBuilder.Append(";");
		}
		return stringBuilder.ToString();
	}

	public static string Build(string seperator, params object[] arguments)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = !seperator.IsNullOrEmpty();
		int num = 0;
		for (int i = 0; i < arguments.Length; i++)
		{
			string text = arguments[i].ToString();
			if (!text.IsNullOrEmpty())
			{
				if (num > 0 && flag)
				{
					stringBuilder.Append(seperator);
				}
				stringBuilder.Append(text);
				num++;
			}
		}
		return stringBuilder.ToString();
	}

	public static string ParseFriendlyEnumFlags(string friendlyEnumFlags)
	{
		string[] array = friendlyEnumFlags.Split(ENUM_FLAGS_SPLIT, StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Replace(" ", "");
		}
		return array.ToStringSmart();
	}

	private static string _TimeFromSeconds(int value, string label, string delimiter = ", ", bool forceShow = false, string format = "D")
	{
		if (!forceShow && value <= 0)
		{
			return "";
		}
		return value.ToString(format) + (label.Length > 0).ToText(" <size=75%>" + label.Pluralize(Mathf.Min(label.Length, value)) + "</size>") + delimiter;
	}

	public static string ToTimeFromSeconds(float seconds, string day = "day", string hour = "hour", string minute = "minute", string second = "second", string delimiter = ", ", bool forceShowSeconds = true, bool forceShowMinutes = false)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
		return _TimeFromSeconds(timeSpan.Days, day, delimiter) + _TimeFromSeconds(timeSpan.Hours, hour, delimiter) + _TimeFromSeconds(timeSpan.Minutes, minute, delimiter, forceShowMinutes, (timeSpan.Hours > 0) ? "D2" : "D") + _TimeFromSeconds(timeSpan.Seconds, second, "", forceShowSeconds, forceShowMinutes ? "D2" : "D");
	}

	public static string ToStringSimple(this TimeSpan span)
	{
		return ToTimeFromSeconds((float)span.TotalSeconds, "", "", "", "", ":", forceShowSeconds: true, forceShowMinutes: true);
	}

	public static string ParseVariables(this string s, uint id, Dictionary<string, Func<uint, string, string>> variableMap, ref bool parsed, char startVariable = '{', char endType = ':', char endVariable = '}')
	{
		if (s == null)
		{
			return s;
		}
		_VarChars[0] = startVariable;
		_VarChars[1] = endType;
		_VarChars[2] = endVariable;
		int num = 0;
		TextBuilder textBuilder = ParseBuilder;
		string text = "";
		foreach (char c in s)
		{
			if (c != _VarChars[num])
			{
				textBuilder.Append(c);
				continue;
			}
			switch (num)
			{
			case 0:
				textBuilder = VarBuilder;
				num++;
				break;
			case 1:
				text = textBuilder;
				num++;
				break;
			case 2:
			{
				string text2 = textBuilder.ToString();
				string text3 = (variableMap.ContainsKey(text) ? variableMap[text](id, text2) : null);
				ParseBuilder.Append(text3 ?? $"{startVariable}{text}{endType}{text2}{endVariable}");
				parsed = parsed || text3 != null;
				textBuilder = ParseBuilder;
				num = 0;
				break;
			}
			}
		}
		VarBuilder.Clear();
		return ParseBuilder;
	}

	public static bool ContainsAny(this string s, params string[] values)
	{
		if (s.IsNullOrEmpty() || values.IsNullOrEmpty())
		{
			return false;
		}
		using (PoolKeepItemListHandle<int> poolKeepItemListHandle = Pools.UseKeepItemList<int>())
		{
			for (int i = 0; i < values.Length; i++)
			{
				poolKeepItemListHandle.Add(0);
			}
			for (int j = 0; j < s.Length; j++)
			{
				char c = char.ToLower(s[j]);
				for (int k = 0; k < values.Length; k++)
				{
					if (values[k].IsNullOrEmpty())
					{
						continue;
					}
					if (c == values[k][poolKeepItemListHandle[k]])
					{
						if (++poolKeepItemListHandle[k] == values[k].Length)
						{
							return true;
						}
					}
					else
					{
						poolKeepItemListHandle[k] = 0;
					}
				}
			}
		}
		return false;
	}

	public static string FormatLargeNumber(long num)
	{
		if (num >= 1000000000)
		{
			return ((double)num / 1000000000.0).ToString("0.##B");
		}
		if (num >= 100000000)
		{
			return ((double)num / 1000000.0).ToString("0.#M");
		}
		if (num >= 1000000)
		{
			return ((double)num / 1000000.0).ToString("0.##M");
		}
		if (num >= 100000)
		{
			return ((double)num / 1000.0).ToString("0.#k");
		}
		if (num >= 10000)
		{
			return ((double)num / 1000.0).ToString("0.##k");
		}
		return num.ToString("#,0");
	}

	public static string RangeToString(int min, int max, string delimiter = "~")
	{
		if (min != max)
		{
			return min + delimiter + max;
		}
		return max.ToString();
	}

	public static string RangeToStringF(float min, float max, string format = "0.#", bool isPercent = false, string delimiter = "~")
	{
		string text = (isPercent ? "%" : "");
		if (isPercent)
		{
			min *= 100f;
			max *= 100f;
		}
		if (min == max)
		{
			return max.ToString(format) + text;
		}
		if (isPercent)
		{
			min = Mathf.RoundToInt(min);
			max = Mathf.RoundToInt(max);
		}
		return min.ToString(format) + delimiter + max.ToString(format) + text;
	}

	public static string ToPlaceString(this int place)
	{
		if (place <= 0)
		{
			return "";
		}
		int num = place % 10;
		int num2 = ((place >= 10) ? (place % 100 / 10) : 0);
		string text = "th";
		if (num2 != 1)
		{
			switch (num)
			{
			case 1:
				text = "st";
				break;
			case 2:
				text = "nd";
				break;
			case 3:
				text = "rd";
				break;
			}
		}
		return place + text;
	}
}
