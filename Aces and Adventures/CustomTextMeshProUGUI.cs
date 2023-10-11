using TMPro;
using UnityEngine;

public class CustomTextMeshProUGUI : TextMeshProUGUI
{
	[SerializeField]
	protected bool _respondToParentChanges;

	public bool respondToParentChanges
	{
		get
		{
			return _respondToParentChanges;
		}
		set
		{
			_respondToParentChanges = value;
		}
	}

	protected override void OnBeforeTransformParentChanged()
	{
		if (respondToParentChanges)
		{
			base.OnBeforeTransformParentChanged();
		}
	}

	protected override void OnTransformParentChanged()
	{
		if (respondToParentChanges)
		{
			base.OnTransformParentChanged();
		}
	}

	public void SetFont(TMP_FontAsset newFont)
	{
		base.font = newFont;
	}

	public void SetMaterial(Material newMaterial)
	{
		fontSharedMaterial = newMaterial;
	}
}
