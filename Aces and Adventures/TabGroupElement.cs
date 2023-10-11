using UnityEngine;

internal class TabGroupElement
{
	public CanvasGroup cg;

	public TabGroupElement(CanvasGroup cg)
	{
		this.cg = cg;
	}

	public void OnValueChanged(bool isOn)
	{
		cg.alpha = (isOn ? 1 : 0);
		cg.blocksRaycasts = isOn;
	}
}
