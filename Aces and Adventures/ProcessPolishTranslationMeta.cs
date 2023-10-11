using System;

[Serializable]
public class ProcessPolishTranslationMeta : ProcessTranslationMeta
{
	public override bool usesDiacritics => true;
}
