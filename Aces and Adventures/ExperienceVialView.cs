using UnityEngine;

public class ExperienceVialView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/Rewards/ExperienceVialView";

	[SerializeField]
	protected bool _topCorkEnabled = true;

	[SerializeField]
	protected bool _bottomCorkEnabled = true;

	[Header("Experience Vial")]
	public BoolEvent onTopCorkEnabledChange;

	public BoolEvent onBottomCorkEnabledChange;

	public BoolEvent onShouldSimulateLiquidChange;

	public BoolEvent onIsFillingChange;

	private BarFiller _barFiller;

	private BubblePointerClick3D _bubblePointerClick;

	private bool _isFilling;

	public BarFiller barFiller => this.CacheComponentInChildren(ref _barFiller);

	public BubblePointerClick3D bubblePointerClick => this.CacheComponentInChildren(ref _bubblePointerClick);

	public bool topCorkEnabled
	{
		get
		{
			return _topCorkEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _topCorkEnabled, value))
			{
				onTopCorkEnabledChange?.Invoke(value);
			}
		}
	}

	public bool bottomCorkEnabled
	{
		get
		{
			return _bottomCorkEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _bottomCorkEnabled, value))
			{
				onBottomCorkEnabledChange?.Invoke(value);
			}
		}
	}

	public bool isFilling
	{
		get
		{
			return _isFilling;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _isFilling, value))
			{
				onIsFillingChange?.Invoke(value);
			}
		}
	}

	public static ExperienceVialView Create(ExperienceVial vial, Transform parent = null)
	{
		return DirtyPools.Unpool(Blueprint, parent).GetComponent<ExperienceVialView>().SetData(vial) as ExperienceVialView;
	}

	private void _OnExperienceChange(int previous, int current)
	{
		barFiller.value = current;
	}

	private void _OnShouldSimulateLiquidChange(bool shouldSimulate)
	{
		onShouldSimulateLiquidChange?.Invoke(shouldSimulate);
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (newTarget is ExperienceVial experienceVial)
		{
			barFiller.max = 100f;
			barFiller.value = (int)experienceVial.experience;
			_OnShouldSimulateLiquidChange(experienceVial.shouldSimulateLiquid);
			experienceVial.shouldSimulateLiquid.onValueChanged += _OnShouldSimulateLiquidChange;
			experienceVial.experience.onValueChanged += _OnExperienceChange;
		}
	}
}
