using System.Collections.Generic;
using Gameplay.UI.Others.MenuLogic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class PatchNotesElement : MonoBehaviour
{
	public Text versionText;

	public List<GameObject> newIndicators;

	public List<Text> patchNotesTexts;

	[Button(ButtonSizes.Small)]
	public void GetReferences()
	{
		Text[] componentsInChildren = base.transform.GetComponentsInChildren<Text>();
		foreach (Text text in componentsInChildren)
		{
			if (text.gameObject.name.Equals("Version"))
			{
				versionText = text;
			}
			else if (text.gameObject.name.Equals("NewIndicator") && !newIndicators.Contains(text.gameObject))
			{
				newIndicators.Add(text.gameObject);
			}
			else if (text.gameObject.name.Equals("Text") && !patchNotesTexts.Contains(text))
			{
				patchNotesTexts.Add(text);
			}
		}
	}

	public void DisplayAsSeen()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponentInParent<RectTransform>());
		newIndicators.ForEach(delegate(GameObject x)
		{
			x.SetActive(value: false);
		});
		patchNotesTexts.ForEach(delegate(Text x)
		{
			x.CrossFadeColor(PatchNotesWidget.seenPatchNotesColor, 0f, ignoreTimeScale: true, useAlpha: false);
		});
	}

	public void DisplayAsNew()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponentInParent<RectTransform>());
		newIndicators.ForEach(delegate(GameObject x)
		{
			x.SetActive(value: true);
		});
		patchNotesTexts.ForEach(delegate(Text x)
		{
			x.CrossFadeColor(PatchNotesWidget.newPatchNotesColor, 0f, ignoreTimeScale: true, useAlpha: false);
		});
	}
}
