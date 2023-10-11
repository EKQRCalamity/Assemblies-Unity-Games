using System.Collections;
using UnityEngine;

public abstract class AGameStepPresentResult : GameStep
{
	protected ATarget _card;

	private GameStepProjectileMedia _media;

	private bool _done;

	protected abstract ProjectileMediaPack _clickMedia { get; }

	protected virtual bool _shouldHaveOffset => true;

	protected AGameStepPresentResult(ATarget card)
	{
		_card = card;
	}

	protected virtual void _OnClick(AdventureCard.Pile pile, ATarget card)
	{
		if (card == _card)
		{
			if (_card.view.hasOffset && _media == null)
			{
				base.state.stack.ParallelProcess(_media = new GameStepProjectileMedia(_clickMedia, new ActionContext(base.state.player, null, _card)));
			}
			if (_card.view.hasOffset)
			{
				_card.view.offsets.Clear();
			}
			else
			{
				_done = true;
			}
		}
	}

	protected virtual void _OnPointerEnter(AdventureCard.Pile pile, ATarget card)
	{
		if (card == _card)
		{
			card.view.RequestGlow(this, Colors.TARGET);
		}
	}

	protected virtual void _OnPointerExit(AdventureCard.Pile pile, ATarget card)
	{
		if (card == _card)
		{
			card.view.ReleaseGlow(this);
		}
	}

	protected virtual void _OnConfirm()
	{
		_OnClick(AdventureCard.Pile.SelectionHand, _card);
	}

	protected override void OnFirstEnabled()
	{
		base.state.exileDeck.Add(_card, ExilePile.Character);
		if (_shouldHaveOffset)
		{
			_card.view.offsets.Add(Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, 180f)));
			base.state.exileDeck.layout.GetLayout(ExilePile.Character).ForceFinishLayoutAnimations();
		}
		base.state.adventureDeck.Transfer(_card, AdventureCard.Pile.SelectionHand);
		_card.view.ClearEnterTransitions();
	}

	protected override void OnEnable()
	{
		base.state.adventureDeck.layout.onPointerClick += _OnClick;
		base.state.adventureDeck.layout.onPointerEnter += _OnPointerEnter;
		base.state.adventureDeck.layout.onPointerExit += _OnPointerExit;
		base.view.onConfirmPressed += _OnConfirm;
		base.view.onBackPressed += _OnConfirm;
	}

	public override void Start()
	{
		if (_shouldHaveOffset)
		{
			AppendStep(new GameStepWait(0.333f, null, canSkip: false));
		}
	}

	protected override IEnumerator Update()
	{
		while (!_done)
		{
			yield return null;
		}
		while (true)
		{
			GameStepProjectileMedia media = _media;
			if (media != null && !media.finished)
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	protected override void OnDisable()
	{
		base.state.adventureDeck.layout.onPointerClick -= _OnClick;
		base.state.adventureDeck.layout.onPointerEnter -= _OnPointerEnter;
		base.state.adventureDeck.layout.onPointerExit -= _OnPointerExit;
		base.view.onConfirmPressed -= _OnConfirm;
		base.view.onBackPressed -= _OnConfirm;
	}

	protected override void OnFinish()
	{
		base.state.rewardDeck.Transfer(_card, RewardPile.Results);
	}
}
