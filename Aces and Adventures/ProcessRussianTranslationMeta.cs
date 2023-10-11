using System;

[Serializable]
public class ProcessRussianTranslationMeta : ProcessTranslationMeta
{
	public override bool usesDiacritics => true;
}
