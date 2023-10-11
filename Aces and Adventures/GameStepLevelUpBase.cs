public class GameStepLevelUpBase : AGameStepLevelUp
{
	protected override bool _cancelStoneEnabled => true;

	private void _TransitionToPourStep()
	{
		AppendStep(new GameStepLevelUpPreparePour());
	}

	private void _OnRest(LevelUpPile pile, ATarget card)
	{
		switch (pile)
		{
		case LevelUpPile.VialDiscard:
		case LevelUpPile.PotDiscard:
		case LevelUpPile.DiscardLevelUp:
			card.view.RepoolCard();
			break;
		case LevelUpPile.LeafExit:
			if (card is LevelUpLeaf)
			{
				card.view.RepoolCard();
			}
			break;
		}
	}

	private void _OnPointerHeld(ACardLayout layout, CardLayoutElement card)
	{
		if (base.quickLevelUp && (card.card is ClassSeal || card.card is LevelUpPlant))
		{
			_TransitionToPourStep();
		}
	}

	protected override void _OnPointerClick(LevelUpPile pile, ATarget card)
	{
		switch (pile)
		{
		case LevelUpPile.ActiveSeal:
		case LevelUpPile.Pot:
			_TransitionToPourStep();
			break;
		case LevelUpPile.Seals:
			AppendStep(new GameStepLevelUpSwapPlant(card as ClassSeal));
			break;
		case LevelUpPile.LevelUps:
			AppendStep(new GameStepViewLevelUps());
			break;
		}
	}

	protected override void _OnVialStandClick()
	{
		_TransitionToPourStep();
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

	protected override void _OnConfirmPressed()
	{
		_TransitionToPourStep();
	}

	protected override void OnFirstEnabled()
	{
		base.levelUpView.main.onRest += _OnRest;
		base.levelUpView.levelAmount = base.levelUp.readExperience.pendingLevelUps;
		base.levelUpView.levelDisplayOpen = true;
		base.levelUpView.main.GetLayout(LevelUpPile.Seals).EnableInput();
		AppendStep(new GameStepLevelUpSwapCards(base.levelUp.selectedCharacter, 0.25f));
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.levelUpView.bubblePotHolderPointerClickTo = base.plant.view.gameObject;
		ACardLayout.OnPointerHeld += _OnPointerHeld;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ACardLayout.OnPointerHeld -= _OnPointerHeld;
	}

	protected override void OnCanceled()
	{
		base.view.buttonDeckLayout.SetActive(ButtonCardType.Back, setActive: false);
		base.manager.stack.Push(new GameStepLevelUpSwapCards(null));
		base.manager.stack.Push(new GameStepWaitFrame());
		base.manager.stack.Push(new GameStepWaitForLayoutRest(base.view.stoneDeckLayout.cancelInactive));
		base.manager.stack.Push(new GameStepGenericSimple(delegate
		{
			base.state.stoneDeck.GetCardsSafe().AsEnumerable().EffectAll(delegate(Stone stone)
			{
				stone.view.DestroyCard();
			});
		}));
		base.manager.stack.Push(new GameStepTimelineReverse(base.manager.establishShotToLevelUp, base.manager.adventureLookAt, 0.8f));
		base.manager.stack.Push(new GameStepEstablishShot());
	}

	protected override void OnDestroy()
	{
		base.levelUpView.main.onRest -= _OnRest;
		base.levelUpView.levelDisplayOpen = false;
		base.levelUpView.main.GetLayout(LevelUpPile.Seals).DisableInput();
		base.state.stack.Unregister();
	}
}
