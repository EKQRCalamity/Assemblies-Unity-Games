[UIField("Translation", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Determines how text will be translated automatically.\nSet to manual or simply m in order to signify that an entry has been translated by hand and should no longer update automatically.")]
public enum TranslationMode
{
	Auto,
	Manual,
	Parsed,
	Simple,
	Context,
	Alias,
	AliasContext,
	LanguageName,
	Replace,
	ReverseTranslate
}
