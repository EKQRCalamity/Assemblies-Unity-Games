using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomLanguageFont : MonoBehaviour
{
	[Serializable]
	public struct LanguageFont
	{
		public Localization.Languages languageApplied;

		public bool needSpacing;

		public float characterSpacing;

		public float lineSpacing;

		public float paragraphSpacing;

		public bool needFontSize;

		public float customFontSize;

		public bool needKerning;
	}

	[SerializeField]
	public List<LanguageFont> customLayouts;

	private LanguageFont englishBasicFont;

	private TMP_Text textMeshProComponent;

	private Text textComponent;

	private void Awake()
	{
		textMeshProComponent = GetComponent<TMP_Text>();
		textComponent = GetComponent<Text>();
		englishBasicFont = default(LanguageFont);
		englishBasicFont.characterSpacing = textMeshProComponent.characterSpacing;
		englishBasicFont.lineSpacing = textMeshProComponent.lineSpacing;
		englishBasicFont.paragraphSpacing = textMeshProComponent.paragraphSpacing;
		englishBasicFont.needFontSize = true;
		englishBasicFont.customFontSize = textMeshProComponent.fontSize;
		englishBasicFont.needKerning = textMeshProComponent.enableKerning;
	}

	private void Start()
	{
		Localization.OnLanguageChangedEvent += ReviewFont;
		ReviewFont();
	}

	private void OnDestroy()
	{
		Localization.OnLanguageChangedEvent -= ReviewFont;
	}

	private void OnEnable()
	{
		ReviewFont();
	}

	private void ReviewFont()
	{
		if (textMeshProComponent == null && textComponent == null)
		{
			return;
		}
		int num = 0;
		bool flag = false;
		while (!flag && num < customLayouts.Count)
		{
			flag = customLayouts[num].languageApplied == Localization.language;
			num++;
		}
		num--;
		if (flag)
		{
			if (customLayouts[num].needSpacing)
			{
				ApplySpacingChanges(customLayouts[num]);
			}
			if (customLayouts[num].needFontSize)
			{
				ApplyFontSizeChanges(customLayouts[num]);
			}
			textMeshProComponent.enableKerning = customLayouts[num].needKerning;
		}
		else
		{
			ApplySpacingChanges(englishBasicFont);
			ApplyFontSizeChanges(englishBasicFont);
			textMeshProComponent.enableKerning = englishBasicFont.needKerning;
		}
	}

	private void ApplySpacingChanges(LanguageFont languageLayout)
	{
		if (textMeshProComponent != null)
		{
			textMeshProComponent.characterSpacing = languageLayout.characterSpacing;
			textMeshProComponent.lineSpacing = languageLayout.lineSpacing;
			textMeshProComponent.paragraphSpacing = languageLayout.paragraphSpacing;
		}
		else
		{
			textComponent.lineSpacing = languageLayout.lineSpacing;
		}
	}

	private void ApplyFontSizeChanges(LanguageFont languageLayout)
	{
		if (textMeshProComponent != null)
		{
			textMeshProComponent.fontSize = languageLayout.customFontSize;
		}
		else
		{
			textComponent.fontSize = (int)languageLayout.customFontSize;
		}
	}
}
