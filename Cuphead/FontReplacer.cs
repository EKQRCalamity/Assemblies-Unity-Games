using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FontReplacer : MonoBehaviour
{
	public Localization localizationAsset;

	public Localization.Languages sourceLanguage;

	public Localization.Languages destinationLanguage;

	public List<Font> allSourceFonts;

	public List<Font> allDestinationFonts;

	public List<TMP_FontAsset> allSourceFontAssets;

	public List<TMP_FontAsset> allDestinationFontAssets;

	private void Awake()
	{
		Object.Destroy(base.gameObject);
	}
}
