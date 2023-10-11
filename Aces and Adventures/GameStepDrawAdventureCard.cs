public class GameStepDrawAdventureCard : GameStep
{
	private AdventureCard.Pile? _drawTo;

	private ATarget _cardToDraw;

	public GameStepDrawAdventureCard(AdventureCard.Pile? drawTo = null)
	{
		_drawTo = drawTo;
	}

	protected override void OnFirstEnabled()
	{
		if (!((_cardToDraw = base.state.adventureDeck.NextInPile()) is IAdventureCard adventureCard) || adventureCard.adventureCardCommon == null)
		{
			return;
		}
		foreach (AdventureCard.SelectInstruction aboutToDrawInstruction in adventureCard.adventureCardCommon.aboutToDrawInstructions)
		{
			foreach (GameStep gameStep in aboutToDrawInstruction.GetGameSteps(base.state))
			{
				AppendStep(gameStep);
			}
		}
		adventureCard.adventureCardCommon.ClearAboutToDrawInstructions();
	}

	public override void Start()
	{
		if (base.state.adventureDeck.NextInPile() == _cardToDraw)
		{
			IdDeck<AdventureCard.Pile, ATarget> adventureDeck = base.state.adventureDeck;
			AdventureCard.Pile? drawTo = _drawTo ?? (_cardToDraw as IAdventureCard)?.pileToTransferToOnDraw;
			AppendStep(adventureDeck.DrawStep(1, null, drawTo));
		}
	}
}
