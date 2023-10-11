using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class LocaleOverrideHook : MonoBehaviour
{
	public Locale localeOverride
	{
		get
		{
			return GetComponent<LocalizeStringEvent>()?.StringReference.LocaleOverride;
		}
		set
		{
			LocalizeStringEvent component = GetComponent<LocalizeStringEvent>();
			if ((object)component != null)
			{
				component.StringReference.LocaleOverride = value;
			}
			LocalizeTMPFontEvent component2 = GetComponent<LocalizeTMPFontEvent>();
			if ((object)component2 != null)
			{
				component2.AssetReference.LocaleOverride = value;
			}
			LocalizeMaterialEvent component3 = GetComponent<LocalizeMaterialEvent>();
			if ((object)component3 != null)
			{
				component3.AssetReference.LocaleOverride = value;
			}
		}
	}

	public Locale localeOverrideSprite
	{
		get
		{
			return GetComponent<LocalizeSpriteEvent>()?.AssetReference.LocaleOverride;
		}
		set
		{
			LocalizeSpriteEvent component = GetComponent<LocalizeSpriteEvent>();
			if ((object)component != null)
			{
				component.AssetReference.LocaleOverride = value;
			}
		}
	}
}
