using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Localization.Metadata;

[Serializable]
[Metadata(AllowedTypes = MetadataType.Locale)]
public class ProcessTranslationMeta : IMetadata
{
	[Serializable]
	public struct TextMap
	{
		public string original;

		public string mapTo;

		public TextMap(string original, string mapTo)
		{
			this.original = original;
			this.mapTo = mapTo;
		}
	}

	public static ProcessTranslationMeta Default = new ProcessTranslationMeta();

	public char[] preserveSpacingBefore = new char[6] { ']', '.', '!', '-', ',', '?' };

	public char[] preserveSpacingAfter = new char[4] { '\n', '[', '(', '>' };

	public char[] preserveSpacingBeforeSmartText = new char[3] { '|', '(', ':' };

	public char[] preserveSpacingAfterSmartText = new char[4] { ':', '+', '}', '|' };

	public TextMap[] mapText;

	public TextMap[] mapSmartText;

	private Dictionary<string, string> _textMap;

	private Dictionary<string, string> _smartTextMap;

	private Dictionary<string, string> textMap
	{
		get
		{
			Dictionary<string, string> dictionary = _textMap;
			if (dictionary == null)
			{
				IEnumerable<TextMap> first = _DefaultTextMaps();
				IEnumerable<TextMap> enumerable = mapText;
				dictionary = (_textMap = first.Concat(enumerable ?? Enumerable.Empty<TextMap>()).ToDictionarySafe((TextMap t) => t.original, (TextMap t) => t.mapTo));
			}
			return dictionary;
		}
	}

	private Dictionary<string, string> smartTextMap
	{
		get
		{
			Dictionary<string, string> dictionary = _smartTextMap;
			if (dictionary == null)
			{
				IEnumerable<TextMap> first = _DefaultSmartTextMaps();
				IEnumerable<TextMap> enumerable = mapSmartText;
				dictionary = (_smartTextMap = first.Concat(enumerable ?? Enumerable.Empty<TextMap>()).ToDictionarySafe((TextMap t) => t.original, (TextMap t) => t.mapTo));
			}
			return dictionary;
		}
	}

	public virtual bool usesDiacritics => false;

	protected virtual string _ProcessTranslation(string sourceText, string translatedText)
	{
		return translatedText;
	}

	protected virtual string _ProcessSmartTranslation(string sourceText, string translatedText)
	{
		return translatedText;
	}

	protected virtual IEnumerable<TextMap> _DefaultTextMaps()
	{
		return Enumerable.Empty<TextMap>();
	}

	protected virtual IEnumerable<TextMap> _DefaultSmartTextMaps()
	{
		return Enumerable.Empty<TextMap>();
	}

	public string ProcessTranslation(string sourceText, string translatedText)
	{
		if (textMap.Count > 0)
		{
			translatedText = translatedText.ReplaceManyWords(textMap, StringComparison.Ordinal, (char c) => true, (char c) => true);
		}
		char[] array = preserveSpacingAfter;
		foreach (char character in array)
		{
			translatedText = LocalizationUtil.ProcessSpacingAfterCharacter(sourceText, translatedText, character);
		}
		array = preserveSpacingBefore;
		foreach (char character2 in array)
		{
			translatedText = LocalizationUtil.ProcessSpacingBeforeCharacter(sourceText, translatedText, character2);
		}
		translatedText = translatedText.CopyTrimming(sourceText);
		return _ProcessTranslation(sourceText, translatedText);
	}

	public string ProcessSmartTranslation(string sourceText, string translatedText)
	{
		translatedText = translatedText.InsureBracketClosures();
		if (smartTextMap.Count > 0)
		{
			translatedText = translatedText.ProcessTextBetween((string input) => input.ReplaceManyWords(smartTextMap, StringComparison.Ordinal, (char c) => true, (char c) => true));
		}
		char[] array = preserveSpacingAfterSmartText;
		foreach (char character in array)
		{
			translatedText = LocalizationUtil.ProcessSpacingAfterCharacter(sourceText, translatedText, character);
		}
		array = preserveSpacingBeforeSmartText;
		foreach (char character2 in array)
		{
			translatedText = LocalizationUtil.ProcessSpacingBeforeCharacter(sourceText, translatedText, character2);
		}
		translatedText = LocalizationUtil.RemoveInvalidSmartBraces(sourceText, translatedText);
		translatedText = LocalizationUtil.ValidateSmartTextFunctionParameters(sourceText, translatedText);
		return _ProcessSmartTranslation(sourceText, translatedText);
	}
}
