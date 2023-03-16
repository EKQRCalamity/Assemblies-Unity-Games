using UnityEngine.UI;

public class TextAutoLocalize : Text
{
	public override string text
	{
		get
		{
			return base.text;
		}
		set
		{
			TranslationElement translationElement = Localization.Find(value);
			base.text = ((translationElement == null) ? value : translationElement.translations[(int)Localization.language].text);
		}
	}
}
