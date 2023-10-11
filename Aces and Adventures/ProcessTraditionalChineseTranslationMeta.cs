using System;
using System.Collections.Generic;

[Serializable]
public class ProcessTraditionalChineseTranslationMeta : ProcessTranslationMeta
{
	private static readonly HashSet<char> _KeepSpacesAfter = new HashSet<char> { '。', '？', '！', '；' };

	protected override string _ProcessTranslation(string sourceText, string translatedText)
	{
		return translatedText.RemoveWhiteSpacesExceptThoseAfter(_KeepSpacesAfter);
	}
}
