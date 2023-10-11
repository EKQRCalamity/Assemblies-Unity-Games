using System;
using UnityEngine;

public class TooltipDynamicObject : TooltipGenerator
{
	public Func<GameObject> getContent { get; set; }

	protected override void _OnShowTooltip()
	{
		if (getContent != null)
		{
			base.creator.Create(getContent());
		}
	}

	protected override void _ClearTooltip()
	{
		getContent = null;
	}
}
