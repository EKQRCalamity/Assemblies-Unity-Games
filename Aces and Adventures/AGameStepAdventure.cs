using System;
using System.Linq;
using UnityEngine;

public abstract class AGameStepAdventure : GameStep
{
	private static SoundPack.SoundData _SoundData = new SoundPack.SoundData();

	private AdventureTargetView _autoNarratedCard;

	private VoiceSource _activeNarration;

	private TextMeshTyper _activeTyper;

	private bool _hasPlayedNarration;

	protected IdDeck<AdventureCard.Pile, ATarget> adventureDeck => base.state.adventureDeck;

	public override bool canSafelyCancelStack => true;

	private void _OnAdventureClick(AdventureCard.Pile pile, ATarget card)
	{
		if (pile == AdventureCard.Pile.SelectionHand && card is IAdventureCard card2)
		{
			_OnAdventureCardSelected(card2);
		}
	}

	private void _OnAdventureOver(AdventureCard.Pile pile, ATarget card)
	{
		if (pile == AdventureCard.Pile.SelectionHand && card is IAdventureCard card2)
		{
			_OnAdventureCardOver(card2);
		}
	}

	private void _OnAdventureExit(AdventureCard.Pile pile, ATarget card)
	{
		if (pile == AdventureCard.Pile.SelectionHand && card is IAdventureCard card2)
		{
			_OnAdventureCardExit(card2);
		}
	}

	private void _PlayNarration(IAdventureCard card)
	{
		_hasPlayedNarration = true;
		if (card is ItemCard itemCard && itemCard.isFoundItem)
		{
			_activeNarration = VoiceManager.Instance.Play(base.state.player.view.transform, base.state.player.audio.character.findItem, interrupt: true);
		}
		else if ((bool)card.adventureCardCommon.narration)
		{
			_activeNarration = VoiceManager.Instance.Play(_SoundData.SetAudioRef(card.adventureCardCommon.narration), interrupt: true, 0f, isGlobal: false, MasterMixManager.Narration);
		}
		if ((bool)_activeTyper)
		{
			_activeTyper.StartTyping(_activeNarration ? ((Func<AudioSource>)(() => _activeNarration.source)) : null, card.adventureCardCommon.narrationStart, card.adventureCardCommon.narrationDurationAdjustment, (bool)_activeNarration && ProfileManager.options.game.ui.syncNarrativeText);
		}
	}

	private void _StopNarration()
	{
		_hasPlayedNarration = false;
		_activeNarration.StopAndClear(ref _activeNarration);
		if ((bool)_autoNarratedCard)
		{
			_autoNarratedCard.descriptionText.GetComponent<TextMeshTyper>().DisableAll();
			_autoNarratedCard.offsets.Clear();
		}
	}

	protected TextMeshTyper _GetActiveTyper(IAdventureCard card)
	{
		if (card.adventureCard.view is AdventureTargetView adventureTargetView)
		{
			TextMeshTyper component = adventureTargetView.descriptionText.GetComponent<TextMeshTyper>();
			if ((object)component != null && !component.isFinished)
			{
				return component;
			}
		}
		return null;
	}

	protected bool _FinishTyperIfNeeded(IAdventureCard card)
	{
		TextMeshTyper textMeshTyper = _GetActiveTyper(card);
		if ((object)textMeshTyper != null)
		{
			textMeshTyper.Finish();
			return true;
		}
		return false;
	}

