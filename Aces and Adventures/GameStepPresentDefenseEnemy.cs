using System.Collections;
using System.Linq;
using UnityEngine;

public class GameStepPresentDefenseEnemy : AGameStepCombat
{
	private int _count;

	private bool _calculatingDefenseHand;

	private void _CalculateDefenseHand()
	{
		if (_calculatingDefenseHand)
		{
			return;
		}
		_calculatingDefenseHand = true;
		base.defender.resourceDeck.TransferPile(ResourceCard.Pile.DefenseHand, ResourceCard.Pile.Hand);
		(PoolKeepItemListHandle<ResourceCard>, PokerHandType) defenseCombatHand = base.defender.GetDefenseCombatHand(ResourceCard.Pile.Hand, base.attacker, null, base.defender.resourceDeck.GetCards(ResourceCard.Pile.Hand));
		if ((bool)defenseCombatHand.Item1)
		{
			foreach (ResourceCard item in defenseCombatHand.Item1)
			{
				base.defender.resourceDeck.Transfer(item, ResourceCard.Pile.DefenseHand);
			}
		}
		else
		{
			foreach (ResourceCard item2 in base.defender.resourceDeck.GetCardsSafe(ResourceCard.Pile.Hand).AsEnumerable().Take(base.attacker.resourceDeck.Count(ResourceCard.Pile.AttackHand)))
			{
				base.defender.resourceDeck.Transfer(item2, ResourceCard.Pile.DefenseHand);
			}
		}
		_calculatingDefenseHand = false;
		_UpdateGlowsAndLines();
	}

	private void _UpdateGlowsAndLines()
	{
		base.view.ClearPersistentAttackGlowsAndLines();
		PokerHand attackHand = base.attacker.GetAttackHand(base.defender);
		DefenseResultType defenseType = base.combat.resultOverride?.GetDefenseResultType() ?? base.defender.CanFormDefense(base.defender.resourceDeck.GetCards(ResourceCard.Pile.DefenseHand), base.attacker, attackHand, base.defender.resourceDeck.GetCards(ResourceCard.Pile.DefenseHand));
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = base.defender.GetDefenseCombatHand(ResourceCard.Pile.DefenseHand, base.attacker, attackHand, base.defender.resourceDeck.GetCards(ResourceCard.Pile.DefenseHand)).hand;
		foreach (ResourceCard card in base.defender.resourceDeck.GetCards(ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand))
		{
			card.view.offsets.Clear();
		}
		foreach (ResourceCard card2 in base.defender.resourceDeck.GetCards(ResourceCard.Pile.DefenseHand))
		{
			card2.view.RequestGlow(this, defenseType.GetTint(), GlowTags.Persistent | GlowTags.Attack);
			AGameStepTurn.AddEnemyAttackTargetLine(this, card2, base.attacker, defenseType.GetTint(), TargetLineTags.Persistent);
			if (base.defender.resourceDeck.Count(ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand) > base.defender.resourceDeck.Count(ResourceCard.Pile.DefenseHand) && poolKeepItemListHandle?.Count == base.defender.resourceDeck.Count(ResourceCard.Pile.DefenseHand))
			{
				card2.view.offsets.Add(Matrix4x4.Translate(base.view.enemyResourceDeckLayout.combatHand.transform.forward * -0.008f));
			}
		}
		foreach (ResourceCard item in base.attacker.GetAttackCombatHand(ResourceCard.Pile.AttackHand, base.defender, base.attacker.resourceDeck.GetCards(ResourceCard.Pile.AttackHand)).hand)
		{
			item.view.RequestGlow(this, defenseType.Opposite().GetTint(), GlowTags.Persistent | GlowTags.Attack);
			AGameStepTurn.AddAttackTargetLine(this, item, base.defender, defenseType.Opposite().GetTint(), null, null, 1f, 1f, null, 1f, TargetLineTags.Persistent);
		}
	}

	private void _OnResourceTransfer(ResourceCard card, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
	{
		if ((ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(oldPile) || (ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(newPile))
		{
			_CalculateDefenseHand();
		}
	}

	private void _OnWildValueChange(ResourceCard card, ResourceCard.WildContext wildContext)
	{
		if ((card.faction == Faction.Enemy && (ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(card.pile)) || (card.faction == Faction.Player && card.pile == ResourceCard.Pile.AttackHand))
		{
			_CalculateDefenseHand();
		}
	}

	private void _OnWildsChange(ResourceCard card)
	{
		if (card.pile == ResourceCard.Pile.AttackHand && card.faction == Faction.Player)
		{
			base.combat.playerAttackWildedAfterLaunch = true;
		}
		else if (card.faction == Faction.Enemy && (ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(card.pile))
		{
			base.combat.enemyDefenseWildedAfterLaunch = true;
		}
	}

	protected override void OnFirstEnabled()
	{
		base.state.SignalDefensePresent();
	}

	public override void Start()
	{
		_count = base.defender.GetDefenseAgainst(base.attacker, shouldTriggerMedia: true);
	}

	protected override IEnumerator Update()
	{
		yield return AppendStep(base.defender.resourceDeck.DrawStep(_count));
		_CalculateDefenseHand();
		base.defender.resourceDeck.onTransfer += _OnResourceTransfer;
		base.state.onWildValueChanged += _OnWildValueChange;
		base.state.onWildsChanged += _OnWildsChange;
		base.state.SignalDefenseLaunched();
		if (base.defender.resourceDeck.Count(ResourceCard.Pile.DefenseHand) > 0)
		{
			yield return AppendStep(new GameStepWait(2f));
		}
		yield return AppendStep(new GameStepDecideCombatVictor());
	}

	protected override void OnDestroy()
	{
		base.defender.resourceDeck.onTransfer -= _OnResourceTransfer;
		base.state.onWildValueChanged -= _OnWildValueChange;
		base.state.onWildsChanged -= _OnWildsChange;
		PoolKeepItemListHandle<ResourceCardView> componentsInChildrenPooled = base.defender.resourceDeck.Layout<ResourceDeckLayout>().combatHand.gameObject.GetComponentsInChildrenPooled<ResourceCardView>();
		if (componentsInChildrenPooled.Count > 0)
		{
			((CardStackLayout)base.view.enemyResourceDeckLayout.GetLayout(ResourceCard.Pile.DiscardPile)).dragCount = componentsInChildrenPooled.Count;
		}
		foreach (ResourceCardView item in componentsInChildrenPooled)
		{
			base.defender.resourceDeck.Discard(item.resourceCard);
		}
		base.view.ClearPersistentAttackGlowsAndLines();
	}
}
