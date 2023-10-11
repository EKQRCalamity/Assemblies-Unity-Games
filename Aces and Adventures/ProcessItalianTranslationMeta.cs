using System;

[Serializable]
public class ProcessItalianTranslationMeta : ProcessTranslationMeta
{
	public override bool usesDiacritics => true;
}
