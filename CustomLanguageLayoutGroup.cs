using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomLanguageLayoutGroup : MonoBehaviour
{
	[Serializable]
	public struct LanguageLayoutGroup
	{
		public Localization.Languages languageApplied;

		public bool needPadding;

		public RectOffset padding;

		public bool needSpacing;

		public float spacing;
	}

	[SerializeField]
	private HorizontalOrVerticalLayoutGroup layoutComponent;

	[SerializeField]
	public List<LanguageLayoutGroup> customLayouts;

	private LanguageLayoutGroup englishBasicLayout;

	private void Awake()
	{
		englishBasicLayout = default(LanguageLayoutGroup);
		englishBasicLayout.needPadding = true;
		englishBasicLayout.padding = layoutComponent.padding;
		englishBasicLayout.needSpacing = true;
		englishBasicLayout.spacing = layoutComponent.spacing;
	}

	private void Start()
	{
		Localization.OnLanguageChangedEvent += ReviewLayout;
	}

	private void OnDestroy()
	{
		Localization.OnLanguageChangedEvent -= ReviewLayout;
	}

	private void OnEnable()
	{
		ReviewLayout();
	}

	private void ReviewLayout()
	{
		if (layoutComponent == null)
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
			if (customLayouts[num].needPadding)
			{
				ApplyPaddingChanges(customLayouts[num]);
			}
		}
		else
		{
			ApplySpacingChanges(englishBasicLayout);
			ApplyPaddingChanges(englishBasicLayout);
		}
	}

	private void ApplySpacingChanges(LanguageLayoutGroup languageLayout)
	{
		layoutComponent.spacing = languageLayout.spacing;
	}

	private void ApplyPaddingChanges(LanguageLayoutGroup languageLayout)
	{
		layoutComponent.padding = languageLayout.padding;
	}
}
