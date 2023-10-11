using System;
using System.Collections.Generic;

[Serializable]
public class ProcessSimplifiedChineseTranslationMeta : ProcessTranslationMeta
{
	private static readonly HashSet<char> _KeepSpacesAfter = new HashSet<char> { '。', '？', '！', '；' };

	protected override string _ProcessTranslation(string sourceText, string translatedText)
	{
		translatedText = LocalizationUtil.ProcessSpacingBeforeCharacter(sourceText, translatedText, '。', '.');
		translatedText = LocalizationUtil.ProcessSpacingBeforeCharacter(sourceText, translatedText, '，', ',');
		translatedText = LocalizationUtil.ProcessSpacingBeforeCharacter(sourceText, translatedText, '；', ';');
		translatedText = translatedText.RemoveWhiteSpacesExceptThoseAfter(_KeepSpacesAfter);
		return translatedText;
	}
}
