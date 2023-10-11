using System;

[Serializable]
public class ProcessGermanTranslationMeta : ProcessTranslationMeta
{
	public override bool usesDiacritics => true;
}
