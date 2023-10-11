using UnityEngine;

public class ButtonCardView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/ButtonCardView";

	[Header("Button")]
	public StringEvent onLabelChange;

	public ButtonCard button
	{
		get
		{
			return (ButtonCard)base.target;
		}
		set
		{
			base.target = value;
		}
	}

	public static ButtonCardView Create(ButtonCard card, Transform parent = null)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<ButtonCardView>()._SetData(card);
	}

	private ButtonCardView _SetData(ButtonCard buttonCard)
	{
		button = buttonCard;
		return this;
	}

	private void _OnCardChanged()
	{
		onLabelChange?.Invoke(EnumUtil.FriendlyName(button.type));
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (newTarget != null)
		{
			_OnCardChanged();
		}
	}
}
