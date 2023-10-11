using System;
using System.Collections.Generic;

[Serializable]
public class ProcessFrenchTranslationMeta : ProcessTranslationMeta
{
	private static readonly Dictionary<string, string> _ReplaceMap = new Dictionary<string, string>
	{
		{ "d' ", "d'" },
		{ "l' ", "l'" },
		{ "D' ", "D'" },
		{ "L' ", "L'" }
	};

	public override bool usesDiacritics => true;

	protected override string _ProcessTranslation(string sourceText, string translatedText)
	{
		return translatedText.ReplaceManyWords(_ReplaceMap, StringComparison.Ordinal, null, (char c) => true);
	}
}
