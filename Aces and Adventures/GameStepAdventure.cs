using System.Collections;
using System.Linq;

public class GameStepAdventure : GameStepAdventureNarrationLoop
{
	private class GameStepDrawTrumpCard : GameStep
	{
		private Ability _trumpCard;

		public GameStepDrawTrumpCard(Ability trumpCard)
		{
			_trumpCard = trumpCard;
		}

		protected override IEnumerator Update()
		{
			base.state.adventureDeck.Transfer(_trumpCard, AdventureCard.Pile.SelectionHand);
			foreach (float item in Wait(0.3f))
			{
				_ = item;
				yield return null;
			}
			base.state.abilityDeck.Transfer(_trumpCard, Ability.Pile.HeroAct);
		}
	}

	private GameStep _timerStep;

	protected override bool _saveState => true;

	protected override void Awake()
	{
		_timerStep = base.state.stack.ParallelProcess(new GameStepAdventureTimer());
	}

	public override void Start()
	{
		if (base.state.parameters.adventureStarted)
		{
			return;
		}
		base.state.parameters.adventureStarted = true;
		Ability ability = base.state.heroDeck.GetCards().FirstOrDefault((Ability d) => d.data.category == AbilityData.Category.TrumpCard);
		if (ability != null)
		{
			base.state.stack.Append(new GameStepDrawTrumpCard(ability));
		}
		foreach (AdventureCard.SelectInstruction adventureStartInstruction in base.state.adventure.data.adventureStartInstructions)
		{
			foreach (GameStep gameStep in adventureStartInstruction.GetGameSteps(base.state))
			{
				base.state.stack.Append(gameStep);
			}
		}
		if (base.state.adventure.data.adventureStartInstructions.None((AdventureCard.SelectInstruction instruction) => instruction is AdventureCard.SelectInstruction.SetLighting))
		{
			base.state.stack.Append(GameStepLighting.Create(ContentRef.Defaults.lighting.adventure.DataOrDefault()));
		}
		if ((bool)base.state.modifierNode)
		{
			foreach (GameStep step in base.state.modifierNode.data.onSelectInstructions.GetSteps(base.state))
			{
				base.state.stack.Append(step);
			}
		}
		for (int i = 0; i < base.state.parameters.startingAbilityCount; i++)
		{
			base.state.stack.Append(base.state.abilityDeck.DrawStep());
		}
		if (base.state.parameters.mulliganCount > 0)
		{
			base.state.stack.Append(base.state.playerResourceDeck.DrawToSizeStep(base.state.player.playerStats[PlayerStatType.ResourceHandSize], null, null, null, 0f));
			base.state.stack.Append(new GameStepWait(0.2f, null, canSkip: false));
			base.state.stack.Append(new GameStepDiscardResourceChoice(base.state.parameters.mulliganCount, DiscardReason.Mulligan));
		}
		else
		{
			base.state.stack.Append(base.state.playerResourceDeck.DrawToSizeStep(base.state.player.playerStats[PlayerStatType.ResourceHandSize]));
		}
		for (int j = 0; j < base.state.parameters.initialLevelUpAmount; j++)
		{
			base.state.stack.Append(new GameStepLevelUp());
		}
		foreach (AdventureCard.SelectInstruction item in base.state.adventure.data.adventureStartInstructionsLate)
		{
			foreach (GameStep gameStep2 in item.GetGameSteps(base.state))
			{
				base.state.stack.Append(gameStep2);
			}
		}
	}

	protected override void OnFinish()
	{
		_timerStep.finished = true;
	}

	protected override void End()
	{
		base.state.parameters.adventureEnded = true;
		AppendStep(new GameStepAdventureShowResults());
	}

	protected override void OnDestroy()
	{
		base.state.parameters.adventureEnded = true;
	}
}
