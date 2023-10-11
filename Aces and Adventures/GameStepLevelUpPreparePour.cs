public class GameStepLevelUpPreparePour : AGameStepLevelUp
{
	protected override bool _cancelStoneEnabled => true;

	protected override void _OnPointerClick(LevelUpPile pile, ATarget card)
	{
		switch (pile)
		{
		case LevelUpPile.VialPour:
		case LevelUpPile.ActiveSeal:
		case LevelUpPile.Pot:
			_OnConfirmPressed();
			return;
		case LevelUpPile.Seals:
			if (card is ClassSeal selectedSeal)
			{
				_OnBackPressed();
				AppendStep(new GameStepWait(0.25f, null, canSkip: false));
				AppendStep(new GameStepLevelUpSwapPlant(selectedSeal));
				AppendStep(new GameStepLevelUpPreparePour());
				return;
			}
			break;
		}
		if (pile == LevelUpPile.LevelUps)
		{
			AppendStep(new GameStepViewLevelUps());
		}
	}

	protected override void _OnPointerEnter(LevelUpPile pile, ATarget card)
	{
		_OnPointerEnterClassSeal(pile, card);
	}

	protected override void _OnPointerExit(LevelUpPile pile, ATarget card)
	{
		_OnPointerExitClassSeal(pile, card);
	}

	protected override void _OnBackPressed()
	{
		Cancel();
	}

	protected override void _OnVialStandClick()
	{
		_OnBackPressed();
	}

	protected override void _OnConfirmPressed()
	{
		if (base.levelUp.readExperience.CanLevelUp(base.levelUp.selectedCharacter))
		{
			TransitionTo(new GameStepLevelUpPour());
		}
		else if (base.levelUp.readExperience.CanRebirth(base.levelUp.selectedCharacter))
		{
			TransitionTo(new GameStepLevelUpPourRebirthConfirm());
		}
		else
		{
			base.view.LogError(((base.levelUp.readExperience.pendingLevelUps > 0) ? ((!IOUtil.IsDemo) ? LevelUpMessages.AlreadyMaxLevel : LevelUpMessages.DemoMaxLevel) : LevelUpMessages.InsufficientExperience).Localize());
		}
	}

	protected override void OnFirstEnabled()
	{
		base.levelUp.main.TransferPile(LevelUpPile.Vial, LevelUpPile.VialPour);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.vial.vialview.bubblePointerClick.enabled = false;
	}

	public override void Start()
	{
		if (base.quickLevelUp)
		{
			_OnConfirmPressed();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		base.vial.vialview.bubblePointerClick.enabled = true;
	}

	protected override void OnCanceled()
	{
		base.levelUp.main.TransferPile(LevelUpPile.VialPour, LevelUpPile.Vial);
	}
}
