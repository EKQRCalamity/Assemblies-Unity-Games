using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField("Attack Damage", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class SuccessfulAttackAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIDeepValueChange]
	private DynamicNumber _baseAttackDamage;

	public override bool dealsDamage => true;

	private ActiveCombat _CreateCombat(ActionContext context, ACombatant combatant)
	{
		return new ActiveCombat(context.GetTarget<ACombatant>(ActionContextTarget.Owner), combatant)
		{
			damage = _baseAttackDamage.GetValue(context),
			attackHasBeenLaunched = true,
			defenseHasBeenLaunched = true,
			resultOverride = AttackResultType.Success,
			action = this,
			ability = context.ability,
			resultIsFinal = true
		};
	}

	public override IEnumerable<AbilityKeyword> GetKeywords(AbilityData abilityData)
	{
		foreach (AbilityKeyword keyword in base.GetKeywords(abilityData))
		{
			yield return keyword;
		}
		yield return AbilityKeyword.AttackDamage;
	}

	protected override void _Tick(ActionContext context, ACombatant combatant)
	{
		GameState state = context.gameState;
		ActiveCombat tempActiveCombat = state.activeCombat;
		ActiveCombat combat = _CreateCombat(context, combatant);
		PoolKeepItemListHandle<ResourceCard> combatHand = null;
		PoolKeepItemListHandle<ResourceCard> activationHand = null;
		state.stack.activeStep.AppendStep(new GameStepGeneric
		{
			onStart = delegate
			{
				combatHand = combat.attacker.resourceDeck.TransferPileReturn(ResourceCard.Pile.AttackHand, ResourceCard.Pile.ActivationHandWaiting);
				activationHand = combat.attacker.resourceDeck.TransferPileReturn(ResourceCard.Pile.ActivationHand, ResourceCard.Pile.AttackHand);
				state.activeCombat = combat;
			}
		});
		state.stack.activeStep.AppendStep(new GameStepDealCombatDamage
		{
			waitTime = 0f
		});
		state.stack.activeStep.AppendStep(new GameStepGeneric
		{
			onStart = delegate
			{
				state.activeCombat = tempActiveCombat;
				foreach (ResourceCard item in combatHand)
				{
					combat.attacker.resourceDeck.Transfer(item, ResourceCard.Pile.AttackHand);
				}
				foreach (ResourceCard item2 in activationHand)
				{
					combat.attacker.resourceDeck.Transfer(item2, ResourceCard.Pile.ActivationHand);
				}
			}
		});
	}

	protected override string _ToStringUnique()
	{
		return $"Deal {_baseAttackDamage} <i>attack</i> damage to";
	}

	public override int GetPotentialDamage(ActionContext context)
	{
		if (!(context.target is ACombatant combatant))
		{
			return 0;
		}
		GameState gameState = context.gameState;
		ActiveCombat activeCombat = gameState.activeCombat;
		ActiveCombat activeCombat3 = (gameState.activeCombat = _CreateCombat(context, combatant));
		IdDeck<ResourceCard.Pile, ResourceCard> resourceDeck = activeCombat3.attacker.resourceDeck;
		int suppressEvents = resourceDeck.suppressEvents + 1;
		resourceDeck.suppressEvents = suppressEvents;
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = activeCombat3.attacker.resourceDeck.TransferPileReturn(ResourceCard.Pile.AttackHand, ResourceCard.Pile.ActivationHandWaiting);
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle2 = activeCombat3.attacker.resourceDeck.TransferPileReturn(ResourceCard.Pile.ActivationHand, ResourceCard.Pile.AttackHand);
		int potentialAttackDamage = activeCombat3.potentialAttackDamage;
		foreach (ResourceCard item in poolKeepItemListHandle.value)
		{
			activeCombat3.attacker.resourceDeck.Transfer(item, ResourceCard.Pile.AttackHand);
		}
		foreach (ResourceCard item2 in poolKeepItemListHandle2.value)
		{
			activeCombat3.attacker.resourceDeck.Transfer(item2, ResourceCard.Pile.ActivationHand);
		}
		IdDeck<ResourceCard.Pile, ResourceCard> resourceDeck2 = activeCombat3.attacker.resourceDeck;
		suppressEvents = resourceDeck2.suppressEvents - 1;
		resourceDeck2.suppressEvents = suppressEvents;
		gameState.activeCombat = activeCombat;
		return potentialAttackDamage;
	}
}
