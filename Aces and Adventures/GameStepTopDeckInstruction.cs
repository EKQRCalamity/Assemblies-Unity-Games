using System.Collections.Generic;

public abstract class GameStepTopDeckInstruction : GameStep
{
	protected ActionContext _context;

	private PoolKeepItemListHandle<ResourceCard> _previousTopDeckHand;

	protected virtual List<AAction.Condition.Combatant> _drawFromTargetConditions => null;

	public abstract TopDeckInstruction instruction { get; }

	public GameStepTopDeckInstruction(ActionContext context)
	{
		_context = context;
	}

	protected abstract IEnumerable<ActionContextTarget> _DrawFromTargets();

	protected override void OnFirstEnabled()
	{
		foreach (ActionContextTarget item in _DrawFromTargets())
		{
			ACombatant target = _context.GetTarget<ACombatant>(item);
			if (target != null && _drawFromTargetConditions.All(_context.SetTarget(target)))
			{
				GameStepGroup gameStepGroup = base.contextGroup;
				if (gameStepGroup == null || !gameStepGroup.HasHashTag(target.ToId<ATarget>()))
				{
					continue;
				}
			}
			CancelGroup();
			return;
		}
		_previousTopDeckHand = Pools.UseKeepItemList<ResourceCard>();
		foreach (ActionContextTarget item2 in _DrawFromTargets())
		{
			foreach (ResourceCard card in _context.GetTarget<ACombatant>(item2).resourceDeck.GetCards(ResourceCard.Pile.TopDeckHand))
			{
				_previousTopDeckHand.value.AddUnique(card);
			}
		}
		foreach (ResourceCard item3 in _previousTopDeckHand.value)
		{
			item3.deck.Transfer(item3, ResourceCard.Pile.ActivationHandWaiting).view.ClearTransitions();
		}
	}

	public void RestorePreviousTopDeckHands()
	{
		if (!_previousTopDeckHand)
		{
			return;
		}
		foreach (ResourceCard item in _previousTopDeckHand)
		{
			item.deck.Transfer(item, ResourceCard.Pile.TopDeckHand).view.ClearTransitions();
		}
	}
}
