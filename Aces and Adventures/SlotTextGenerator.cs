using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotTextGenerator : MonoBehaviour
{
	public enum ForcedCase
	{
		None,
		LowerCase,
		UpperCase
	}

	private static Byte2 LowerCaseAscii;

	private static Byte2 UpperCaseAscii;

	private static Byte2 NumericAscii;

	private const byte SpaceAscii = 32;

	private static Byte2 ValidAsciiRange;

	private const char ReplaceInvalidWith = '~';

	private static readonly char[] LowerCaseGlyphs;

	private static readonly char[] UpperCaseGlyphs;

	private static readonly char[] NumericGlyphs;

	private static readonly char[] SpaceGlyph;

	private static readonly char[] GrammarGlyphs;

	private static readonly char[] LogicGlyphs;

	private static readonly char[] DelimiterGlyphs;

	private static readonly List<char[]> SpecificGlyphSets;

	private static readonly char[] MiscGlyphs;

	[Header("Slots")]
	public List<SlotText> slots;

	public Transform nextSlotTransform;

	[Range(1f, 64f)]
	public int numberOfSlots = 20;

	public bool generateToNumberOfSlotsOnAwake = true;

	public TextAlignment textAlignment;

	public ForcedCase forcedTextCase;

	public string truncationText = "...";

	[Range(0f, 1f)]
	public float delayBetweenLetters;

	[SerializeField]
	protected bool _reverseOrderOfDelayedLetters;

	public bool useScaledTime;

	[Header("Events")]
	[SerializeField]
	protected StringEvent _OnTextChangedEvent;

	private string _text;

	private int? _initialSlotCount;

	private int _slotBlueprintIndex;

	private int _pendingLetterIndex;

	private List<Couple<char, float>> _pendingLetters = new List<Couple<char, float>>();

	private Color32? _pendingColorChange;

	public string text
	{
		get
		{
			return _text ?? (_text = "");
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _text, value ?? ""))
			{
				_OnTextChanged();
			}
		}
	}

	public StringEvent OnTextChanged => _OnTextChangedEvent ?? (_OnTextChangedEvent = new StringEvent());

	public bool reverseOrderOfDelayedLetters
	{
		get
		{
			if (textAlignment == TextAlignment.Right)
			{
				return !_reverseOrderOfDelayedLetters;
			}
			return _reverseOrderOfDelayedLetters;
		}
		set
		{
			_reverseOrderOfDelayedLetters = value;
		}
	}

	public Color32? pendingColorChange
	{
		get
		{
			return _pendingColorChange;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _pendingColorChange, value) && _pendingColorChange.HasValue)
			{
				_OnTextChanged();
			}
		}
	}

	static SlotTextGenerator()
	{
		LowerCaseAscii = new Byte2(97, 122);
		UpperCaseAscii = new Byte2(65, 90);
		NumericAscii = new Byte2(48, 57);
		ValidAsciiRange = new Byte2((byte)32, (byte)126);
		LowerCaseGlyphs = GenerateGlyphSet(LowerCaseAscii);
		UpperCaseGlyphs = GenerateGlyphSet(UpperCaseAscii);
		NumericGlyphs = GenerateGlyphSet(NumericAscii);
		SpaceGlyph = GenerateGlyphSet(new Byte2((byte)32, (byte)32));
		GrammarGlyphs = new char[8] { '.', '?', '!', ':', ';', ',', '\'', '"' };
		LogicGlyphs = new char[11]
		{
			'+', '-', '/', '*', '%', '^', '=', '<', '>', '&',
			'|'
		};
		DelimiterGlyphs = new char[7] { '(', ')', '[', ']', '{', '}', '_' };
		List<char[]> glyphSets = new List<char[]> { LowerCaseGlyphs, UpperCaseGlyphs, NumericGlyphs, SpaceGlyph, GrammarGlyphs, LogicGlyphs, DelimiterGlyphs };
		MiscGlyphs = (from c in GenerateGlyphSet(ValidAsciiRange)
			where !glyphSets.Any((char[] set) => set.Contains(c))
			select c).ToArray();
		SpecificGlyphSets = new List<char[]> { GrammarGlyphs, LogicGlyphs, DelimiterGlyphs, MiscGlyphs };
	}

	private static char[] GenerateGlyphSet(Byte2 range)
	{
		char[] array = new char[range.y - range.x + 1];
		int num = 0;
		for (int i = range.x; i <= range.y; i++)
		{
			array[num++] = (char)i;
		}
		return array;
	}

	private static char[] GetGlyphSet(char c)
	{
		byte v = (byte)c;
		if (LowerCaseAscii.InRangeInclusive(v))
		{
			return LowerCaseGlyphs;
		}
		if (UpperCaseAscii.InRangeInclusive(v))
		{
			return UpperCaseGlyphs;
		}
		if (NumericAscii.InRangeInclusive(v))
		{
			return NumericGlyphs;
		}
		if (c == ' ')
		{
			return SpaceGlyph;
		}
		foreach (char[] specificGlyphSet in SpecificGlyphSets)
		{
			if (Array.IndexOf(specificGlyphSet, c) >= 0)
			{
				return specificGlyphSet;
			}
		}
		throw new ArgumentOutOfRangeException("c", $"{c} is not a supported glyph");
	}

	public static PoolStructListHandle<char> GetTransitionCharacters(char start, char end, ref int rotationDirection)
	{
		PoolStructListHandle<char> poolStructListHandle = Pools.UseStructList<char>();
		if (start == end)
		{
			return poolStructListHandle;
		}
		char[] glyphSet = GetGlyphSet(start);
		char[] glyphSet2 = GetGlyphSet(end);
		int num = Array.IndexOf(glyphSet, start);
		int num2 = Array.IndexOf(glyphSet2, end);
		bool num3 = glyphSet == glyphSet2;
		int num4 = glyphSet.Length - num + num2;
		int num5 = num + (glyphSet2.Length - num2);
		int? num6 = (num3 ? new int?(num2 - num) : null);
		int? num7 = (num6.HasValue ? new int?(Mathf.Abs(num6.Value)) : null);
		bool flag = num7.HasValue && num7.Value <= num4 && num7.Value <= num5;
		rotationDirection = (flag ? Math.Sign(num6.Value) : ((num4 < num5) ? 1 : (-1)));
		if (rotationDirection > 0)
		{
			if (flag)
			{
				for (int i = num; i <= num2; i++)
				{
					poolStructListHandle.Add(glyphSet[i]);
				}
				return poolStructListHandle;
			}
			for (int j = num; j < glyphSet.Length; j++)
			{
				poolStructListHandle.Add(glyphSet[j]);
			}
			for (int k = 0; k <= num2; k++)
			{
				poolStructListHandle.Add(glyphSet2[k]);
			}
			return poolStructListHandle;
		}
		if (flag)
		{
			for (int num8 = num; num8 >= num2; num8--)
			{
				poolStructListHandle.Add(glyphSet[num8]);
			}
			return poolStructListHandle;
		}
		for (int num9 = num; num9 >= 0; num9--)
		{
			poolStructListHandle.Add(glyphSet[num9]);
		}
		for (int num10 = glyphSet2.Length - 1; num10 >= num2; num10--)
		{
			poolStructListHandle.Add(glyphSet2[num10]);
		}
		return poolStructListHandle;
	}

	private void _OnTextChanged()
	{
		if (text.Length > slots.Count)
		{
			_text = _text.Substring(0, slots.Count - truncationText.Length) + truncationText;
		}
		_text = ((forcedTextCase == ForcedCase.None) ? _text : ((forcedTextCase == ForcedCase.LowerCase) ? _text.ToLowerInvariant() : _text.ToUpperInvariant()));
		_text = _text.ReplaceWhere((char c) => !ValidAsciiRange.InRangeInclusive(c), '~');
		int num = ((textAlignment != 0) ? ((textAlignment == TextAlignment.Center) ? Mathf.RoundToInt((float)(slots.Count - _text.Length) * 0.5f) : (slots.Count - _text.Length)) : 0);
		_pendingLetters.Clear();
		_pendingLetterIndex = 0;
		for (int i = 0; i < num; i++)
		{
			_pendingLetters.Add(new Couple<char, float>(' ', 0f));
		}
		for (int j = 0; j < _text.Length; j++)
		{
			_pendingLetters.Add(new Couple<char, float>(_text[j], (slots[j + num].letter != _text[j]) ? delayBetweenLetters : 0f));
		}
		for (int k = _text.Length + num; k < slots.Count; k++)
		{
			_pendingLetters.Add(new Couple<char, float>(' ', 0f));
		}
		if (reverseOrderOfDelayedLetters)
		{
			_pendingLetters.Reverse();
		}
		for (int l = 0; l < _pendingLetters.Count; l++)
		{
			if (_pendingLetters[l].b > 0f)
			{
				_pendingLetters[l] = new Couple<char, float>(_pendingLetters[l].a, 0f);
				break;
			}
		}
		OnTextChanged.Invoke(text);
	}

	private void Awake()
	{
		if (generateToNumberOfSlotsOnAwake)
		{
			GenerateSlots(numberOfSlots - slots.Count);
		}
	}

	private void Update()
	{
		float num = GameUtil.GetDeltaTime(useScaledTime);
		while (num > 0f && _pendingLetterIndex < _pendingLetters.Count)
		{
			Couple<char, float> couple = _pendingLetters[_pendingLetterIndex];
			Couple<char, float> value = new Couple<char, float>(couple.a, couple.b - num);
			_pendingLetters[_pendingLetterIndex] = value;
			if (!(value.b > 0f))
			{
				int index = (reverseOrderOfDelayedLetters ? (_pendingLetters.Count - 1 - _pendingLetterIndex) : _pendingLetterIndex);
				slots[index].letter = couple.a;
				if (_pendingColorChange.HasValue)
				{
					slots[index].textTint = _pendingColorChange.Value;
				}
				_pendingLetterIndex++;
				num -= couple.b;
				continue;
			}
			break;
		}
	}

	public void GenerateSlots(int numberOfSlotsToGenerate)
	{
		_initialSlotCount = _initialSlotCount ?? slots.Count;
		for (int i = 0; i < numberOfSlotsToGenerate; i++)
		{
			Transform transform = slots[slots.Count - 1].transform;
			slots.Add(UnityEngine.Object.Instantiate(slots[_slotBlueprintIndex].gameObject, nextSlotTransform.position, nextSlotTransform.rotation, base.transform).GetComponent<SlotText>());
			Transform transform2 = slots[slots.Count - 1].transform;
			nextSlotTransform.position += transform2.position - transform.position;
			_slotBlueprintIndex = (_slotBlueprintIndex + 1) % _initialSlotCount.Value;
		}
	}

	public void RequestTextColorChange(Color32 color)
	{
		pendingColorChange = color;
	}

	public void RequestTextColorChange(Color color)
	{
		pendingColorChange = color;
	}

	public void SetInt(int value)
	{
		text = value.ToString();
	}
}
