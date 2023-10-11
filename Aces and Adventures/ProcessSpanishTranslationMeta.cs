using System;

[Serializable]
public class ProcessSpanishTranslationMeta : ProcessTranslationMeta
{
	public override bool usesDiacritics => true;

	protected override string _ProcessTranslation(string sourceText, string translatedText)
	{
		return translatedText.Replace("¡ ", "¡");
	}
}
