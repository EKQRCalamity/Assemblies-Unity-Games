using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class ProcessJapaneseTranslationMeta : ProcessTranslationMeta
{
	private static readonly HashSet<char> _KeepSpacesAfter = new HashSet<char> { '。', '？', '！', '；' };

	private static readonly Dictionary<string, string> _SmartReplaceMap = new Dictionary<string, string>
	{
		{ "）", ")" },
		{ "：", ":" },
		{ "（", "(" },
		{ "＃", "#" }
	};

	protected override IEnumerable<TextMap> _DefaultSmartTextMaps()
	{
		return _SmartReplaceMap.Select((KeyValuePair<string, string> p) => new TextMap(p.Key, p.Value));
	}

	protected override string _ProcessTranslation(string sourceText, string translatedText)
	{
		return translatedText.RemoveWhiteSpacesExceptThoseAfter(_KeepSpacesAfter);
	}
}