	protected virtual void _OnAdventureCardSelected(IAdventureCard card)
	{
		if (_FinishTyperIfNeeded(card))
		{
			return;
		}
		if (card is ItemCard itemCard && itemCard.hasCost)
		{
			VoiceManager.Instance.Play(base.state.player.view.transform, base.state.player.audio.character.buyItem, interrupt: true);
		}
		PoolKeepItemListHandle<GameStep> poolKeepItemListHandle = Pools.UseKeepItemList(card.adventureCardCommon.selectInstructions.SelectMany((AdventureCard.SelectInstruction instruction) => instruction.GetGameSteps(base.state)));
		int num = poolKeepItemListHandle.value.LastIndexOf((GameStep step) => (step is GameStepActionAct gameStepActionAct && gameStepActionAct.action.requiresUserInput) || step is AdventureCard.SelectInstruction.TopDeck.ConditionInstructionStep);
		if (num >= 0)
		{
			poolKeepItemListHandle.value.Insert(0, new GameStepGenericSimple(delegate
			{
				base.view.adventureDeckLayout.SetLayout(AdventureCard.Pile.SelectionHand, base.view.playerAbilityDeckLayout.activationHand);
			}));
		}
		poolKeepItemListHandle.value.Insert((num >= 0) ? (num + 2) : 0, card.selectTransferStep);
		PoolKeepItemListHandle<GameStep> selectedGameSteps = null;
		if (num >= 0)
		{
			poolKeepItemListHandle.value.Insert(num + 3, new GameStepGenericDestroy(delegate
			{
				base.view.adventureDeckLayout.RestoreLayoutToDefault(AdventureCard.Pile.SelectionHand);
				Pools.Repool(ref selectedGameSteps);
			}, delegate
			{
				base.state.adventureDeck.TransferPile(AdventureCard.Pile.SelectionHand, AdventureCard.Pile.ActiveHand);
				if ((bool)selectedGameSteps)
				{
					foreach (GameStep item in selectedGameSteps.value)
					{
						item.Cancel();
					}
				}
			}));
		}
		AppendGroup(new GameStepGrouper(poolKeepItemListHandle.AsEnumerable(), changesContext: true));
		foreach (GameStep item2 in (selectedGameSteps = Pools.UseKeepItemList(card.selectedGameSteps)).value)
		{
			AppendStep(item2);
		}
	}

	protected virtual void _OnAdventureCardOver(IAdventureCard card)
	{
		int num = adventureDeck.Count(AdventureCard.Pile.SelectionHand);
		if (num > 1 && card.adventureCard.view.pointerOver.enabled)
		{
			_PlayNarration(card);
		}
		else if (num == 1 && card is ItemCard itemCard && itemCard.isFoundItem)
		{
			_PlayNarration(card);
		}
	}

	protected virtual void _OnAdventureCardExit(IAdventureCard card)
	{
		if (adventureDeck.Count(AdventureCard.Pile.SelectionHand) > 1)
		{
			_StopNarration();
		}
	}

	private void _OnConfirmPressed()
	{
		if ((bool)_autoNarratedCard)
		{
			_OnAdventureClick(AdventureCard.Pile.SelectionHand, _autoNarratedCard.adventureCard.adventureCard);
		}
	}

	protected override void OnEnable()
	{
		adventureDeck.layout.onPointerClick += _OnAdventureClick;
		adventureDeck.layout.onPointerEnter += _OnAdventureOver;
		adventureDeck.layout.onPointerExit += _OnAdventureExit;
		base.view.onConfirmPressed += _OnConfirmPressed;
		_autoNarratedCard = ((adventureDeck.Count(AdventureCard.Pile.SelectionHand) == 1 && adventureDeck.NextInPile(AdventureCard.Pile.SelectionHand) is IAdventureCard adventureCard && adventureCard.adventureCard.view is AdventureTargetView adventureTargetView && (bool)adventureTargetView.adventureCard?.adventureCardCommon?.narration) ? adventureTargetView : null);
		if ((bool)_autoNarratedCard)
		{
			if (!_hasPlayedNarration && !_activeTyper)
			{
				_autoNarratedCard.descriptionText.RecalculateAutoFontSize();
				_activeTyper = _autoNarratedCard.descriptionText.gameObject.GetOrAddComponent<TextMeshTyper>().Stop();
			}
			Transform transform = base.view.adventureDeckLayout.GetLayout(AdventureCard.Pile.SelectionHand).transform;
			if ((object)transform != null)
			{
				_autoNarratedCard.offsets.Add(Matrix4x4.Translate(transform.up * 0.06f + transform.forward * -0.003f));
			}
		}
	}

	protected override void LateUpdate()
	{
		if ((bool)_autoNarratedCard)
		{
			if (!_hasPlayedNarration && _autoNarratedCard.atRestInLayout)
			{
				_PlayNarration(_autoNarratedCard.adventureCard);
			}
			if ((bool)_activeTyper && _activeTyper.isFinished)
			{
				_autoNarratedCard.RequestGlow(this, Colors.TARGET);
				_activeTyper = null;
			}
		}
	}

	protected override void OnDisable()
	{
		adventureDeck.layout.onPointerClick -= _OnAdventureClick;
		adventureDeck.layout.onPointerEnter -= _OnAdventureOver;
		adventureDeck.layout.onPointerExit -= _OnAdventureExit;
		base.view.onConfirmPressed -= _OnConfirmPressed;
		_StopNarration();
	}
}
