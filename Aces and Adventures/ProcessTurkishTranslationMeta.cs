using System;

[Serializable]
public class ProcessTurkishTranslationMeta : ProcessTranslationMeta
{
	public override bool usesDiacritics => true;
}
