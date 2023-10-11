using System.Collections;

public class GameStepTurnEnemyPrepareAttack : AGameStepTurn
{
	protected ACombatant _target;

	protected int _count;

	private bool _recalculatingAttackHand;

	protected Enemy attacker => (Enemy)_entity;

	protected ACombatant defender => _target;

	public GameStepTurnEnemyPrepareAttack(Enemy enemy, ACombatant target = null)
		: base(enemy)
	{
		if (_target == null)
		{
			_target = base.state.player;
		}
	}

	private void _RecalculateAttackHand()
	{
		if (_recalculatingAttackHand)
		{
			return;
		}
		_recalculatingAttackHand = true;
		using (attacker.resourceDeck.TransferPileReturn(ResourceCard.Pile.AttackHand, ResourceCard.Pile.Hand))
		{
			foreach (ResourceCard item in attacker.GetAttackCombatHand(ResourceCard.Pile.Hand, defender, attacker.resourceDeck.GetCards(ResourceCard.Pile.Hand)).hand)
			{
				attacker.resourceDeck.Transfer(item, ResourceCard.Pile.AttackHand);
			}
			_recalculatingAttackHand = false;
		}
	}

	private void _OnResourceTransfer(ResourceCard card, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
	{
		if ((ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(oldPile) || (ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(newPile))
		{
			if (attacker.resourceDeck.Count(ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand) == 0)
			{
				base.state.activeCombat.canceled = true;
			}
			else
			{
				_RecalculateAttackHand();
			}
		}
	}

	private void _OnWildValueChange(ResourceCard card, ResourceCard.WildContext wildContext)
	{
		if ((ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(card.pile) && card.faction == Faction.Enemy)
		{
			_RecalculateAttackHand();
		}
	}

	private void _OnDragBegin(ACardLayout layout, CardLayoutElement card)
	{
		if (card.card == attacker)
		{
			_StopDisplayingEffectiveEnemyOffense(attacker);
		}
	}

	public override void StopDisplayingEffectiveStats()
	{
		_StopDisplayingEffectiveEnemyOffense(attacker);
	}

	private void _OnDragEnd(ACardLayout layout, CardLayoutElement card)
	{
		if (card.card == attacker)
		{
			_DisplayEffectiveEnemyOffense(attacker);
		}
	}

	public override void DisplayEffectiveStats()
	{
		_DisplayEffectiveEnemyOffense(attacker);
	}

	private void _OnTapped(bool tapped)
	{
		if (tapped)
		{
			ActiveCombat activeCombat = base.state.activeCombat;
			if (activeCombat != null && !activeCombat.defenseHasBeenLaunched)
			{
				base.state.activeCombat.canceled = true;
			}
		}
	}

	public override void Start()
	{
		ACardLayout.OnDragBegan += _OnDragBegin;
		ACardLayout.OnDragEnded += _OnDragEnd;
		base.state.activeCombat = new ActiveCombat(attacker, _target);
		_DisplayEffectiveEnemyOffense(attacker);
		_count = attacker.GetOffenseAgainst(defender, shouldTriggerMedia: true);
	}

	protected override IEnumerator Update()
	{
		yield return AppendStep(attacker.resourceDeck.DrawStep(_count));
		foreach (ResourceCard item in attacker.GetAttackCombatHand(ResourceCard.Pile.Hand, defender, attacker.resourceDeck.GetCards(ResourceCard.Pile.Hand)).hand)
		{
			attacker.resourceDeck.Transfer(item, ResourceCard.Pile.AttackHand);
		}
		attacker.resourceDeck.onTransfer += _OnResourceTransfer;
		base.state.onWildValueChanged += _OnWildValueChange;
		attacker.tapped.onValueChanged += _OnTapped;
		yield return TransitionTo(new GameStepGroupDynamic(new GameStepLaunchAttack()));
	}

	protected override void OnDestroy()
	{
		ACardLayout.OnDragBegan -= _OnDragBegin;
		ACardLayout.OnDragEnded -= _OnDragEnd;
		attacker.resourceDeck.onTransfer -= _OnResourceTransfer;
		base.state.onWildValueChanged -= _OnWildValueChange;
		attacker.tapped.onValueChanged -= _OnTapped;
		base.state.activeCombat = null;
		PoolKeepItemListHandle<ResourceCardView> componentsInChildrenPooled = attacker.resourceDeck.Layout<ResourceDeckLayout>().combatHand.gameObject.GetComponentsInChildrenPooled<ResourceCardView>();
		if (componentsInChildrenPooled.Count > 0)
		{
			((CardStackLayout)base.view.enemyResourceDeckLayout.GetLayout(ResourceCard.Pile.DiscardPile)).dragCount = componentsInChildrenPooled.Count;
		}
		foreach (ResourceCardView item in componentsInChildrenPooled)
		{
			attacker.resourceDeck.Discard(item.resourceCard);
		}
		_StopDisplayingEffectiveEnemyOffense(attacker);
	}
}
