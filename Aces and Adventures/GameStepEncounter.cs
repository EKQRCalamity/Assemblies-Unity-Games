using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameStepEncounter : GameStep
{
	public static void SetEntityTakingTurnOffsets(AEntity entityTakingTurn)
	{
		entityTakingTurn.view.offsets.Add(AGameStepTurn.OFFSET);
	}

	protected override void OnFirstEnabled()
	{
		if (!base.state.encounterActive)
		{
			Enemy enemy = base.state.adventureDeck.GetCards(AdventureCard.Pile.TurnOrder).OfType<Enemy>().FirstOrDefault();
			if (enemy != null)
			{
				AppendStep(new GameStepSound(enemy.audio.encounterStart.GetSound(base.state.cosmeticRandom)?.audioRef, 1f));
			}
		}
	}

	protected override void OnEnable()
	{
		if (base.hasStarted)
		{
			EncounterState? encounterState = base.state.GetEncounterState();
			if (encounterState != EncounterState.Active)
			{
				TransitionTo((encounterState == EncounterState.Victory) ? ((GameStep)new GameStepEncounterEndVictory()) : ((GameStep)new GameStepEncounterEndDefeat()));
			}
		}
	}

	public override void Start()
	{
		base.view.OffsetTurnOrderForCombat(offset: true);
	}

	protected override IEnumerator Update()
	{
		if (!base.state.encounterActive)
		{
			GameState gameState = base.state;
			int encounterNumber = gameState.encounterNumber + 1;
			gameState.encounterNumber = encounterNumber;
			base.state.EncounterStartStrategicTime();
			base.state.roundNumber = 1;
			foreach (AEntity item in base.state.turnOrderQueue)
			{
				item.OnEncounterStart();
			}
			base.state.SignalEncounterStart();
			if (!base.isActiveStep)
			{
				yield return null;
			}
			base.state.GoToNextRound(incrementRound: false);
		}
		while (base.state.encounterActive)
		{
			if (base.state.SignalEndTurn() && !base.isActiveStep)
			{
				yield return null;
			}
			if (base.state.SignalEndRound())
			{
				if (!base.isActiveStep)
				{
					yield return null;
				}
				base.state.GoToNextRound();
				if (!base.isActiveStep)
				{
					yield return null;
				}
			}
			base.state.CheckForEnemiesThatShouldBeDead();
			if (!base.isActiveStep)
			{
				yield return null;
			}
			AEntity nextEntity = base.state.GetNextEntityInTurnOrder();
			SetEntityTakingTurnOffsets(nextEntity);
			base.state.stoneDeck.Layout<StoneDeckLayout>().Transfer(StoneType.Turn, (nextEntity.faction == Faction.Enemy) ? Stone.Pile.EnemyTurn : Stone.Pile.PlayerTurn);
			if (nextEntity is ACombatant aCombatant)
			{
				VoiceManager.Instance.Play(aCombatant.view.transform, aCombatant.audio.turnStart, interrupt: false, 2f);
			}
			foreach (float item2 in Wait(1f / 3f))
			{
				_ = item2;
				yield return null;
			}
			base.state.entityTakingTurn = nextEntity;
			base.state.SignalTurnStartEarly(nextEntity);
			if (!base.isActiveStep)
			{
				yield return null;
			}
			nextEntity.OnTurnStart();
			if (nextEntity.canAct)
			{
				yield return AppendStep(new GameStepSaveGameState());
			}
			yield return AppendStep(nextEntity.GetTurnStep());
		}
	}

	protected override void OnDestroy()
	{
		EncounterState? encounterState = base.state.GetEncounterState();
		base.view.OffsetTurnOrderForCombat(offset: false);
		base.state.roundNumber = 0;
		base.state.EncounterEndStrategicTime();
		EncounterCard encounterCard = base.state.adventureDeck.GetCards(AdventureCard.Pile.ActiveHand).OfType<EncounterCard>().FirstOrDefault();
		base.state.adventureDeck.TransferPile(AdventureCard.Pile.ActiveHand, AdventureCard.Pile.Discard);
		foreach (AEntity item in base.state.turnOrderQueue)
		{
			item.Untap();
		}
		base.state.player.OnEncounterEnd();
		foreach (Ability item2 in base.state.player.abilityDeck.GetCardsSafe(Ability.Piles.HeroAct | Ability.Piles.HeroPassive))
		{
			if (item2 is ItemCard itemCard && itemCard.isEncounterAbility)
			{
				base.state.adventureDeck.Discard(itemCard.Unapply());
			}
		}
		List<AdventureCard.SelectInstruction> list = encounterCard?.onCompletedInstructions;
		if (list == null || list.Count <= 0 || base.canceled || encounterState != EncounterState.Victory)
		{
			return;
		}
		base.state.stack.Push(new GameStepSaveGameState(list));
		foreach (GameStep step in list.GetSteps(base.state))
		{
			base.state.stack.Push(step);
		}
	}
}
