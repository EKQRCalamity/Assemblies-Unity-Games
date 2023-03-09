using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CustomLanguageLayout : MonoBehaviour
{
	[Serializable]
	public struct LanguageLayout
	{
		public Localization.Languages languageApplied;

		public bool needCustomOffset;

		public Vector3 positionOffset;

		public bool needCustomWidth;

		public float customWidth;

		public bool needCustomHeight;

		public float customHeight;
	}

	[SerializeField]
	public List<LanguageLayout> customLayouts;

	private RectTransform rectTransform;

	private LanguageLayout englishBasicLayout;

	private TextContainer textContainer;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		textContainer = GetComponent<TextContainer>();
		englishBasicLayout = default(LanguageLayout);
		englishBasicLayout.positionOffset = rectTransform.localPosition;
		englishBasicLayout.customWidth = rectTransform.sizeDelta.x;
		englishBasicLayout.customHeight = rectTransform.sizeDelta.y;
	}

	private void OnDestroy()
	{
		Localization.OnLanguageChangedEvent -= ReviewLayout;
	}

	private void OnEnable()
	{
		Localization.OnLanguageChangedEvent += ReviewLayout;
		ReviewLayout();
	}

	private void OnDisable()
	{
		ResetToEnglish();
		Localization.OnLanguageChangedEvent -= ReviewLayout;
	}

	private void ReviewLayout()
	{
		if (!(rectTransform == null))
		{
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
				LanguageLayout languageLayout = customLayouts[num];
				ApplylayoutChanges(languageLayout);
			}
			else
			{
				ResetToEnglish();
			}
		}
	}

	private void ResetToEnglish()
	{
		rectTransform.localPosition = englishBasicLayout.positionOffset;
		if (textContainer != null)
		{
			textContainer.height = englishBasicLayout.customHeight;
			textContainer.width = englishBasicLayout.customWidth;
		}
		else
		{
			rectTransform.sizeDelta = new Vector2(englishBasicLayout.customWidth, englishBasicLayout.customHeight);
		}
	}

	private void ApplylayoutChanges(LanguageLayout languageLayout)
	{
		if (languageLayout.needCustomOffset)
		{
			rectTransform.localPosition = new Vector3(englishBasicLayout.positionOffset.x + languageLayout.positionOffset.x, englishBasicLayout.positionOffset.y + languageLayout.positionOffset.y, englishBasicLayout.positionOffset.z + languageLayout.positionOffset.z);
		}
		else
		{
			rectTransform.localPosition = new Vector3(englishBasicLayout.positionOffset.x, englishBasicLayout.positionOffset.y, englishBasicLayout.positionOffset.z);
		}
		if (textContainer != null)
		{
			textContainer.width = ((!languageLayout.needCustomWidth) ? englishBasicLayout.customWidth : languageLayout.customWidth);
			textContainer.height = ((!languageLayout.needCustomHeight) ? englishBasicLayout.customHeight : languageLayout.customHeight);
		}
		else
		{
			float x = ((!languageLayout.needCustomWidth) ? englishBasicLayout.customWidth : languageLayout.customWidth);
			float y = ((!languageLayout.needCustomHeight) ? englishBasicLayout.customHeight : languageLayout.customHeight);
			rectTransform.sizeDelta = new Vector2(x, y);
		}
	}
}
