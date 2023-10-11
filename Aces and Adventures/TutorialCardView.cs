using TMPro;
using UnityEngine;

public class TutorialCardView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/TutorialCardView";

	[Header("Tutorial")]
	public TextMeshProUGUI descriptionText;

	public StringEvent onNameChange;

	public StringEvent onDescriptionChange;

	public TutorialCard tutorial
	{
		get
		{
			return (TutorialCard)base.target;
		}
		set
		{
			base.target = value;
		}
	}

	public static TutorialCardView Create(TutorialCard tutorial, Transform parent = null)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<TutorialCardView>()._SetData(tutorial);
	}

	private TutorialCardView _SetData(TutorialCard tutorialCard)
	{
		tutorial = tutorialCard;
		return this;
	}

	private void _OnCardChanged()
	{
		onNameChange?.InvokeLocalized(this, () => tutorial?.data.name);
		onDescriptionChange?.InvokeLocalized(this, () => tutorial?.data.description);
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (newTarget != null)
		{
			_OnCardChanged();
		}
	}
}
