using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipText : TooltipGenerator
{
	public bool backgroundEnabled = true;

	public TextAlignmentOptions alignment = TextAlignmentOptions.Center;

	[Multiline(10)]
	public string richText;

	public Func<string> dynamicText;

	private string _textOfLastCreated;

	protected override void _OnShowTooltip()
	{
		if (dynamicText != null)
		{
			richText = dynamicText();
		}
		if (!(richText == _textOfLastCreated) && !richText.IsNullOrEmpty())
		{
			_textOfLastCreated = richText;
			GameObject gameObject = Pools.Unpool(TooltipCreator.TooltipTextBlueprint);
			TextMeshProUGUI componentInChildren = gameObject.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
			componentInChildren.text = richText;
			componentInChildren.alignment = alignment;
			gameObject.GetComponent<Image>().enabled = backgroundEnabled;
			base.creator.Create(gameObject);
		}
	}

	public override void OnHideTooltip()
	{
		base.OnHideTooltip();
		_textOfLastCreated = null;
	}

	protected override void _ClearTooltip()
	{
		richText = "";
		dynamicText = null;
	}

	public void SetText(string text)
	{
		richText = text;
	}
}
