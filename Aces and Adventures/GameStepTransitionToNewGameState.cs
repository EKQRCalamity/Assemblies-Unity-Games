public class GameStepTransitionToNewGameState : GameStep
{
	private DataRef<AdventureData> _selectedAdventure;

	public GameStepTransitionToNewGameState(DataRef<AdventureData> selectedAdventure = null)
	{
		_selectedAdventure = selectedAdventure;
	}

	public override void Start()
	{
		base.state.Destroy();
		GameStateView.Instance.state = GameState.BeginAdventureSelection(_selectedAdventure);
	}
}
